using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.ServiceCatalog;

public sealed class CreateServiceRequest
{
    [Required, MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Range(1, int.MaxValue)]
    public int DurationMinutes { get; init; }

    [Range(typeof(decimal), "0", "9999999999999999")]
    public decimal Price { get; init; }
}
