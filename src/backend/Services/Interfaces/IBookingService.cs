using ServiceBooking.Api.DTOs.Booking;
using ServiceBooking.Api.DTOs.Common;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(
        Guid customerId,
        CreateBookingRequest request,
        CancellationToken cancellationToken);

    Task<PagedResult<BookingResponse>> GetForCustomerAsync(
        Guid customerId,
        MyBookingsQuery query,
        CancellationToken cancellationToken);

    Task<BookingResponse> CancelForCustomerAsync(
        Guid bookingId,
        Guid customerId,
        CancelBookingRequest request,
        CancellationToken cancellationToken);

    Task<PagedResult<BookingResponse>> GetForAdminAsync(
        BookingListQuery query,
        CancellationToken cancellationToken);

    Task<BookingResponse> UpdateStatusAsync(
        Guid bookingId,
        UpdateBookingStatusRequest request,
        CancellationToken cancellationToken);
}
