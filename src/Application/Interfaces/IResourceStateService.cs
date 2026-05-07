using NexusResourceEngine.Application.DTOs.States;

namespace NexusResourceEngine.Application.Interfaces;

public interface IResourceStateService
{
    Task<List<ResourceStateDto>> GetAllAsync(Guid tenantId);
    Task<ResourceStateDto> CreateAsync(CreateResourceStateDto dto, Guid tenantId);
}
