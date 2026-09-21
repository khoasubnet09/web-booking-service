using ServiceBooking.Api.Enums;

namespace ServiceBooking.Api.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FullName { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsActive { get; set; } = true;
    public ICollection<Booking> Bookings { get; set; } = [];
}
