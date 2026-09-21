using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Entities;
using ServiceBooking.Api.Enums;

namespace ServiceBooking.Api.Data;

public class DbSeeder(IPasswordHasher<User> passwordHasher)
{
    public async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var admin = await GetOrCreateUserAsync(dbContext, "admin@servicebooking.com", "System Administrator", UserRole.Admin, "Admin@123", cancellationToken);
        var customer1 = await GetOrCreateUserAsync(dbContext, "customer1@servicebooking.com", "Tran Dang Dang Khoa", UserRole.Customer, "Customer@123", cancellationToken);
        var customer2 = await GetOrCreateUserAsync(dbContext, "customer2@servicebooking.com", "Pham Van Tien Dat", UserRole.Customer, "Customer@123", cancellationToken);

        var staff1 = await GetOrCreateStaffAsync(dbContext, "ngoc.mai@servicebooking.com", "Ngoc Mai", cancellationToken);
        var staff2 = await GetOrCreateStaffAsync(dbContext, "minh.tuan@servicebooking.com", "Minh Tuan", cancellationToken);

        var services = new[]
        {
            await GetOrCreateServiceAsync(dbContext, "Haircut", "Professional haircut service.", 30, 150000m, cancellationToken),
            await GetOrCreateServiceAsync(dbContext, "Hair Coloring", "Full hair coloring consultation and service.", 90, 650000m, cancellationToken),
            await GetOrCreateServiceAsync(dbContext, "Manicure", "Basic manicure and nail care.", 45, 180000m, cancellationToken),
            await GetOrCreateServiceAsync(dbContext, "Facial Treatment", "Relaxing facial cleansing treatment.", 60, 350000m, cancellationToken),
            await GetOrCreateServiceAsync(dbContext, "Massage", "Full body relaxation massage.", 60, 400000m, cancellationToken)
        };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var scheduleDates = Enumerable.Range(-2, 7).Select(offset => today.AddDays(offset)).ToArray();

        foreach (var staff in new[] { staff1, staff2 })
        {
            foreach (var workDate in scheduleDates)
            {
                var exists = await dbContext.WorkSchedules.AnyAsync(
                    schedule => schedule.StaffId == staff.Id && schedule.WorkDate == workDate &&
                                schedule.StartTime == new TimeOnly(9, 0) && schedule.EndTime == new TimeOnly(17, 0), cancellationToken);

                if (!exists)
                {
                    dbContext.WorkSchedules.Add(new WorkSchedule
                    {
                        StaffId = staff.Id,
                        WorkDate = workDate,
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(17, 0)
                    });
                }
            }
        }

        var bookings = new[]
        {
            new SeedBooking("SEED-001", customer1, services[0], staff1, today.AddDays(-2), new TimeOnly(9, 0), BookingStatus.Completed),
            new SeedBooking("SEED-002", customer2, services[2], staff2, today.AddDays(-2), new TimeOnly(10, 0), BookingStatus.Completed),
            new SeedBooking("SEED-003", customer1, services[3], staff1, today.AddDays(-1), new TimeOnly(11, 0), BookingStatus.Completed),
            new SeedBooking("SEED-004", customer2, services[4], staff2, today, new TimeOnly(9, 0), BookingStatus.Confirmed),
            new SeedBooking("SEED-005", customer1, services[1], staff1, today.AddDays(1), new TimeOnly(9, 0), BookingStatus.Pending),
            new SeedBooking("SEED-006", customer2, services[0], staff2, today.AddDays(1), new TimeOnly(11, 0), BookingStatus.Confirmed),
            new SeedBooking("SEED-007", customer1, services[2], staff1, today.AddDays(2), new TimeOnly(13, 0), BookingStatus.Cancelled),
            new SeedBooking("SEED-008", customer2, services[3], staff2, today.AddDays(2), new TimeOnly(14, 0), BookingStatus.Pending),
            new SeedBooking("SEED-009", customer1, services[4], staff1, today.AddDays(3), new TimeOnly(10, 0), BookingStatus.Confirmed),
            new SeedBooking("SEED-010", customer2, services[0], staff2, today.AddDays(4), new TimeOnly(15, 0), BookingStatus.Pending)
        };

        foreach (var booking in bookings)
        {
            var exists = await dbContext.Bookings.AnyAsync(item => item.BookingCode == booking.Code, cancellationToken);
            if (exists)
            {
                continue;
            }

            var startTime = new DateTimeOffset(
                booking.Date.Year, booking.Date.Month, booking.Date.Day,
                booking.StartTime.Hour, booking.StartTime.Minute, 0, TimeSpan.Zero);

            dbContext.Bookings.Add(new Booking
            {
                BookingCode = booking.Code,
                CustomerId = booking.Customer.Id,
                ServiceId = booking.Service.Id,
                StaffId = booking.Staff.Id,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(booking.Service.DurationMinutes),
                Status = booking.Status,
                CustomerNote = "Sample booking data.",
                CancellationReason = booking.Status == BookingStatus.Cancelled ? "Sample cancellation." : null,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<User> GetOrCreateUserAsync(AppDbContext dbContext, string email, string fullName, UserRole role, string password, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (user is not null)
        {
            return user;
        }

        user = new User
        {
            Email = email,
            FullName = fullName,
            Role = role,
            IsActive = true,
            PasswordHash = string.Empty
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);
        dbContext.Users.Add(user);
        return user;
    }

    private static async Task<Staff> GetOrCreateStaffAsync(AppDbContext dbContext, string email, string fullName, CancellationToken cancellationToken)
    {
        var staff = await dbContext.Staffs.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (staff is not null)
        {
            return staff;
        }

        staff = new Staff { Email = email, FullName = fullName, IsActive = true };
        dbContext.Staffs.Add(staff);
        return staff;
    }

    private static async Task<Service> GetOrCreateServiceAsync(AppDbContext dbContext, string name, string description, int durationMinutes, decimal price, CancellationToken cancellationToken)
    {
        var service = await dbContext.Services.SingleOrDefaultAsync(item => item.Name == name, cancellationToken);
        if (service is not null)
        {
            return service;
        }

        service = new Service
        {
            Name = name,
            Description = description,
            DurationMinutes = durationMinutes,
            Price = price,
            IsActive = true
        };

        dbContext.Services.Add(service);
        return service;
    }

    private sealed record SeedBooking(string Code, User Customer, Service Service, Staff Staff, DateOnly Date, TimeOnly StartTime, BookingStatus Status);
}
