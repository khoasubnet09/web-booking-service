using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Auth;

public sealed class CreateRegisterRequest
{
    [Required, StringLength(200)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}