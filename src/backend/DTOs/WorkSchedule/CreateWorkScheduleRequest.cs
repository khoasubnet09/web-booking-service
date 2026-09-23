using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBooking.Api.DTOs.WorkSchedule
{
    public sealed class CreateWorkScheduleRequest
    {
        public DateOnly WorkDate { get; init; }
        
        public TimeOnly StartTime { get; init; }

        public TimeOnly EndTime { get; init; }
    }
}
/* 
DateOnly vaf TimeOnly laf "Value Type", cả 2 đều có một giá trị mặc định, không phải null giống như string?
Ví dụ nếu không truyền gì vào thì mặc định có default value tương ứng. Vì vậy nếu viết:

[Required]
public TimeOnly StartTime { get; set; }

thì [Required] không mang nhiều ý nghĩa với property
*/

// Rule StartTime < EndTime sẽ được kiểm tra trong Service layer.