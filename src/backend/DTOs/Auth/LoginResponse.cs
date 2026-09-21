namespace ServiceBooking.Api.DTOs.Auth;

public sealed record LoginResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    DateTimeOffset ExpiresAt);
