using RESR.Models.Users;

namespace RESR.MAUI.Services;

public interface IUsersApiClient
{
    Task RegisterAsync(RegisterUserRequest request, CancellationToken ct);
    Task LoginAsync(Login login, CancellationToken ct);
    Task<PaginatedUsersResponse> GetUsersAsync(CancellationToken ct);
    Task<UserResponse?> GetMeAsync(CancellationToken ct);
}
