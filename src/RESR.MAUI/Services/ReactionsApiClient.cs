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
            () => _httpClient.GetAsync($"api/reactions/resources/{idResource}", ct),
            Array.Empty<ReactionResponse>(),
            ct,
            requireAuthorization: false);
    }

    public async Task<ReactionResponse> CreateAsync(int idResource, CreateReactionRequest request, CancellationToken ct)
    {
        return await SendForPayloadAsync(
            () => _httpClient.PostAsJsonAsync($"api/reactions/resources/{idResource}", request, ct),
            "La reaction a ete creee, mais la reponse du serveur est invalide.",
            ct);
    }

    public async Task<ReactionResponse> UpdateAsync(int idReaction, UpdateReactionRequest request, CancellationToken ct)
    {
        return await SendForPayloadAsync(
            () => _httpClient.PatchAsJsonAsync($"api/reactions/{idReaction}", request, ct),
            "La reaction a ete mise a jour, mais la reponse du serveur est invalide.",
            ct);
    }

    public async Task DeleteAsync(int idReaction, CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.DeleteAsync($"api/reactions/{idReaction}", ct);

            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
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
                throw await ApiClientErrors.FromResponseAsync(response, ct);

            var payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
            return payload ?? fallback;
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
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
                throw await ApiClientErrors.FromResponseAsync(response, ct);

            var payload = await response.Content.ReadFromJsonAsync<ReactionResponse>(cancellationToken: ct);
            return payload ?? throw ApiClientErrors.InvalidResponse(response.StatusCode, emptyPayloadMessage);
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
