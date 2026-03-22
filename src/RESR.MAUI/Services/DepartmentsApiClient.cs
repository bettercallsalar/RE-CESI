using System.Net.Http.Json;
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
                throw await ApiClientErrors.FromResponseAsync(response, ct);

            var departments = await response.Content.ReadFromJsonAsync<IReadOnlyList<DepartmentResponse>>(cancellationToken: ct);
            return departments ?? Array.Empty<DepartmentResponse>();
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiClientErrors.Timeout();
        }
    }
}
