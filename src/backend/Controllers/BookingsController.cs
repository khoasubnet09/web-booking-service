using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ServiceBooking.Api.DTOs.Booking;
using ServiceBooking.Api.DTOs.Common;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(
    IBookingAvailabilityService bookingAvailabilityService,
    IBookingService bookingService) : ControllerBase
{
    [HttpGet("available-slots")]
    [Authorize]
    public async Task<ActionResult<AvailableSlotsResponse>> GetAvailableSlots([FromQuery] AvailableSlotsQuery query, CancellationToken cancellationToken)
    {
        var result = await bookingAvailabilityService.GetAvailableSlotsAsync(query, cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<BookingResponse>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = GetCurrentUserId();
        if (customerId is null)
        {
            return Unauthorized();
        }

        var booking = await bookingService.CreateAsync(customerId.Value, request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, booking);
    }

    [HttpGet("my-bookings")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetMyBookings(
        [FromQuery] MyBookingsQuery query,
        CancellationToken cancellationToken)
    {
        var customerId = GetCurrentUserId();
        if (customerId is null)
        {
            return Unauthorized();
        }

        var result = await bookingService.GetForCustomerAsync(customerId.Value, query, cancellationToken);

        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<BookingResponse>> Cancel(
        Guid id,
        [FromBody] CancelBookingRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = GetCurrentUserId();
        if (customerId is null)
        {
            return Unauthorized();
        }

        var booking = await bookingService.CancelForCustomerAsync(
            id,
            customerId.Value,
            request,
            cancellationToken);

        return Ok(booking);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetForAdmin(
        [FromQuery] BookingListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await bookingService.GetForAdminAsync(query, cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<BookingResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateBookingStatusRequest request,
        CancellationToken cancellationToken)
    {
        var booking = await bookingService.UpdateStatusAsync(id, request, cancellationToken);

        return Ok(booking);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
