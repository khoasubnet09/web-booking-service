using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceBooking.Api.Data;
using ServiceBooking.Api.DTOs.Auth;
using ServiceBooking.Api.Entities;
using ServiceBooking.Api.Enums;
using ServiceBooking.Api.Exceptions;
using ServiceBooking.Api.Services.Interfaces;

namespace ServiceBooking.Api.Services;

public class AuthService(AppDbContext dbContext, IPasswordHasher<User> passwordHasher, IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<RegisterResponse> RegisterAsync(CreateRegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);

        if (emailExists)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "EMAIL_ALREADY_EXISTS",
                "Email is already in use.");
        }

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            Role = UserRole.Customer,
            IsActive = true,
            PasswordHash = string.Empty
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id);
    }

    public async Task<LoginResponse?> LoginAsync(CreateLoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var token = jwtTokenService.CreateToken(user);

        return new LoginResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            token.AccessToken,
            token.ExpiresAt);
    }

    public async Task<GetCurrentUserResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        return new GetCurrentUserResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString());
    }
}
