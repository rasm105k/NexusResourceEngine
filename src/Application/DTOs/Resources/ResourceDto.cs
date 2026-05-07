namespace NexusResourceEngine.Application.DTOs.Resources;

public class ResourceDto
{
    public Guid ResourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CurrentStateId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Metadata { get; set; }
}
