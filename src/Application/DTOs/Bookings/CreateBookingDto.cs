namespace NexusResourceEngine.Application.DTOs.Bookings;

public class CreateBookingDto
{
    public Guid ResourceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
