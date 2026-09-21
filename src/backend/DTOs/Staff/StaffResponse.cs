namespace ServiceBooking.Api.DTOs.Staff;

public sealed record StaffResponse(
    Guid Id,
    string FullName,
    string Email,
    bool IsActive);