namespace NexusResourceEngine.Application.DTOs.States;

public class UpdateResourceStateDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsBookable { get; set; }
    public string ColorCode { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
