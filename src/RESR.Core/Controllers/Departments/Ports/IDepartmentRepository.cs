using RESR.Models.Departments;

namespace RESR.Core.Controllers.Departments.Ports;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct);
    Task<Department?> GetByIdAsync(int idDepartment, CancellationToken ct);
}