namespace ServiceBooking.Api.DTOs.Booking;

public sealed record AvailableSlotResponse(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
);
