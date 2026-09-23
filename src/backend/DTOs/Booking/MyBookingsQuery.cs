using System.ComponentModel.DataAnnotations;
using ServiceBooking.Api.Enums;

namespace ServiceBooking.Api.DTOs.Booking;

public sealed class MyBookingsQuery
{
    public BookingStatus? Status { get; init; }

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    [Range(1, 10_000)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}
