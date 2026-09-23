using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceBooking.Api.DTOs.WorkSchedule;

namespace ServiceBooking.Api.Services.Interfaces
{
    public interface IWorkScheduleService
    {
        Task<IReadOnlyList<WorkScheduleResponse>> GetByStaffIdAsync(Guid staffId, CancellationToken cancellationToken);

        Task<WorkScheduleResponse> CreateAsync(Guid staffId, CreateWorkScheduleRequest request, CancellationToken cancellationToken);
    }
}