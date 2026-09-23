using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Booking;

public sealed class CreateBookingRequest
{
    public Guid ServiceId { get; init; }

    public Guid StaffId { get; init; }

    public DateTimeOffset StartTime { get; init; }

    [MaxLength(1000)]
    public string? CustomerNote { get; init; }
}
