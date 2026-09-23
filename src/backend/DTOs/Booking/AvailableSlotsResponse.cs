namespace ServiceBooking.Api.DTOs.Booking;

public sealed record AvailableSlotsResponse(
    Guid ServiceId,
    Guid StaffId,
    DateOnly Date,
    int DurationMinutes,
    IReadOnlyList<AvailableSlotResponse> Slots
);
