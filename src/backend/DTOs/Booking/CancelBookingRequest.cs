using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Booking;

public sealed class CancelBookingRequest
{
    [Required, MaxLength(1000)]
    public string CancellationReason { get; init; } = string.Empty;
}
