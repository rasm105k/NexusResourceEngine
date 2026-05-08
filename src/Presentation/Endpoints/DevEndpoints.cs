using Microsoft.AspNetCore.Identity;
using NexusResourceEngine.Application.DTOs.Auth;
using NexusResourceEngine.Application.DTOs.States;
using NexusResourceEngine.Application.DTOs.Transitions;
using NexusResourceEngine.Application.Interfaces;
using NexusResourceEngine.Domain;
using NexusResourceEngine.Infrastructure.Auth;
using NexusResourceEngine.Infrastructure.Data;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class DevEndpoints
{
    public static void MapDevEndpoints(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;

        var group = app.MapGroup("/dev");

        group.MapPost("/seed", async (NexusResourceEngineContext context, IOptions<JwtSettings> jwtSettings) =>
        {
            var tenantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var hasher = new PasswordHasher<User>();

            var tenant = new Tenant
            {
                TenantId = tenantId,
                OrganizationName = "Room Booking Demo",
                CreatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenant);

            var user = new User
            {
                UserId = userId,
                TenantId = tenantId,
                Username = "demo",
                Email = "demo@example.com",
                PasswordHash = hasher.HashPassword(null!, "Demo@123"),
                Role = "Admin"
            };
            context.Users.Add(user);

            var availableId = Guid.NewGuid();
            var bookedId = Guid.NewGuid();
            var occupiedId = Guid.NewGuid();
            var cleaningId = Guid.NewGuid();
            var maintenanceId = Guid.NewGuid();

            var states = new[]
            {
                new ResourceState { StateId = availableId, TenantId = tenantId, Name = "Available",   IsBookable = true,  ColorCode = "#28a745", SortOrder = 1 },
                new ResourceState { StateId = bookedId,    TenantId = tenantId, Name = "Booked",      IsBookable = false, ColorCode = "#ffc107", SortOrder = 2 },
                new ResourceState { StateId = occupiedId,  TenantId = tenantId, Name = "Occupied",    IsBookable = false, ColorCode = "#007bff", SortOrder = 3 },
                new ResourceState { StateId = cleaningId,  TenantId = tenantId, Name = "Cleaning",    IsBookable = false, ColorCode = "#6f42c1", SortOrder = 4 },
                new ResourceState { StateId = maintenanceId, TenantId = tenantId, Name = "Maintenance", IsBookable = false, ColorCode = "#dc3545", SortOrder = 5 }
            };
            context.ResourceStates.AddRange(states);

            var transitions = new[]
            {
                new StateTransition { TransitionId = Guid.NewGuid(), TenantId = tenantId, FromStateId = availableId,   ToStateId = bookedId,   RequiredRole = "Member" },
                new StateTransition { TransitionId = Guid.NewGuid(), TenantId = tenantId, FromStateId = bookedId,     ToStateId = occupiedId, RequiredRole = "Staff" },
                new StateTransition { TransitionId = Guid.NewGuid(), TenantId = tenantId, FromStateId = bookedId,     ToStateId = availableId, RequiredRole = "Member" },
                new StateTransition { TransitionId = Guid.NewGuid(), TenantId = tenantId, FromStateId = occupiedId,   ToStateId = cleaningId, RequiredRole = "Staff" },
                new StateTransition { TransitionId = Guid.NewGuid(), TenantId = tenantId, FromStateId = cleaningId,   ToStateId = availableId, RequiredRole = "Staff" },
                new StateTransition { TransitionId = Guid.NewGuid(), TenantId = tenantId, FromStateId = availableId,   ToStateId = maintenanceId, RequiredRole = "Admin" },
                new StateTransition { TransitionId = Guid.NewGuid(), TenantId = tenantId, FromStateId = maintenanceId, ToStateId = availableId, RequiredRole = "Admin" }
            };
            context.StateTransitions.AddRange(transitions);

            await context.SaveChangesAsync();

            var token = GenerateToken(user, tenantId, jwtSettings.Value);

            return Results.Ok(new
            {
                tenantId,
                email = "demo@example.com",
                password = "Demo@123",
                token,
                userId,
                role = "Admin",
                states = new
                {
                    available = availableId,
                    booked = bookedId,
                    occupied = occupiedId,
                    cleaning = cleaningId,
                    maintenance = maintenanceId
                }
            });
        });
    }

    private static string GenerateToken(User user, Guid tenantId, JwtSettings settings)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim("TenantId", tenantId.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
