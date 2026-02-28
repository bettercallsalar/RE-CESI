using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Permissions;
using RESR.Models.Permissions;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Permissions;

[ApiController]
[Route("api/permissions")]
public sealed class PermissionsController : ControllerBase
{
    private readonly IPermissionService _service;

    public PermissionsController(IPermissionService service) => _service = service;

    [AuthorizeToken(TokenRole.Admin)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PermissionResponse>>> GetAll(CancellationToken ct)
    {
        var permissions = await _service.GetAllAsync(ct);
        return Ok(permissions.Select(ToResponse).ToList());
    }

    [AuthorizeToken(TokenRole.Admin)]
    [HttpGet("{idPermission:int}")]
    public async Task<ActionResult<PermissionResponse>> GetById([FromRoute] int idPermission, CancellationToken ct)
    {
        var permission = await _service.GetByIdAsync(idPermission, ct);
        return permission is null ? NotFound() : Ok(ToResponse(permission));
    }

    private static PermissionResponse ToResponse(Permission permission) =>
        new(permission.IdPermission, permission.Name, permission.Description);
}
