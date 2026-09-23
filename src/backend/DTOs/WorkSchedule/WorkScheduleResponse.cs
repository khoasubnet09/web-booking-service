using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBooking.Api.DTOs.WorkSchedule
{
    public sealed record WorkScheduleResponse(
        Guid Id,
        Guid StaffId,
        DateOnly WorkDate,
        TimeOnly StartTime,
        TimeOnly EndTime
    );
}