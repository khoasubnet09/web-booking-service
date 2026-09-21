namespace ServiceBooking.Api.Entities;

public class Staff
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<WorkSchedule> WorkSchedules { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
}
