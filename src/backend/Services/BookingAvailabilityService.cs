using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Common;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.DTOs.Booking;
using ServiceBooking.Api.Enums;
using ServiceBooking.Api.Exceptions;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class BookingAvailabilityService(AppDbContext dbContext) : IBookingAvailabilityService
{
    private const int SlotIntervalMinutes = 15;

    public async Task<AvailableSlotsResponse> GetAvailableSlotsAsync(AvailableSlotsQuery query, CancellationToken cancellationToken)
    {
        ValidateQuery(query);

        var service = await dbContext.Services
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.ServiceId, cancellationToken);

        if (service is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound,
                "SERVICE_NOT_FOUND",
                "Service was not found.");
        }

        if (!service.IsActive)
        {
            throw new ApiException(StatusCodes.Status409Conflict,
                "SERVICE_INACTIVE",
                "Service is inactive.");
        }

        var staff = await dbContext.Staffs
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == query.StaffId, cancellationToken);

        if (staff is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound,
                "STAFF_NOT_FOUND",
                "Staff was not found.");
        }

        if (!staff.IsActive)
        {
            throw new ApiException(StatusCodes.Status409Conflict,
                "STAFF_INACTIVE",
                "Staff is inactive.");
        }

        if (query.Date < VietnamTime.Today)
        {
            return EmptyResponse(service.DurationMinutes, query);
        }

        var dayStartUtc = VietnamTime.ToUtc(query.Date, TimeOnly.MinValue);
        var dayEndUtc = VietnamTime.ToUtc(query.Date.AddDays(1), TimeOnly.MinValue);

        var schedules = await dbContext.WorkSchedules
            .AsNoTracking()
            .Where(item => item.StaffId == query.StaffId && item.WorkDate == query.Date)
            .OrderBy(item => item.StartTime)
            .Select(item => new ScheduleWindow(item.StartTime, item.EndTime))
            .ToListAsync(cancellationToken);

        var occupiedBookings = await dbContext.Bookings
            .AsNoTracking()
            .Where(item => item.StaffId == query.StaffId
                && item.Status != BookingStatus.Cancelled
                && item.StartTime < dayEndUtc
                && item.EndTime > dayStartUtc)
            .Select(item => new BookingWindow(item.StartTime, item.EndTime))
            .ToListAsync(cancellationToken);

        var nowUtc = DateTimeOffset.UtcNow;
        var slots = new List<AvailableSlotResponse>();

        foreach (var schedule in schedules)
        {
            var scheduleStartUtc = VietnamTime.ToUtc(query.Date, schedule.StartTime);
            var scheduleEndUtc = VietnamTime.ToUtc(query.Date, schedule.EndTime);

            for (var slotStartUtc = scheduleStartUtc;
                slotStartUtc.AddMinutes(service.DurationMinutes) <= scheduleEndUtc;
                slotStartUtc = slotStartUtc.AddMinutes(SlotIntervalMinutes))
            {
                var slotEndUtc = slotStartUtc.AddMinutes(service.DurationMinutes);

                if (slotStartUtc <= nowUtc)
                {
                    continue;
                }

                var overlapsBooking = occupiedBookings.Any(booking => slotStartUtc < booking.EndTime && slotEndUtc > booking.StartTime);

                if (!overlapsBooking)
                {
                    slots.Add(new AvailableSlotResponse(slotStartUtc, slotEndUtc));
                }
            }
        }

        return new AvailableSlotsResponse(
            service.Id,
            staff.Id,
            query.Date,
            service.DurationMinutes,
            slots);
    }

    private static AvailableSlotsResponse EmptyResponse(int durationMinutes, AvailableSlotsQuery query)
    {
        return new AvailableSlotsResponse(
            query.ServiceId,
            query.StaffId,
            query.Date,
            durationMinutes,
            []);
    }

    private static void ValidateQuery(AvailableSlotsQuery query)
    {
        if (query.ServiceId == Guid.Empty)
        {
            throw new ApiException(StatusCodes.Status400BadRequest,
                "SERVICE_ID_REQUIRED",
                "Service id is required.");
        }

        if (query.StaffId == Guid.Empty)
        {
            throw new ApiException(StatusCodes.Status400BadRequest,
                "STAFF_ID_REQUIRED",
                "Staff id is required.");
        }

        if (query.Date == DateOnly.MinValue)
        {
            throw new ApiException(StatusCodes.Status400BadRequest,
                "BOOKING_DATE_REQUIRED",
                "Booking date is required.");
        }
    }

    private sealed record ScheduleWindow(TimeOnly StartTime, TimeOnly EndTime);

    private sealed record BookingWindow(DateTimeOffset StartTime, DateTimeOffset EndTime);
}
