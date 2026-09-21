// IsActive = false chính là thao tác khóa Staff
using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Staff;

public sealed class UpdateStaffRequest
{
    [Required, StringLength(200)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    public string Email { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}