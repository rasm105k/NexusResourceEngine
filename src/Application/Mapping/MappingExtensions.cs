using NexusResourceEngine.Application.DTOs.Bookings;
using NexusResourceEngine.Application.DTOs.Resources;
using NexusResourceEngine.Application.DTOs.States;
using NexusResourceEngine.Application.DTOs.Transitions;
using NexusResourceEngine.Domain;

namespace NexusResourceEngine.Application.Mapping;

public static class MappingExtensions
{
    public static ResourceStateDto ToDto(this ResourceState state)
    {
        return new ResourceStateDto
        {
            StateId = state.StateId,
            Name = state.Name,
            IsBookable = state.IsBookable,
            ColorCode = state.ColorCode,
            SortOrder = state.SortOrder
        };
    }

    public static ResourceState ToEntity(this CreateResourceStateDto dto, Guid tenantId)
    {
        return new ResourceState
        {
            StateId = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.Name,
            IsBookable = dto.IsBookable,
            ColorCode = dto.ColorCode,
            SortOrder = dto.SortOrder
        };
    }

    public static StateTransitionDto ToDto(this StateTransition transition)
    {
        return new StateTransitionDto
        {
            TransitionId = transition.TransitionId,
            FromStateId = transition.FromStateId,
            ToStateId = transition.ToStateId,
            RequiredRole = transition.RequiredRole
        };
    }

    public static StateTransition ToEntity(this CreateStateTransitionDto dto, Guid tenantId)
    {
        return new StateTransition
        {
            TransitionId = Guid.NewGuid(),
            TenantId = tenantId,
            FromStateId = dto.FromStateId,
            ToStateId = dto.ToStateId,
            RequiredRole = dto.RequiredRole
        };
    }

    public static ResourceDto ToDto(this Resource resource)
    {
        return new ResourceDto
        {
            ResourceId = resource.ResourceId,
            Name = resource.Name,
            Description = resource.Description,
            CurrentStateId = resource.CurrentStateId,
            Latitude = resource.Latitude,
            Longitude = resource.Longitude,
            Metadata = resource.Metadata
        };
    }

    public static Resource ToEntity(this CreateResourceDto dto, Guid tenantId)
    {
        return new Resource
        {
            ResourceId = Guid.NewGuid(),
            TenantId = tenantId,
            Name = dto.Name,
            Description = dto.Description,
            CurrentStateId = dto.CurrentStateId ?? Guid.Empty,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Metadata = dto.Metadata
        };
    }

    public static BookingDto ToDto(this Booking booking)
    {
        return new BookingDto
        {
            BookingId = booking.BookingId,
            ResourceId = booking.ResourceId,
            UserId = booking.UserId,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Status = booking.Status
        };
    }

    public static Booking ToEntity(this CreateBookingDto dto, Guid userId, Guid tenantId)
    {
        return new Booking
        {
            BookingId = Guid.NewGuid(),
            TenantId = tenantId,
            ResourceId = dto.ResourceId,
            UserId = userId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = "Pending"
        };
    }
}
