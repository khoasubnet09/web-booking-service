using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ServiceBooking.Api.DTOs.Auth;
using ServiceBooking.Api.Entities;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public JwtTokenResponse CreateToken(User user)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");
        var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is missing.");
        var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is missing.");
        var expirationMinutes = configuration.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60;

        if (expirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT expiration must be greater than zero.");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: signingCredentials);

        return new JwtTokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
