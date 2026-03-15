using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using RESR.Models.Resources;

namespace RESR.MAUI.Services;

public sealed class ArticlesApiClient : IArticlesApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiSession _session;

    public ArticlesApiClient(HttpClient httpClient, IApiSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<ArticleResponse> GetByIdAsync(int idResource, CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.GetAsync($"api/articles/{idResource}", ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(body)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : body;

                throw new ApiException(response.StatusCode, message);
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(ct);
            var article = await JsonSerializer.DeserializeAsync<ArticleResponse>(
                responseStream,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                ct);
            return article ?? throw new ApiException(System.Net.HttpStatusCode.InternalServerError, "Article response was empty.");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
    }

    public async Task CreateAsync(
        CreateArticleRequest request,
        IReadOnlyList<SelectedImageUpload> images,
        int? defaultImageIndex,
        CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var content = new MultipartFormDataContent
            {
                { new StringContent(request.Title), "Title" },
                { new StringContent(request.Visibility), "Visibility" },
                { new StringContent(request.IdCategory.ToString(CultureInfo.InvariantCulture)), "IdCategory" },
                { new StringContent(request.Content), "Content" }
            };

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                content.Add(new StringContent(request.Description.Trim()), "Description");
            }

            if (defaultImageIndex is int index)
            {
                content.Add(new StringContent(index.ToString(CultureInfo.InvariantCulture)), "DefaultImageIndex");
            }

            foreach (var image in images)
            {
                var imageContent = new ByteArrayContent(image.Content);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                content.Add(imageContent, "Images", image.FileName);
            }

            using var response = await _httpClient.PostAsync("api/articles", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(body)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : body;

                throw new ApiException(response.StatusCode, message);
            }
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
    }

    public async Task UpdateAsync(
        int idResource,
        UpdateArticleRequest request,
        IReadOnlyList<SelectedImageUpload> images,
        int? defaultImageIndex,
        CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var content = new MultipartFormDataContent();

            if (request.Title is not null)
                content.Add(new StringContent(request.Title), "Title");
            if (request.Description is not null)
                content.Add(new StringContent(request.Description), "Description");
            if (request.Visibility is not null)
                content.Add(new StringContent(request.Visibility), "Visibility");
            if (request.IdCategory is int idCategory)
                content.Add(new StringContent(idCategory.ToString(CultureInfo.InvariantCulture)), "IdCategory");
            if (request.Content is not null)
                content.Add(new StringContent(request.Content), "Content");

            content.Add(new StringContent((images.Count > 0).ToString()), "ReplaceImages");

            if (defaultImageIndex is int index && images.Count > 0)
                content.Add(new StringContent(index.ToString(CultureInfo.InvariantCulture)), "DefaultImageIndex");

            foreach (var image in images)
            {
                var imageContent = new ByteArrayContent(image.Content);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                content.Add(imageContent, "Images", image.FileName);
            }

            using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"api/articles/{idResource}")
            {
                Content = content
            };
            using var response = await _httpClient.SendAsync(requestMessage, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(body)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : body;

                throw new ApiException(response.StatusCode, message);
            }
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
