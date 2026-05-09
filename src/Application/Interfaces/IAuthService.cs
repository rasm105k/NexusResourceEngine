using NexusResourceEngine.Application.DTOs.Auth;

namespace NexusResourceEngine.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request, Guid tenantId);
    Task<LoginResponseDto> RegisterMemberAsync(RegisterMemberDto request, Guid tenantId);
    Task<InviteResponseDto> InviteAsync(InviteRequestDto request, Guid tenantId);
    Task<LoginResponseDto> AcceptInviteAsync(AcceptInviteDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}
