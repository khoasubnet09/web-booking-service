using System.ComponentModel.DataAnnotations;
using ServiceBooking.Api.Enums;

namespace ServiceBooking.Api.DTOs.Booking;

public sealed class UpdateBookingStatusRequest
{
    public BookingStatus Status { get; init; }

    [MaxLength(1000)]
    public string? CancellationReason { get; init; }
}
