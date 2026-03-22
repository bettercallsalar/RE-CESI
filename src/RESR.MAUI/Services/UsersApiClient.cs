using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
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
                throw await ApiClientErrors.FromResponseAsync(response, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
        }
    }

    public async Task LoginAsync(Login login, CancellationToken ct)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync("api/login", login, ct);

            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);

            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
            if (string.IsNullOrWhiteSpace(payload?.Token))
                throw ApiClientErrors.InvalidResponse(HttpStatusCode.Unauthorized, "La reponse de connexion du serveur est invalide.");

            _session.Token = payload.Token;
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
        }
    }

    public async Task<PaginatedUsersResponse> GetUsersAsync(CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.GetAsync("api/users", ct);

            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);

            var users = await response.Content.ReadFromJsonAsync<PaginatedUsersResponse>(cancellationToken: ct);
            return users ?? new PaginatedUsersResponse([], 1, 20, 0, 0);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
        }
    }

    public async Task<UserResponse?> GetMeAsync(CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.GetAsync("api/users/me", ct);

            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);

            return await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken: ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
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
