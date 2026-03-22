using System.Net.Http.Headers;
using System.Net.Http.Json;
using RESR.Models.Follows;

namespace RESR.MAUI.Services;

public sealed class FollowsApiClient : IFollowsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiSession _session;

    public FollowsApiClient(HttpClient httpClient, IApiSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<bool> ExistsAsync(int idFollower, int idFollowing, CancellationToken ct)
    {
        try
        {
            _ = idFollower;
            ApplyAuthorizationHeader();
            using var response = await _httpClient.GetAsync($"api/follows/me/following/{idFollowing}", ct);

            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);

            var state = await response.Content.ReadFromJsonAsync<FollowStateResponse>(cancellationToken: ct);
            return state?.IsFollowing ?? false;
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
        }
    }

    public async Task FollowAsync(int idFollower, int idFollowing, CancellationToken ct)
    {
        try
        {
            _ = idFollower;
            ApplyAuthorizationHeader();
            using var response = await _httpClient.PostAsync($"api/follows/{idFollowing}", content: null, ct);

            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
        }
    }

    public async Task UnfollowAsync(int idFollower, int idFollowing, CancellationToken ct)
    {
        try
        {
            _ = idFollower;
            ApplyAuthorizationHeader();
            using var response = await _httpClient.DeleteAsync($"api/follows/{idFollowing}", ct);

            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);
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
}
