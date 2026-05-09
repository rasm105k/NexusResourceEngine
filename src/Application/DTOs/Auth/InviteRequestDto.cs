namespace NexusResourceEngine.Application.DTOs.Auth;

public class InviteRequestDto
{
    public string RedirectUrl { get; set; } = string.Empty;
    public List<InviteeDto> Invitees { get; set; } = [];
}
