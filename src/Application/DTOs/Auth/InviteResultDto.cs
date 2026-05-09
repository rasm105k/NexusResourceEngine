namespace NexusResourceEngine.Application.DTOs.Auth;

public class InviteResultDto
{
    public string Email { get; set; } = string.Empty;
    public string InviteUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
