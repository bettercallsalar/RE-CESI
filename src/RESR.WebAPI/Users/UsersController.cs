using Microsoft.AspNetCore.Mvc;
using RESR.Core.Errors;
using RESR.Core.Users;
using RESR.Models.Users;

namespace RESR.WebAPI.Users;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken ct)
    {
        var users = await _service.GetAllAsync(ct);
        return Ok(users.Select(ToResponse).ToList());
    }

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
    }

    [HttpPatch("{idUser:int}")]
    public async Task<ActionResult> Update([FromRoute] int idUser, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        try
        {
            User user = await _service.UpdateAsync(
                new UpdateUserCommand(idUser, req.Username, req.Email, req.FirstName, req.BirthDate, req.IsVerified, req.Bio, req.IdDepartment, req.IdRole),
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
