using Microsoft.AspNetCore.Mvc;
using RESR.Core.Roles;
using RESR.Models.Roles;

namespace RESR.WebAPI.Roles;

[ApiController]
[Route("api/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetAll(CancellationToken ct)
    {
        var roles = await _service.GetAllAsync(ct);
        return Ok(roles.Select(ToResponse).ToList());
    }

    private static RoleResponse ToResponse(Role r) =>
        new RoleResponse(
            r.IdRole,
            r.Name
        );
}