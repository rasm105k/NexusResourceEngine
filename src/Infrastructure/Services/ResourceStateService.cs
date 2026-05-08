using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Application.DTOs.States;
using NexusResourceEngine.Application.Interfaces;
using NexusResourceEngine.Application.Mapping;
using NexusResourceEngine.Infrastructure.Data;

namespace NexusResourceEngine.Infrastructure.Services;

public class ResourceStateService : IResourceStateService
{
    private readonly NexusResourceEngineContext _context;

    public ResourceStateService(NexusResourceEngineContext context)
    {
        _context = context;
    }

    public async Task<List<ResourceStateDto>> GetAllAsync(Guid tenantId)
    {
        return await _context.ResourceStates
            .Where(rs => rs.TenantId == tenantId)
            .OrderBy(rs => rs.SortOrder)
            .Select(rs => rs.ToDto())
            .ToListAsync();
    }

    public async Task<ResourceStateDto> CreateAsync(CreateResourceStateDto dto, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("State name is required.");

        var entity = dto.ToEntity(tenantId);
        _context.ResourceStates.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToDto();
    }
}
