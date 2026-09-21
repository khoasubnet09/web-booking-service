namespace ServiceBooking.Api.DTOs.Auth;

public sealed record GetCurrentUserResponse (
    Guid UserId,
    string Email,
    string FullName,
    string Role);