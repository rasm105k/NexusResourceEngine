namespace NexusResourceEngine.Domain;

public class Tenant
{
    public Guid TenantId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
