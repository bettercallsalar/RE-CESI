using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Departments;
using RESR.Models.Departments;

namespace RESR.WebAPI.Routes.Departments;

[ApiController]
[Route("api/departments")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DepartmentResponse>>> GetAll(CancellationToken ct)
    {
        var departments = await _service.GetAllAsync(ct);
        return Ok(departments.Select(ToResponse).ToList());
    }

    [HttpGet("{idDepartment:int}")]
    public async Task<ActionResult<DepartmentResponse>> GetById([FromRoute] int idDepartment, CancellationToken ct)
    {
        var department = await _service.GetByIdAsync(idDepartment, ct);
        return department is null ? NotFound() : Ok(ToResponse(department));
    }

    private static DepartmentResponse ToResponse(Department department) =>
        new(department.IdDepartment, department.Name, department.Code);
}