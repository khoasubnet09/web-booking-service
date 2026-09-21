using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Api.DTOs.Common;
using ServiceBooking.Api.DTOs.ServiceCatalog;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(IServiceCatalogService serviceCatalog) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ServiceResponse>>> GetActive([FromQuery] ServiceListQuery query, CancellationToken cancellationToken)
    {
        var result = await serviceCatalog.GetActiveAsync(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<ServiceResponse>>> GetForAdmin([FromQuery] ServiceListQuery query, CancellationToken cancellationToken)
    {
        var result = await serviceCatalog.GetForAdminAsync(query, cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceResponse>> Create([FromBody] CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await serviceCatalog.CreateAsync(request, cancellationToken);

        return Created($"/api/services/{service.Id}", service);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ServiceResponse>> Update(Guid id, [FromBody] UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var service = await serviceCatalog.UpdateAsync(id, request, cancellationToken);

        return Ok(service);
    }
}