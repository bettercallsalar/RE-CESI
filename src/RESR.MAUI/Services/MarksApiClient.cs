using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RESR.Models.Marks;

namespace RESR.MAUI.Services;

public sealed class MarksApiClient : IMarksApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiSession _session;

    public MarksApiClient(HttpClient httpClient, IApiSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<PaginatedMarksResponse> GetFavoritesAsync(int page, int pageSize, CancellationToken ct)
    {
        return await GetAsync(
            $"api/marks/favorite?page={page}&pageSize={pageSize}",
            new PaginatedMarksResponse([], page, pageSize, 0, 0),
            ct);
    }

    public async Task<PaginatedMarksResponse> GetReadLaterAsync(int page, int pageSize, CancellationToken ct)
    {
        return await GetAsync(
            $"api/marks/readLater?page={page}&pageSize={pageSize}",
            new PaginatedMarksResponse([], page, pageSize, 0, 0),
            ct);
    }

    public async Task<MarkResponse?> GetFavoriteAsync(int idResource, CancellationToken ct)
    {
        return await GetOptionalAsync($"api/marks/favorite/{idResource}", ct);
    }

    public async Task<MarkResponse?> GetReadLaterAsync(int idResource, CancellationToken ct)
    {
        return await GetOptionalAsync($"api/marks/readLater/{idResource}", ct);
    }

    public async Task<MarkResponse> MarkAsFavoriteAsync(int idResource, CancellationToken ct)
    {
        return await SendForPayloadAsync(
            () => _httpClient.PostAsync($"api/marks/favorite/{idResource}", content: null, ct),
            "Favori cree mais reponse vide.",
            ct);
    }

    public async Task UnmarkAsFavoriteAsync(int idResource, CancellationToken ct)
    {
        await DeleteAsync($"api/marks/favorite/{idResource}", ct);
    }

    public async Task<MarkResponse> MarkAsReadLaterAsync(int idResource, CancellationToken ct)
    {
        return await SendForPayloadAsync(
            () => _httpClient.PostAsync($"api/marks/readLater/{idResource}", content: null, ct),
            "Read later cree mais reponse vide.",
            ct);
    }

    public async Task UnmarkAsReadLaterAsync(int idResource, CancellationToken ct)
    {
        await DeleteAsync($"api/marks/readLater/{idResource}", ct);
    }

    private async Task DeleteAsync(string uri, CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.DeleteAsync(uri, ct);

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

    private async Task<MarkResponse?> GetOptionalAsync(string uri, CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.GetAsync(uri, ct);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }

            return await response.Content.ReadFromJsonAsync<MarkResponse>(cancellationToken: ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
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

    private async Task<MarkResponse> SendForPayloadAsync(
        Func<Task<HttpResponseMessage>> action,
        string emptyPayloadMessage,
        CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await action();

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }

            var payload = await response.Content.ReadFromJsonAsync<MarkResponse>(cancellationToken: ct);
            return payload ?? throw new ApiException(response.StatusCode, emptyPayloadMessage);
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
