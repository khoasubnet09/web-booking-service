namespace ServiceBooking.Api.DTOs.Booking;

public sealed record BookingResponse(
    Guid Id,
    string BookingCode,
    Guid CustomerId,
    string CustomerName,
    Guid ServiceId,
    string ServiceName,
    Guid StaffId,
    string StaffName,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    string? CustomerNote,
    string? CancellationReason,
    DateTimeOffset CreatedAt);
