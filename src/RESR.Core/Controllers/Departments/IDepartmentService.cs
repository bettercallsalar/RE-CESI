using RESR.Models.Departments;

namespace RESR.Core.Controllers.Departments;

public interface IDepartmentService
{
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct);
    Task<Department?> GetByIdAsync(int idDepartment, CancellationToken ct);
}