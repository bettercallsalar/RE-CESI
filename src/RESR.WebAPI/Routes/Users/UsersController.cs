using Microsoft.AspNetCore.Mvc;
using RESR.Core.Errors;
using RESR.Core.Controllers.Users;
using RESR.Models.Users;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Users;

[Route("api/[controller]")]
[ApiController]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    [AuthorizePermission(PermissionNames.ManageUsers)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken ct)
    {
        var users = await _service.GetAllAsync(ct);
        return Ok(users.Select(ToResponse).ToList());
    }

    [AuthorizePermissionOrSelf("idUser", PermissionNames.ManageUsers)]
    [HttpGet("{idUser:int}")]
    public async Task<ActionResult<UserResponse>> GetById([FromRoute] int idUser, CancellationToken ct)
    {
        var user = await _service.GetByIdAsync(idUser, ct);
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterUserRequest req, CancellationToken ct)
    {
        try
        {
            var id = await _service.RegisterAsync(
                new RegisterUserCommand(req.Username, req.Email, req.Password, req.FirstName, req.BirthDate, req.Bio, req.IdDepartment, req.IdRole),
                ct
            );

            return CreatedAtAction(
                nameof(GetById),
                new { idUser = id },
                new
                {
                    message = "User registered successfully",
                    userId = id
                }
            );
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AuthorizePermissionOrSelf("idUser")]
    [HttpPatch("{idUser:int}/verification")]
    public async Task<ActionResult<UserResponse>> SetVerification([FromRoute] int idUser, CancellationToken ct)
    {
        try
        {
            var user = await _service.SetVerificationAsync(new SetUserVerificationCommand(IdUser: idUser, IsVerified: true), ct);
            return Ok(ToResponse(user));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AuthorizePermission(PermissionNames.ManageUsers)]
    [HttpPatch("{idUser:int}")]
    public async Task<ActionResult> Update([FromRoute] int idUser, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        try
        {
            User user = await _service.UpdateAsync(
                new UpdateUserCommand(IdUser: idUser, IdRole: req.IdRole),
                ct
            );
            return Ok(ToResponse(user));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message, userId = idUser, StatusCode = 404 });
        }
    }

    [AuthorizePermissionOrSelf("idUser")]
    [HttpPatch("{idUser:int}/profile")]
    public async Task<ActionResult> UpdateOwnProfile([FromRoute] int idUser, [FromBody] UpdateOwnProfileRequest req, CancellationToken ct)
    {
        try
        {
            User user = await _service.UpdateAsync(
                new UpdateUserCommand(
                    IdUser: idUser,
                    Username: req.Username,
                    Email: req.Email,
                    FirstName: req.FirstName,
                    BirthDate: req.BirthDate,
                    Bio: req.Bio,
                    IdDepartment: req.IdDepartment
                    ),
                ct
            );
            return Ok(ToResponse(user));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [AuthorizePermission(PermissionNames.ManageUsers)]
    [HttpDelete("{idUser:int}")]
    public async Task<ActionResult> SoftDelete([FromRoute] int idUser, CancellationToken ct)
    {
        var ok = await _service.SoftDeleteAsync(idUser, ct);
        return ok ? NoContent() : NotFound();
    }

    private static UserResponse ToResponse(RESR.Models.Users.User u) => new(
        u.IdUser,
        u.Username,
        u.Email,
        u.FirstName,
        u.BirthDate,
        u.Bio,
        u.IsVerified,
        u.IdDepartment,
        u.IdRole
    );
}
