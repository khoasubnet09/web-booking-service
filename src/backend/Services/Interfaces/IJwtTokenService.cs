using ServiceBooking.Api.DTOs.Auth;
using ServiceBooking.Api.Entities;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResponse CreateToken(User user);
}
