using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Application.DTOs.Transitions;
using NexusResourceEngine.Application.Interfaces;
using NexusResourceEngine.Application.Mapping;
using NexusResourceEngine.Infrastructure.Data;

namespace NexusResourceEngine.Infrastructure.Services;

public class StateTransitionService : IStateTransitionService
{
    private readonly NexusResourceEngineContext _context;

    public StateTransitionService(NexusResourceEngineContext context)
    {
        _context = context;
    }

    public async Task<StateTransitionDto> CreateAsync(CreateStateTransitionDto dto, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(dto.RequiredRole))
            throw new InvalidOperationException("Required role is required.");

        var fromStateExists = await _context.ResourceStates
            .AnyAsync(rs => rs.StateId == dto.FromStateId && rs.TenantId == tenantId);

        if (!fromStateExists)
            throw new InvalidOperationException($"FromStateId '{dto.FromStateId}' does not reference an existing state for this tenant.");

        var toStateExists = await _context.ResourceStates
            .AnyAsync(rs => rs.StateId == dto.ToStateId && rs.TenantId == tenantId);

        if (!toStateExists)
            throw new InvalidOperationException($"ToStateId '{dto.ToStateId}' does not reference an existing state for this tenant.");

        var duplicateExists = await _context.StateTransitions
            .AnyAsync(st => st.FromStateId == dto.FromStateId && st.ToStateId == dto.ToStateId && st.TenantId == tenantId);

        if (duplicateExists)
            throw new InvalidOperationException("A transition between these states already exists for this tenant.");

        var entity = dto.ToEntity(tenantId);
        _context.StateTransitions.Add(entity);
        await _context.SaveChangesAsync();

        return entity.ToDto();
    }
}
