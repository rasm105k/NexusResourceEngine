using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Application.DTOs.Users;
using NexusResourceEngine.Application.Interfaces;
using NexusResourceEngine.Application.Mapping;
using NexusResourceEngine.Infrastructure.Data;

namespace NexusResourceEngine.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly NexusResourceEngineContext _context;

    public UserService(NexusResourceEngineContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllAsync(Guid tenantId)
    {
        return await _context.Users
            .Where(u => u.TenantId == tenantId)
            .Select(u => u.ToDto())
            .ToListAsync();
    }

    public async Task<UserDto> UpdateRoleAsync(Guid userId, UpdateUserRoleDto dto, Guid tenantId, Guid currentUserId)
    {
        if (userId == currentUserId)
            throw new InvalidOperationException("Cannot change your own role.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.TenantId == tenantId)
            ?? throw new InvalidOperationException("User not found.");

        if (dto.Role != "Admin" && dto.Role != "Member")
            throw new InvalidOperationException("Role must be 'Admin' or 'Member'.");

        user.Role = dto.Role;
        await _context.SaveChangesAsync();

        return user.ToDto();
    }
}
