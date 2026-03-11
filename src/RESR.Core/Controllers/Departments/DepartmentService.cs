using RESR.Core.Controllers.Departments.Ports;
using RESR.Models.Departments;

namespace RESR.Core.Controllers.Departments;

public sealed class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;

    public DepartmentService(IDepartmentRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);

    public Task<Department?> GetByIdAsync(int idDepartment, CancellationToken ct) => _repo.GetByIdAsync(idDepartment, ct);
}