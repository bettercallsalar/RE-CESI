using System.Net.Http.Json;
using RESR.Models.Users;

namespace RESR.MAUI.Services;

public sealed class UsersApiClient : IUsersApiClient
{
    private readonly HttpClient _httpClient;

    public UsersApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync(CancellationToken ct)
    {
        try
        {
            using var response = await _httpClient.GetAsync("api/users", ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }

            var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>(cancellationToken: ct);
            return users ?? [];
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
    }
}
