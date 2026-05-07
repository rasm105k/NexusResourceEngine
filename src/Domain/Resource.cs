namespace NexusResourceEngine.Domain;

public class Resource
{
    public Guid ResourceId { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CurrentStateId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Metadata { get; set; }
}
