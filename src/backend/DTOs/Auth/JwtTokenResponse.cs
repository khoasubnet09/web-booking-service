namespace ServiceBooking.Api.DTOs.Auth;

public sealed record JwtTokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt);
