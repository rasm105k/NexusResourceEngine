namespace NexusResourceEngine.Application.DTOs.Bookings;

public class BookingDto
{
    public Guid BookingId { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
