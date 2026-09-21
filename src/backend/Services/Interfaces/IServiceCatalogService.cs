using ServiceBooking.Api.DTOs.Common;
using ServiceBooking.Api.DTOs.ServiceCatalog;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<PagedResult<ServiceResponse>> GetActiveAsync(ServiceListQuery query, CancellationToken cancellationToken);

    Task<PagedResult<ServiceResponse>> GetForAdminAsync(ServiceListQuery query, CancellationToken cancellationToken);

    Task<ServiceResponse> CreateAsync(CreateServiceRequest request, CancellationToken cancellationToken);

    Task<ServiceResponse> UpdateAsync(Guid id, UpdateServiceRequest request, CancellationToken cancellationToken);
}