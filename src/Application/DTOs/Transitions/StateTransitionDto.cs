namespace NexusResourceEngine.Application.DTOs.Transitions;

public class StateTransitionDto
{
    public Guid TransitionId { get; set; }
    public Guid FromStateId { get; set; }
    public Guid ToStateId { get; set; }
    public string RequiredRole { get; set; } = string.Empty;
}
