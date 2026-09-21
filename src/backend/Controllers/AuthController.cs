using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Api.DTOs.Auth;
using ServiceBooking.Api.Services.Interfaces;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ServiceBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateRegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Registration completed successfully.",
            response.UserId
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(CreateLoginRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(request, cancellationToken);
        if (response is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Token does not contain a valid user identifier."
            });
        }

        var response = await authService.GetCurrentUserAsync(
            userId,
            cancellationToken);

        if (response is null)
        {
            return Unauthorized(new
            {
                message = "User is no longer available."
            });
        }

        return Ok(response);
    }
}
