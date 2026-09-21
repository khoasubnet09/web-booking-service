// StaffController
/*
    1. Nhận HTTP request.
    2. Đặt route.
    3. Đặt Authorization.
    4. Gọi IStaffService và trả HTTP response.
*/

/* Endpoint và quyền truy cập 
    1. GET /api/staffs (quyền User) : User đã đăng nhập và chỉ xem được Staff active
    2. GET /api/staffs/admin (quyền Admin) : Xem toàn bộ Staff
    3. POST /api/staffs (quyền Admin) : Tạo Staff
    4. PUT /api/staffs/{id} (quyền Admin) : Sửa/mở/khóa Staff 
*/
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceBooking.Api.DTOs.Staff;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Controllers;

[ApiController]
[Route("api/staffs")]
public class StaffsController(IStaffService staffService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<StaffResponse>>> GetActive(CancellationToken cancellationToken)
    {
        var staffs = await staffService.GetActiveAsync(cancellationToken);

        return Ok(staffs);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<StaffResponse>>> GetForAdmin(CancellationToken cancellationToken)
    {
        var staffs = await staffService.GetForAdminAsync(cancellationToken);

        return Ok(staffs);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StaffResponse>> Create([FromBody] CreateStaffRequest request, CancellationToken cancellationToken)
    {
        var staff = await staffService.CreateAsync(request, cancellationToken);

        return Created($"/api/staffs/{staff.Id}", staff);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<StaffResponse>> Update(Guid id, [FromBody] UpdateStaffRequest request, CancellationToken cancellationToken)
    {
        var staff = await staffService.UpdateAsync(id, request, cancellationToken);

        return Ok(staff);
    }
}