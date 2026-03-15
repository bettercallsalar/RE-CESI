using System.Net.Http.Headers;
using System.Net.Http.Json;
using RESR.Models.Reactions;

namespace RESR.MAUI.Services;

public sealed class ReactionsApiClient : IReactionsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiSession _session;

    public ReactionsApiClient(HttpClient httpClient, IApiSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<IReadOnlyList<ReactionResponse>> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        return await SendAsync(
            () => _httpClient.GetAsync($"resources/{idResource}/reactions", ct),
            Array.Empty<ReactionResponse>(),
            ct,
            requireAuthorization: false);
    }

    public async Task<ReactionResponse> CreateAsync(int idResource, CreateReactionRequest request, CancellationToken ct)
    {
        return await SendForPayloadAsync(
            () => _httpClient.PostAsJsonAsync($"resources/{idResource}/reactions", request, ct),
            "Reaction creee mais reponse vide.",
            ct);
    }

    public async Task<ReactionResponse> UpdateAsync(int idReaction, UpdateReactionRequest request, CancellationToken ct)
    {
        return await SendForPayloadAsync(
            () => _httpClient.PatchAsJsonAsync($"api/reactions/{idReaction}", request, ct),
            "Reaction modifiee mais reponse vide.",
            ct);
    }

    public async Task DeleteAsync(int idReaction, CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.DeleteAsync($"api/reactions/{idReaction}", ct);

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

    private async Task<TResponse> SendAsync<TResponse>(
        Func<Task<HttpResponseMessage>> action,
        TResponse fallback,
        CancellationToken ct,
        bool requireAuthorization)
    {
        try
        {
            if (requireAuthorization)
                ApplyAuthorizationHeader();
            else
                _httpClient.DefaultRequestHeaders.Authorization = null;

            using var response = await action();

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

    private async Task<ReactionResponse> SendForPayloadAsync(
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

            var payload = await response.Content.ReadFromJsonAsync<ReactionResponse>(cancellationToken: ct);
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
