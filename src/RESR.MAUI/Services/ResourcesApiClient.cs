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
        return await GetArticlesAsync(page, pageSize, keyword: null, ct);
    }

    public async Task<PaginatedArticlesResponse> GetArticlesAsync(int page, int pageSize, string? keyword, CancellationToken ct)
    {
        var uri = BuildListingUri("api/articles", page, pageSize, keyword);
        return await GetAsync(uri, new PaginatedArticlesResponse([], page, pageSize, 0, 0), ct);
    }

    public async Task<PaginatedArticlesResponse> GetArticlesByUserAsync(int idUser, int page, int pageSize, string? keyword, CancellationToken ct)
    {
        var uri = BuildListingUri("api/articles", page, pageSize, keyword, ("idUser", idUser.ToString()));
        return await GetAsync(uri, new PaginatedArticlesResponse([], page, pageSize, 0, 0), ct);
    }

    public async Task<PaginatedArticlesResponse> GetMyArticlesAsync(int idUser, int page, int pageSize, string? keyword, CancellationToken ct)
    {
        var uri = BuildListingUri($"api/articles/{idUser}/my-articles", page, pageSize, keyword);
        return await GetAsync(uri, new PaginatedArticlesResponse([], page, pageSize, 0, 0), ct);
    }

    public async Task<ArticleResponse?> GetArticleByIdAsync(int idResource, CancellationToken ct)
    {
        return await GetAsync($"api/articles/{idResource}", fallback: (ArticleResponse?)null, ct);
    }

    public async Task<ArticleResponse?> GetOwnArticleByIdAsync(int idResource, CancellationToken ct)
    {
        return await GetAsync($"api/articles/me/{idResource}", fallback: (ArticleResponse?)null, ct);
    }

    public async Task<PaginatedEventsResponse> GetEventsAsync(int page, int pageSize, CancellationToken ct)
    {
        return await GetEventsAsync(page, pageSize, keyword: null, ct);
    }

    public async Task<PaginatedEventsResponse> GetEventsAsync(int page, int pageSize, string? keyword, CancellationToken ct)
    {
        var uri = BuildListingUri("api/events", page, pageSize, keyword);
        return await GetAsync(uri, new PaginatedEventsResponse([], page, pageSize, 0, 0), ct);
    }

    public async Task<EventResponse?> GetEventByIdAsync(int idResource, CancellationToken ct)
    {
        return await GetAsync<EventResponse?>($"api/events/{idResource}", fallback: null, ct);
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

    private static string BuildListingUri(
        string basePath,
        int page,
        int pageSize,
        string? keyword,
        params (string Key, string Value)[] extraQueryParameters)
    {
        var queryParameters = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(keyword))
            queryParameters.Add($"keyword={Uri.EscapeDataString(keyword.Trim())}");

        foreach (var (key, value) in extraQueryParameters)
        {
            if (!string.IsNullOrWhiteSpace(value))
                queryParameters.Add($"{key}={Uri.EscapeDataString(value)}");
        }

        return $"{basePath}?{string.Join("&", queryParameters)}";
    }
}
