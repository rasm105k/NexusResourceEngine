using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Application.DTOs.Resources;
using NexusResourceEngine.Application.Interfaces;
using NexusResourceEngine.Application.Mapping;
using NexusResourceEngine.Infrastructure.Data;

namespace NexusResourceEngine.Infrastructure.Services;

public class ResourceService : IResourceService
{
    private readonly NexusResourceEngineContext _context;

    public ResourceService(NexusResourceEngineContext context)
    {
        _context = context;
    }

    public async Task<List<ResourceDto>> GetAllAsync(Guid tenantId, Guid? stateId = null)
    {
        var query = _context.Resources.Where(r => r.TenantId == tenantId);

        if (stateId.HasValue)
            query = query.Where(r => r.CurrentStateId == stateId.Value);

        return await query
            .Select(r => r.ToDto())
            .ToListAsync();
    }

    public async Task<ResourceDto> CreateAsync(CreateResourceDto dto, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Resource name is required.");

        if (!dto.CurrentStateId.HasValue || dto.CurrentStateId.Value == Guid.Empty)
            throw new InvalidOperationException("CurrentStateId is required.");

        var stateExists = await _context.ResourceStates
            .AnyAsync(rs => rs.StateId == dto.CurrentStateId && rs.TenantId == tenantId);

        if (!stateExists)
            throw new InvalidOperationException("CurrentStateId does not reference an existing state for this tenant.");

        var entity = dto.ToEntity(tenantId);
        _context.Resources.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToDto();
    }

    public async Task<ResourceDto> ChangeStateAsync(Guid resourceId, UpdateResourceStateDto dto, Guid tenantId, string currentUserRole)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.ResourceId == resourceId && r.TenantId == tenantId)
            ?? throw new InvalidOperationException("Resource not found.");

        if (dto.NewStateId == Guid.Empty)
            throw new InvalidOperationException("NewStateId is required.");

        var stateExists = await _context.ResourceStates
            .AnyAsync(rs => rs.StateId == dto.NewStateId && rs.TenantId == tenantId);

        if (!stateExists)
            throw new InvalidOperationException("NewStateId does not reference an existing state for this tenant.");

        var transition = await _context.StateTransitions
            .FirstOrDefaultAsync(st =>
                st.FromStateId == resource.CurrentStateId &&
                st.ToStateId == dto.NewStateId &&
                st.TenantId == tenantId);

        if (transition is null)
            throw new InvalidOperationException("No valid transition exists between the current state and the target state.");

        if (currentUserRole != "Admin" && transition.RequiredRole != currentUserRole)
            throw new UnauthorizedAccessException("Your role does not have permission to perform this state transition.");

        resource.CurrentStateId = dto.NewStateId;
        await _context.SaveChangesAsync();

        return resource.ToDto();
    }
}
