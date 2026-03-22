using System.Net.Http.Headers;
using System.Net.Http.Json;
using RESR.Models.Comments;

namespace RESR.MAUI.Services;

public sealed class CommentsApiClient : ICommentsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiSession _session;

    public CommentsApiClient(HttpClient httpClient, IApiSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<IReadOnlyList<CommentResponse>> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        return await SendAsync(
            () => _httpClient.GetAsync($"api/comments/resources/{idResource}", ct),
            Array.Empty<CommentResponse>(),
            ct);
    }

    public async Task<CommentResponse> CreateAsync(int idResource, CreateCommentRequest request, CancellationToken ct)
    {
        ApplyAuthorizationHeader();

        try
        {
            using var response = await _httpClient.PostAsJsonAsync($"api/comments/resources/{idResource}", request, ct);
            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);

            var payload = await response.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: ct);
            return payload ?? throw ApiClientErrors.InvalidResponse(response.StatusCode, "Le commentaire a ete cree, mais la reponse du serveur est invalide.");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
        }
    }

    private async Task<TResponse> SendAsync<TResponse>(
        Func<Task<HttpResponseMessage>> action,
        TResponse fallback,
        CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
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

    private void ApplyAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(_session.Token)
            ? null
            : new AuthenticationHeaderValue("Bearer", _session.Token);
    }
}
