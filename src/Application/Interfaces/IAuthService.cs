using NexusResourceEngine.Application.DTOs.Auth;

namespace NexusResourceEngine.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request, Guid tenantId);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}
