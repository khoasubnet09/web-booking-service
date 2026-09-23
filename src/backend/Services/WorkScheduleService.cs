using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.DTOs.WorkSchedule;
using ServiceBooking.Api.Entities;
using ServiceBooking.Api.Exceptions;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class WorkScheduleService(AppDbContext dbContext) : IWorkScheduleService
{
    public async Task<IReadOnlyList<WorkScheduleResponse>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken)
    {
        var staffExists = await dbContext.Staffs.AnyAsync(item => item.Id == staffId, cancellationToken);

        if (!staffExists)
        {
            throw new ApiException(StatusCodes.Status404NotFound,
                "STAFF_NOT_FOUND",
                "Staff was not found.");
        }

        return await dbContext.WorkSchedules
            .AsNoTracking()
            .Where(item => item.StaffId == staffId)
            .OrderBy(item => item.WorkDate)
            .ThenBy(item => item.StartTime)
            .Select(item => new WorkScheduleResponse(
                item.Id,
                item.StaffId,
                item.WorkDate,
                item.StartTime,
                item.EndTime))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkScheduleResponse> CreateAsync(Guid staffId, CreateWorkScheduleRequest request, CancellationToken cancellationToken)
    {
        var staffExists = await dbContext.Staffs.AnyAsync(item => item.Id == staffId, cancellationToken);

        if (!staffExists)
        {
            throw new ApiException(StatusCodes.Status404NotFound,
                "STAFF_NOT_FOUND",
                "Staff was not found.");
        }

        if (request.StartTime >= request.EndTime)
        {
            throw new ApiException(StatusCodes.Status400BadRequest,
                "WORK_SCHEDULE_INVALID_TIME",
                "Start time must be earlier than end time.");
        }

        var overlaps = await dbContext.WorkSchedules.AnyAsync(
            item => item.StaffId == staffId
                && item.WorkDate == request.WorkDate
                && request.StartTime < item.EndTime
                && request.EndTime > item.StartTime,
            cancellationToken);

        if (overlaps)
        {
            throw new ApiException(StatusCodes.Status409Conflict,
                "WORK_SCHEDULE_OVERLAP",
                "The work schedule overlaps an existing schedule.");
        }

        var schedule = new WorkSchedule
        {
            StaffId = staffId,
            WorkDate = request.WorkDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };

        dbContext.WorkSchedules.Add(schedule);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new WorkScheduleResponse(
            schedule.Id,
            schedule.StaffId,
            schedule.WorkDate,
            schedule.StartTime,
            schedule.EndTime);
    }
}