using System.Net.Http.Headers;
using System.Net.Http.Json;
using RESR.Models.Resources;

namespace RESR.MAUI.Services;

public sealed class ResourcesApiClient : IResourcesApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiSession _session;

    public ResourcesApiClient(HttpClient httpClient, IApiSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<PaginatedArticlesResponse> GetArticlesAsync(int page, int pageSize, CancellationToken ct)
    {
        var uri = $"api/articles?page={page}&pageSize={pageSize}";
        return await GetAsync(uri, new PaginatedArticlesResponse([], page, pageSize, 0, 0), ct);
    }

    public async Task<PaginatedEventsResponse> GetEventsAsync(int page, int pageSize, CancellationToken ct)
    {
        var uri = $"api/events?page={page}&pageSize={pageSize}";
        return await GetAsync(uri, new PaginatedEventsResponse([], page, pageSize, 0, 0), ct);
    }

    private async Task<TResponse> GetAsync<TResponse>(string uri, TResponse fallback, CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.GetAsync(uri, ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }

            var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
            return payload ?? fallback;
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
}
