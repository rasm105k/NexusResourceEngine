namespace NexusResourceEngine.Domain;

public class Booking
{
    public Guid BookingId { get; set; }
    public Guid TenantId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
