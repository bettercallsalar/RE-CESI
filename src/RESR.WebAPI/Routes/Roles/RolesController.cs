using Microsoft.AspNetCore.Mvc;
using RESR.Core.Errors;
using RESR.Core.Controllers.Roles;
using RESR.Models.Permissions;
using RESR.Models.Roles;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Roles;

[ApiController]
[Route("api/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service) => _service = service;

    [AuthorizePermission([PermissionNames.ManageRoles, PermissionNames.ManageUsers])]
    [HttpGet("assignable")]
    public async Task<ActionResult<IReadOnlyList<RoleSummaryResponse>>> GetAssignableRoles(CancellationToken ct)
    {
        var roles = await _service.GetAllAsync(ct);

        return Ok(roles
            .Select(role => new RoleSummaryResponse(role.IdRole, role.Name, role.Description))
            .ToList());
    }

    [AuthorizeRole(RoleIds.SuperAdmin)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetAll(CancellationToken ct)
    {
        var roles = await _service.GetAllAsync(ct);
        var responses = new List<RoleResponse>(roles.Count);

        foreach (var role in roles)
        {
            var permissions = await _service.GetPermissionsByRoleIdAsync(role.IdRole, ct);
            responses.Add(ToResponse(role, permissions));
        }

        return Ok(responses);
    }

    [AuthorizeRole(RoleIds.SuperAdmin)]
    [HttpGet("{idRole:int}")]
    public async Task<ActionResult<RoleResponse>> GetById([FromRoute] int idRole, CancellationToken ct)
    {
        var role = await _service.GetByIdAsync(idRole, ct);

        if (role is null)
            return NotFound();

        var permissions = await _service.GetPermissionsByRoleIdAsync(idRole, ct);
        return Ok(ToResponse(role, permissions));
    }

    [AuthorizeRole(RoleIds.SuperAdmin)]
    [HttpGet("{idRole:int}/permissions")]
    public async Task<ActionResult<IReadOnlyList<PermissionResponse>>> GetRolePermissions([FromRoute] int idRole, CancellationToken ct)
    {
        var role = await _service.GetByIdAsync(idRole, ct);
        if (role is null)
            return NotFound(new { message = $"Role {idRole} not found" });

        var permissions = await _service.GetPermissionsByRoleIdAsync(idRole, ct);
        return Ok(permissions.Select(p => new PermissionResponse(p.IdPermission, p.Name, p.Description)).ToList());
    }

    [AuthorizeRole(RoleIds.SuperAdmin)]
    [HttpPost("{idRole:int}/permissions/{idPermission:int}")]
    public async Task<ActionResult> AddPermissionToRole([FromRoute] int idRole, [FromRoute] int idPermission, CancellationToken ct)
    {
        try
        {
            await _service.AddPermissionToRoleAsync(idRole, idPermission, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [AuthorizeRole(RoleIds.SuperAdmin)]
    [HttpDelete("{idRole:int}/permissions/{idPermission:int}")]
    public async Task<ActionResult> RemovePermissionFromRole([FromRoute] int idRole, [FromRoute] int idPermission, CancellationToken ct)
    {
        try
        {
            await _service.RemovePermissionFromRoleAsync(idRole, idPermission, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private static RoleResponse ToResponse(Role role, IReadOnlyList<Permission> permissions) =>
        new RoleResponse(
            role.IdRole,
            role.Name,
            role.Description,
            permissions.Select(p => new PermissionResponse(p.IdPermission, p.Name, p.Description)).ToList()
        );
}
