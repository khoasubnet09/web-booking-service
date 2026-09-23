using ServiceBooking.Api.DTOs.Booking;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IBookingAvailabilityService
{
    Task<AvailableSlotsResponse> GetAvailableSlotsAsync(AvailableSlotsQuery query, CancellationToken cancellationToken);
}
