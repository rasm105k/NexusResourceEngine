namespace NexusResourceEngine.Application.DTOs.Transitions;

public class CreateStateTransitionDto
{
    public Guid FromStateId { get; set; }
    public Guid ToStateId { get; set; }
    public string RequiredRole { get; set; } = string.Empty;
}
