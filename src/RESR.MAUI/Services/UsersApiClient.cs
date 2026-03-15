using System.Net.Http.Headers;
using System.Net.Http.Json;
using RESR.Models.Users;

namespace RESR.MAUI.Services;

public sealed class UsersApiClient : IUsersApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiSession _session;

    public UsersApiClient(HttpClient httpClient, IApiSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task RegisterAsync(RegisterUserRequest request, CancellationToken ct)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync("api/users/register", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
    }

    public async Task LoginAsync(Login login, CancellationToken ct)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync("api/login", login, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }

            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
            if (string.IsNullOrWhiteSpace(payload?.Token))
                throw new ApiException(System.Net.HttpStatusCode.Unauthorized, "Token missing from login response.");

            _session.Token = payload.Token;
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
    }

    public async Task<PaginatedUsersResponse> GetUsersAsync(CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.GetAsync("api/users", ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }

            var users = await response.Content.ReadFromJsonAsync<PaginatedUsersResponse>(cancellationToken: ct);
            return users ?? new PaginatedUsersResponse([], 1, 20, 0, 0);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
    }

    public async Task<UserResponse?> GetMeAsync(CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.GetAsync("api/users/me", ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }

            return await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken: ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
    }

    private void ApplyAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_session.Token)
            ? null
            : new AuthenticationHeaderValue("Bearer", _session.Token);
    }

    private sealed record LoginResponse(string Token);
}
