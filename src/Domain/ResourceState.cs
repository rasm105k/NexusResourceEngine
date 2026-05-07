namespace NexusResourceEngine.Domain;

public class ResourceState
{
    public Guid StateId { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBookable { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
