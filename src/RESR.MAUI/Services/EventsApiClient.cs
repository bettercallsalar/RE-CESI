using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using RESR.Models.Resources;

namespace RESR.MAUI.Services;

public sealed class EventsApiClient : IEventsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiSession _session;

    public EventsApiClient(HttpClient httpClient, IApiSession session)
    {
        _httpClient = httpClient;
        _session = session;
    }

    public async Task<EventResponse> GetByIdAsync(int idResource, CancellationToken ct)
    {
        return await GetAsync($"api/events/{idResource}", "La reponse de l'evenement est invalide.", ct);
    }

    public async Task<EventResponse> GetOwnByIdAsync(int idResource, CancellationToken ct)
    {
        return await GetAsync($"api/events/me/{idResource}", "La reponse de votre evenement est invalide.", ct);
    }

    private async Task<EventResponse> GetAsync(string uri, string emptyMessage, CancellationToken ct)
    {
        try
        {
            ApplyAuthorizationHeader();
            using var response = await _httpClient.GetAsync(uri, ct);

            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);

            await using var responseStream = await response.Content.ReadAsStreamAsync(ct);
            var @event = await JsonSerializer.DeserializeAsync<EventResponse>(
                responseStream,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                ct);
            return @event ?? throw ApiClientErrors.InvalidResponse(System.Net.HttpStatusCode.InternalServerError, emptyMessage);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
        }
    }

    public async Task CreateAsync(
        CreateEventRequest request,
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
                { new StringContent(request.StartDate.ToString("O", CultureInfo.InvariantCulture)), "StartDate" }
            };

            AddIfNotEmpty(content, "Description", request.Description);
            AddIfNotEmpty(content, "Subtitle", request.Subtitle);
            AddIfNotEmpty(content, "Address", request.Address);

            if (request.EndDate is not null)
            {
                content.Add(new StringContent(request.EndDate.Value.ToString("O", CultureInfo.InvariantCulture)), "EndDate");
            }

            if (request.IdDepartment is int idDepartment)
            {
                content.Add(new StringContent(idDepartment.ToString(CultureInfo.InvariantCulture)), "IdDepartment");
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

            using var response = await _httpClient.PostAsync("api/events", content, ct);

            if (!response.IsSuccessStatusCode)
                throw await ApiClientErrors.FromResponseAsync(response, ct);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
        }
    }

    public async Task UpdateAsync(
        int idResource,
        UpdateEventRequest request,
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
            if (request.Subtitle is not null)
                content.Add(new StringContent(request.Subtitle), "Subtitle");
            if (request.StartDate is DateTime startDate)
                content.Add(new StringContent(startDate.ToString("O", CultureInfo.InvariantCulture)), "StartDate");
            if (request.EndDate is DateTime endDate)
                content.Add(new StringContent(endDate.ToString("O", CultureInfo.InvariantCulture)), "EndDate");
            if (request.Address is not null)
                content.Add(new StringContent(request.Address), "Address");
            if (request.IdDepartment is int idDepartment)
                content.Add(new StringContent(idDepartment.ToString(CultureInfo.InvariantCulture)), "IdDepartment");

            content.Add(new StringContent((images.Count > 0).ToString()), "ReplaceImages");

            if (defaultImageIndex is int index && images.Count > 0)
                content.Add(new StringContent(index.ToString(CultureInfo.InvariantCulture)), "DefaultImageIndex");

            foreach (var image in images)
            {
                var imageContent = new ByteArrayContent(image.Content);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
                content.Add(imageContent, "Images", image.FileName);
            }

            using var requestMessage = new HttpRequestMessage(HttpMethod.Patch, $"api/events/{idResource}")
            {
                Content = content
            };
            using var response = await _httpClient.SendAsync(requestMessage, ct);

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

    private static void AddIfNotEmpty(MultipartFormDataContent content, string fieldName, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            content.Add(new StringContent(value.Trim()), fieldName);
        }
    }
}
