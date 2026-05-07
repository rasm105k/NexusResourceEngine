using NexusResourceEngine.Application.DTOs.Transitions;

namespace NexusResourceEngine.Application.Interfaces;

public interface IStateTransitionService
{
    Task<StateTransitionDto> CreateAsync(CreateStateTransitionDto dto, Guid tenantId);
}
