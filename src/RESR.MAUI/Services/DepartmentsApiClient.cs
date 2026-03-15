using System.Text.Json;
using RESR.Models.Departments;

namespace RESR.MAUI.Services;

public sealed class DepartmentsApiClient : IDepartmentsApiClient
{
    private readonly HttpClient _httpClient;

    public DepartmentsApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<DepartmentResponse>> GetDepartmentsAsync(CancellationToken ct)
    {
        try
        {
            using var response = await _httpClient.GetAsync("api/departments", ct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var message = string.IsNullOrWhiteSpace(content)
                    ? $"API call failed with status {(int)response.StatusCode}."
                    : content;

                throw new ApiException(response.StatusCode, message);
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(ct);
            var departments = await JsonSerializer.DeserializeAsync<IReadOnlyList<DepartmentResponse>>(
                responseStream,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                ct);
            return departments ?? Array.Empty<DepartmentResponse>();
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException("API call timed out.");
        }
    }
}
