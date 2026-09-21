namespace ServiceBooking.Api.Entities;

public class WorkSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StaffId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Staff Staff { get; set; } = null!;
}
