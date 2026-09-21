using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.DTOs.Common;
using ServiceBooking.Api.DTOs.ServiceCatalog;
using ServiceBooking.Api.Entities;
using ServiceBooking.Api.Exceptions;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class ServiceCatalogService(AppDbContext dbContext) : IServiceCatalogService
{
    public Task<PagedResult<ServiceResponse>> GetActiveAsync(ServiceListQuery query, CancellationToken cancellationToken)
    {
        return GetPagedAsync(query, activeOnly: true, cancellationToken);
    }

    public Task<PagedResult<ServiceResponse>> GetForAdminAsync(ServiceListQuery query,  CancellationToken cancellationToken)
    {
        return GetPagedAsync(query, activeOnly: false, cancellationToken);
    }

    public async Task<ServiceResponse> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = new Service
        {
            Name = NormalizeName(request.Name),
            Description = NormalizeDescription(request.Description),
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            IsActive = true
        };

        dbContext.Services.Add(service);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(service);
    }

    public async Task<ServiceResponse> UpdateAsync(Guid id, UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await dbContext.Services.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (service is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "SERVICE_NOT_FOUND", "Service was not found.");
        }

        service.Name = NormalizeName(request.Name);
        service.Description = NormalizeDescription(request.Description);
        service.DurationMinutes = request.DurationMinutes;
        service.Price = request.Price;
        service.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(service);
    }

    private async Task<PagedResult<ServiceResponse>> GetPagedAsync(ServiceListQuery request, bool activeOnly, CancellationToken cancellationToken)
    {
        IQueryable<Service> services = dbContext.Services.AsNoTracking();

        if (activeOnly)
        {
            services = services.Where(item => item.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            services = services.Where(item =>
                item.Name.Contains(search) || (item.Description != null && item.Description.Contains(search)));
        }

        var totalCount = await services.CountAsync(cancellationToken);

        var items = await services
            .OrderBy(item => item.Name)
            .ThenBy(item => item.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new ServiceResponse(
                item.Id,
                item.Name,
                item.Description,
                item.DurationMinutes,
                item.Price,
                item.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceResponse>(
            items,
            request.Page,
            request.PageSize,
            totalCount);
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "SERVICE_NAME_REQUIRED",
                "Service name is required.");
        }

        return name.Trim();
    }

    private static string? NormalizeDescription(string? description)
    {
        return string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    private static ServiceResponse ToResponse(Service service)
    {
        return new ServiceResponse(
            service.Id,
            service.Name,
            service.Description,
            service.DurationMinutes,
            service.Price,
            service.IsActive);
    }
}