using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NexusResourceEngine.Application.DTOs.Auth;
using NexusResourceEngine.Application.Interfaces;
using NexusResourceEngine.Domain;
using NexusResourceEngine.Infrastructure.Auth;
using NexusResourceEngine.Infrastructure.Data;

namespace NexusResourceEngine.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly NexusResourceEngineContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(NexusResourceEngineContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request, Guid tenantId)
    {
        var tenant = new Tenant
        {
            TenantId = tenantId,
            OrganizationName = request.OrganizationName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tenants.Add(tenant);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            TenantId = tenantId,
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(null!, request.Password),
            Role = "Admin"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return GenerateToken(user, tenantId);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(null!, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return GenerateToken(user, user.TenantId);
    }

    private LoginResponseDto GenerateToken(User user, Guid tenantId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim("TenantId", tenantId.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = user.UserId,
            Role = user.Role
        };
    }
}
