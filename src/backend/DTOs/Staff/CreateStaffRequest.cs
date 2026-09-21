// Staff mới mặc định active trong Service layer; không để client quyết định IsActive lúc tạo
using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.Staff;

public sealed class CreateStaffRequest
{
    [Required, StringLength(200)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(255)]
    public string Email { get; init; } = string.Empty;
}