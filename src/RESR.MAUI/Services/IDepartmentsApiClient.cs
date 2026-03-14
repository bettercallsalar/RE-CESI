using RESR.Models.Departments;

namespace RESR.MAUI.Services;

public interface IDepartmentsApiClient
{
    Task<IReadOnlyList<DepartmentResponse>> GetDepartmentsAsync(CancellationToken ct);
}
