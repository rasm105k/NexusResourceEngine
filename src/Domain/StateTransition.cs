namespace NexusResourceEngine.Domain;

public class StateTransition
{
    public Guid TransitionId { get; set; }
    public Guid TenantId { get; set; }
    public Guid FromStateId { get; set; }
    public Guid ToStateId { get; set; }
    public string RequiredRole { get; set; } = string.Empty;
}
