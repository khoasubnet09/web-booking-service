using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Common;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.DTOs.Booking;
using ServiceBooking.Api.DTOs.Common;
using ServiceBooking.Api.Entities;
using ServiceBooking.Api.Enums;
using ServiceBooking.Api.Exceptions;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class BookingService(AppDbContext dbContext) : IBookingService
{
    public async Task<BookingResponse> CreateAsync(
        Guid customerId,
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        ValidateCreateRequest(request);

        var customer = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == customerId && item.IsActive, cancellationToken);

        if (customer is null)
        {
            throw new ApiException(
                StatusCodes.Status401Unauthorized,
                "CUSTOMER_NOT_AVAILABLE",
                "Customer is no longer available.");
        }

        var service = await dbContext.Services
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.ServiceId, cancellationToken);

        if (service is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "SERVICE_NOT_FOUND", "Service was not found.");
        }

        if (!service.IsActive)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "SERVICE_INACTIVE", "Service is inactive.");
        }

        var staff = await dbContext.Staffs
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.StaffId, cancellationToken);

        if (staff is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "STAFF_NOT_FOUND", "Staff was not found.");
        }

        if (!staff.IsActive)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "STAFF_INACTIVE", "Staff is inactive.");
        }

        var startTimeUtc = request.StartTime.ToUniversalTime();
        if (startTimeUtc <= DateTimeOffset.UtcNow)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "BOOKING_START_TIME_IN_PAST",
                "Booking start time must be in the future.");
        }

        var endTimeUtc = startTimeUtc.AddMinutes(service.DurationMinutes);
        var localStartTime = VietnamTime.ToVietnamTime(startTimeUtc);
        var localEndTime = VietnamTime.ToVietnamTime(endTimeUtc);
        var localWorkDate = DateOnly.FromDateTime(localStartTime.DateTime);

        var fitsWorkSchedule = localWorkDate == DateOnly.FromDateTime(localEndTime.DateTime)
            && await dbContext.WorkSchedules.AnyAsync(
                item => item.StaffId == staff.Id
                    && item.WorkDate == localWorkDate
                    && item.StartTime <= TimeOnly.FromDateTime(localStartTime.DateTime)
                    && item.EndTime >= TimeOnly.FromDateTime(localEndTime.DateTime),
                cancellationToken);

        if (!fitsWorkSchedule)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "BOOKING_OUTSIDE_WORK_SCHEDULE",
                "Booking must fit completely inside a staff work schedule.");
        }

        var overlapsExistingBooking = await dbContext.Bookings.AnyAsync(
            item => item.StaffId == staff.Id
                && item.Status != BookingStatus.Cancelled
                && startTimeUtc < item.EndTime
                && endTimeUtc > item.StartTime,
            cancellationToken);

        if (overlapsExistingBooking)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "BOOKING_SLOT_CONFLICT",
                "The selected booking slot is no longer available.");
        }

        var booking = new Booking
        {
            BookingCode = CreateBookingCode(),
            CustomerId = customer.Id,
            ServiceId = service.Id,
            StaffId = staff.Id,
            StartTime = startTimeUtc,
            EndTime = endTimeUtc,
            Status = BookingStatus.Pending,
            CustomerNote = NormalizeOptionalText(request.CustomerNote),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(booking, customer.FullName, service.Name, staff.FullName);
    }

    public async Task<PagedResult<BookingResponse>> GetForCustomerAsync(
        Guid customerId,
        MyBookingsQuery query,
        CancellationToken cancellationToken)
    {
        ValidateDateRange(query.FromDate, query.ToDate);

        IQueryable<Booking> bookings = dbContext.Bookings
            .AsNoTracking()
            .Include(item => item.Customer)
            .Include(item => item.Service)
            .Include(item => item.Staff)
            .Where(item => item.CustomerId == customerId);

        if (query.Status.HasValue)
        {
            bookings = bookings.Where(item => item.Status == query.Status.Value);
        }

        bookings = ApplyDateRange(bookings, query.FromDate, query.ToDate);

        return await ToPagedResponseAsync(bookings, query.Page, query.PageSize, cancellationToken);
    }

    public async Task<BookingResponse> CancelForCustomerAsync(
        Guid bookingId,
        Guid customerId,
        CancelBookingRequest request,
        CancellationToken cancellationToken)
    {
        var booking = await dbContext.Bookings
            .Include(item => item.Customer)
            .Include(item => item.Service)
            .Include(item => item.Staff)
            .SingleOrDefaultAsync(
                item => item.Id == bookingId && item.CustomerId == customerId,
                cancellationToken);

        if (booking is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "BOOKING_NOT_FOUND", "Booking was not found.");
        }

        CancelBooking(booking, request.CancellationReason);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(booking);
    }

    public async Task<PagedResult<BookingResponse>> GetForAdminAsync(
        BookingListQuery query,
        CancellationToken cancellationToken)
    {
        ValidateDateRange(query.FromDate, query.ToDate);
        ValidateOptionalId(query.CustomerId, "CUSTOMER_ID_INVALID", "Customer id must not be empty.");
        ValidateOptionalId(query.ServiceId, "SERVICE_ID_INVALID", "Service id must not be empty.");
        ValidateOptionalId(query.StaffId, "STAFF_ID_INVALID", "Staff id must not be empty.");

        IQueryable<Booking> bookings = dbContext.Bookings
            .AsNoTracking()
            .Include(item => item.Customer)
            .Include(item => item.Service)
            .Include(item => item.Staff);

        if (query.Status.HasValue)
        {
            bookings = bookings.Where(item => item.Status == query.Status.Value);
        }

        if (query.CustomerId.HasValue)
        {
            bookings = bookings.Where(item => item.CustomerId == query.CustomerId.Value);
        }

        if (query.ServiceId.HasValue)
        {
            bookings = bookings.Where(item => item.ServiceId == query.ServiceId.Value);
        }

        if (query.StaffId.HasValue)
        {
            bookings = bookings.Where(item => item.StaffId == query.StaffId.Value);
        }

        bookings = ApplyDateRange(bookings, query.FromDate, query.ToDate);

        return await ToPagedResponseAsync(bookings, query.Page, query.PageSize, cancellationToken);
    }

    public async Task<BookingResponse> UpdateStatusAsync(
        Guid bookingId,
        UpdateBookingStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Status))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "BOOKING_STATUS_INVALID",
                "Booking status is invalid.");
        }

        var booking = await dbContext.Bookings
            .Include(item => item.Customer)
            .Include(item => item.Service)
            .Include(item => item.Staff)
            .SingleOrDefaultAsync(item => item.Id == bookingId, cancellationToken);

        if (booking is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "BOOKING_NOT_FOUND", "Booking was not found.");
        }

        if (request.Status == BookingStatus.Cancelled)
        {
            CancelBooking(
                booking,
                request.CancellationReason,
                rejectStartedBooking: false);
        }
        else if (booking.Status == BookingStatus.Pending && request.Status == BookingStatus.Confirmed)
        {
            booking.Status = BookingStatus.Confirmed;
        }
        else if (booking.Status == BookingStatus.Confirmed && request.Status == BookingStatus.Completed)
        {
            booking.Status = BookingStatus.Completed;
        }
        else
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "BOOKING_STATUS_TRANSITION_NOT_ALLOWED",
                $"Cannot change booking status from {booking.Status} to {request.Status}.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(booking);
    }

    private static void ValidateCreateRequest(CreateBookingRequest request)
    {
        if (request.ServiceId == Guid.Empty)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "SERVICE_ID_REQUIRED", "Service id is required.");
        }

        if (request.StaffId == Guid.Empty)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "STAFF_ID_REQUIRED", "Staff id is required.");
        }

        if (request.StartTime == DateTimeOffset.MinValue)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "BOOKING_START_TIME_REQUIRED",
                "Booking start time is required.");
        }
    }

    private static void ValidateDateRange(DateOnly? fromDate, DateOnly? toDate)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "BOOKING_DATE_RANGE_INVALID",
                "From date must be earlier than or equal to to date.");
        }
    }

    private static void ValidateOptionalId(Guid? value, string code, string message)
    {
        if (value.HasValue && value.Value == Guid.Empty)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, code, message);
        }
    }

    private static IQueryable<Booking> ApplyDateRange(
        IQueryable<Booking> bookings,
        DateOnly? fromDate,
        DateOnly? toDate)
    {
        if (fromDate.HasValue)
        {
            var fromUtc = VietnamTime.ToUtc(fromDate.Value, TimeOnly.MinValue);
            bookings = bookings.Where(item => item.StartTime >= fromUtc);
        }

        if (toDate.HasValue)
        {
            var afterToUtc = VietnamTime.ToUtc(toDate.Value.AddDays(1), TimeOnly.MinValue);
            bookings = bookings.Where(item => item.StartTime < afterToUtc);
        }

        return bookings;
    }

    private async Task<PagedResult<BookingResponse>> ToPagedResponseAsync(
        IQueryable<Booking> bookings,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await bookings.CountAsync(cancellationToken);
        var items = await bookings
            .OrderByDescending(item => item.StartTime)
            .ThenBy(item => item.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BookingResponse>(
            items.Select(ToResponse).ToList(),
            page,
            pageSize,
            totalCount);
    }

    private static void CancelBooking(
        Booking booking,
        string? cancellationReason,
        bool rejectStartedBooking = true)
    {
        if (booking.Status == BookingStatus.Completed)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "BOOKING_COMPLETED_CANNOT_BE_CANCELLED",
                "Completed bookings cannot be cancelled.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "BOOKING_ALREADY_CANCELLED",
                "Booking is already cancelled.");
        }

        if (rejectStartedBooking && booking.StartTime <= DateTimeOffset.UtcNow)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "BOOKING_ALREADY_STARTED",
                "Bookings cannot be cancelled after they have started.");
        }

        if (string.IsNullOrWhiteSpace(cancellationReason))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "CANCELLATION_REASON_REQUIRED",
                "Cancellation reason is required.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = cancellationReason.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string CreateBookingCode()
    {
        return $"BK-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..50];
    }

    private static BookingResponse ToResponse(Booking booking)
    {
        return ToResponse(
            booking,
            booking.Customer.FullName,
            booking.Service.Name,
            booking.Staff.FullName);
    }

    private static BookingResponse ToResponse(
        Booking booking,
        string customerName,
        string serviceName,
        string staffName)
    {
        return new BookingResponse(
            booking.Id,
            booking.BookingCode,
            booking.CustomerId,
            customerName,
            booking.ServiceId,
            serviceName,
            booking.StaffId,
            staffName,
            booking.StartTime,
            booking.EndTime,
            booking.Status.ToString(),
            booking.CustomerNote,
            booking.CancellationReason,
            booking.CreatedAt);
    }
}
