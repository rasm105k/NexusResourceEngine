using NexusResourceEngine.Application.DTOs.Bookings;

namespace NexusResourceEngine.Application.Interfaces;

public interface IBookingService
{
    Task<bool> CheckAvailabilityAsync(Guid resourceId, DateTime start, DateTime end, Guid tenantId);
    Task<BookingDto> CreateAsync(CreateBookingDto dto, Guid userId, Guid tenantId);
    Task<BookingDto> UpdateStatusAsync(Guid bookingId, UpdateBookingStatusDto dto, Guid tenantId);
}
