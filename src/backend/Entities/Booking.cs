using ServiceBooking.Api.Enums;

namespace ServiceBooking.Api.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string BookingCode { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid StaffId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? CustomerNote { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public User Customer { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public Staff Staff { get; set; } = null!;
}
