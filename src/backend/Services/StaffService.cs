// StaffService : xử lý nghiệp vụ Staff và thao tác EF Core, cụ thể:

/*
1. Controller chỉ gọi StaffService
2. StaffService kiểm tra dữ liệu/nghiệp vụ
3. StaffService dùng AppDbContext truy vấn SQL Server */

/* Các ru;e cần xử lý tại đây:
- Staff mới mặc định IsActive = true.
- Email phải duy nhất.
- Email được chuẩn hoá về chữ thường.
- Không được tạo/sửa FullName chỉ chứa khoảng trắng.
- Không tìm thấy Staff khi update → 404.
- Không cho đổi email thành email của Staff khác → 409. */
using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.DTOs.Staff;
using ServiceBooking.Api.Entities;
using ServiceBooking.Api.Exceptions;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class StaffService(AppDbContext dbContext) : IStaffService
{
    public async Task<IReadOnlyList<StaffResponse>> GetActiveAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Staffs
            .AsNoTracking()
            .Where(item => item.IsActive)
            .OrderBy(item => item.FullName)
            .ThenBy(item => item.Email)
            .Select(item => new StaffResponse(
                item.Id,
                item.FullName,
                item.Email,
                item.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StaffResponse>> GetForAdminAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Staffs
            .AsNoTracking()
            .OrderBy(item => item.FullName)
            .ThenBy(item => item.Email)
            .Select(item => new StaffResponse(
                item.Id,
                item.FullName,
                item.Email,
                item.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<StaffResponse> CreateAsync(CreateStaffRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        var emailExists = await dbContext.Staffs.AnyAsync(item => item.Email == email, cancellationToken);

        if (emailExists)
        {
            throw new ApiException(StatusCodes.Status409Conflict,
                "STAFF_EMAIL_ALREADY_EXISTS",
                "Staff email is already in use.");
        }

        var staff = new Staff
        {
            FullName = NormalizeFullName(request.FullName),
            Email = email,
            IsActive = true
        };

        dbContext.Staffs.Add(staff);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(staff);
    }

    public async Task<StaffResponse> UpdateAsync(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken)
    {
        var staff = await dbContext.Staffs.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (staff is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound,
                "STAFF_NOT_FOUND",
                "Staff was not found.");
        }

        var email = NormalizeEmail(request.Email);

        var emailExists = await dbContext.Staffs.AnyAsync(
            item => item.Email == email && item.Id != id, cancellationToken);

        if (emailExists)
        {
            throw new ApiException(StatusCodes.Status409Conflict,
                "STAFF_EMAIL_ALREADY_EXISTS",
                "Staff email is already in use.");
        }

        staff.FullName = NormalizeFullName(request.FullName);
        staff.Email = email;
        staff.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(staff);
    }

    private static string NormalizeFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ApiException(StatusCodes.Status400BadRequest,
                "STAFF_FULL_NAME_REQUIRED",
                "Staff full name is required.");
        }

        return fullName.Trim();
    }

    private static string NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ApiException(StatusCodes.Status400BadRequest,
                "STAFF_EMAIL_REQUIRED",
                "Staff email is required.");
        }

        return email.Trim().ToLowerInvariant();
    }

    private static StaffResponse ToResponse(Staff staff)
    {
        return new StaffResponse(
            staff.Id,
            staff.FullName,
            staff.Email,
            staff.IsActive);
    }
}