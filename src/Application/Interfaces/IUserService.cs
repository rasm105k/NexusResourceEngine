using NexusResourceEngine.Application.DTOs.Users;

namespace NexusResourceEngine.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(Guid tenantId);
    Task<UserDto> UpdateRoleAsync(Guid userId, UpdateUserRoleDto dto, Guid tenantId, Guid currentUserId);
}
