// IStaffService: hợp đồng quy định Controller được phép gọi những thao tác Staff nào, trước khi viết logic EF Core

/* 
GetActiveAsync : User đã đăng nhập xem Staff active
GetForAdminAsync : Admin xem toàn bộ Staff 
CreateAsync : Admin tạo Staff
UpdateAsync : Admin sửa hoặc khóa/mở Staff */

using ServiceBooking.Api.DTOs.Staff;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IStaffService
{
    Task<IReadOnlyList<StaffResponse>> GetActiveAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<StaffResponse>> GetForAdminAsync(CancellationToken cancellationToken);

    Task<StaffResponse> CreateAsync(CreateStaffRequest request, CancellationToken cancellationToken);

    Task<StaffResponse> UpdateAsync(Guid id, UpdateStaffRequest request, CancellationToken cancellationToken);
}