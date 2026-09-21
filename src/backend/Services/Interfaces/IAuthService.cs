using ServiceBooking.Api.DTOs.Auth;

namespace ServiceBooking.Api.Services.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(CreateRegisterRequest request, CancellationToken cancellationToken);
    Task<LoginResponse?> LoginAsync(CreateLoginRequest request, CancellationToken cancellationToken);
    Task<GetCurrentUserResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
