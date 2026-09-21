using System.ComponentModel.DataAnnotations;

namespace ServiceBooking.Api.DTOs.ServiceCatalog;

public sealed class ServiceListQuery
{
    [MaxLength(200)]
    public string? Search { get; init; }

    [Range(1, 10_000)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}