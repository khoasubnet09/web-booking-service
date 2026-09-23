namespace ServiceBooking.Api.DTOs.Booking;

public sealed class AvailableSlotsQuery
{
    public Guid ServiceId { get; init; }

    public Guid StaffId { get; init; }

    public DateOnly Date { get; init; }
}