using System.Net.Http.Json;
using RESR.Models.Categories;

namespace RESR.MAUI.Services;

public sealed class CategoriesApiClient : ICategoriesApiClient
{
    private readonly HttpClient _httpClient;

    public CategoriesApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(CancellationToken ct)
    {
        try
        {
            using var response = await _httpClient.GetAsync("api/categories", ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }

            var categories = await response.Content.ReadFromJsonAsync<IReadOnlyList<CategoryResponse>>(cancellationToken: ct);
            return categories ?? Array.Empty<CategoryResponse>();
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
    }
}
