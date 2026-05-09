using NexusResourceEngine.Application.DTOs.Resources;

namespace NexusResourceEngine.Application.Interfaces;

public interface IResourceService
{
    Task<List<ResourceDto>> GetAllAsync(Guid tenantId, Guid? stateId = null);
    Task<ResourceDto> CreateAsync(CreateResourceDto dto, Guid tenantId);
    Task<ResourceDto> ChangeStateAsync(Guid resourceId, UpdateResourceStateDto dto, Guid tenantId, string currentUserRole);
}
