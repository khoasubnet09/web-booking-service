namespace ServiceBooking.Api.DTOs.ServiceCatalog;

public sealed record ServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price,
    bool IsActive
);
