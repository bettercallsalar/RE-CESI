using Microsoft.AspNetCore.Mvc;
using RESR.Core.Errors;
using RESR.Core.Controllers.Users;
using RESR.Models.Users;
using RESR.WebAPI.Security;
using RESR.Core.Security.Token;

namespace RESR.WebAPI.Routes.Users;

[Route("api/[controller]")]
[ApiController]
public sealed class UsersController : AuthenticatedResourceControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service, ITokenService tokenService)
        : base(tokenService)
    {
        _service = service;
    }
    [AuthorizePermission(PermissionNames.ManageUsers)]
    [HttpGet]
    public async Task<ActionResult<PaginatedUsersResponse>> GetUsersPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] List<int>? departmentIds = null,
        [FromQuery] List<int>? roleIds = null,
        [FromQuery] DateOnly? birthDate = null,
        [FromQuery] bool? isVerified = null,
        [FromQuery] bool includeDeleted = false,
        CancellationToken ct = default)
    {
        if (page <= 0 || departmentIds is not null && departmentIds.Any(id => id <= 0) || roleIds is not null && roleIds.Any(id => id <= 0))
            return BadRequest(new { message = "Page number, DepartmentIds or RoleIds must be greater than 0" });

        var filters = new UserListingFilters(
            Keyword: keyword,
            DepartmentIds: departmentIds,
            RoleIds: roleIds,
            BirthDate: birthDate,
            IsVerified: isVerified,
            IncludeDeleted: includeDeleted
        );

        var (users, totalCount) = await _service.GetUsersPaginatedAsync(page, pageSize, filters, ct);
        var items = users.Select(ToResponse).ToList();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedUsersResponse(items, page, pageSize, totalCount, totalPages));
    }

    [AuthorizePermission(PermissionNames.ManageUsers)]
    [HttpGet("manageable")]
    public async Task<ActionResult<PaginatedUsersResponse>> GetManageableUsersPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] List<int>? departmentIds = null,
        [FromQuery] DateOnly? birthDate = null,
        [FromQuery] bool? isVerified = null,
        CancellationToken ct = default)
    {
        if (page <= 0 || departmentIds is not null && departmentIds.Any(id => id <= 0))
            return BadRequest(new { message = "Page number or DepartmentIds must be greater than 0" });

        var filters = new UserListingFilters(
            Keyword: keyword,
            DepartmentIds: departmentIds,
            RoleIds: null,
            BirthDate: birthDate,
            IsVerified: isVerified,
            IncludeDeleted: false
        );

        var (users, totalCount) = await _service.GetManageableUsersPaginatedAsync(page, pageSize, filters, ct);
        var items = users.Select(ToResponse).ToList();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedUsersResponse(items, page, pageSize, totalCount, totalPages));
    }

    [AuthorizePermission(PermissionNames.ManageUsers)]
    [HttpGet("{idUser:int}")]
    public async Task<ActionResult<UserResponse>> GetById([FromRoute] int idUser, CancellationToken ct)
    {
        var user = await _service.GetByIdAsync(idUser, ct);
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [HttpGet("{idUser:int}/profile")]
    public async Task<ActionResult<PublicUserProfileResponse>> GetPublicProfile([FromRoute] int idUser, CancellationToken ct)
    {
        var (authResult, _) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        var user = await _service.GetByIdAsync(idUser, ct);
        return user is null ? NotFound() : Ok(ToPublicProfileResponse(user));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetOwnProfile(CancellationToken ct)
    {
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        var user = await _service.GetByIdAsync(idUser, ct);
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterUserRequest req, CancellationToken ct)
    {
        try
        {
            var id = await _service.RegisterAsync(
                new RegisterUserCommand(req.Username, req.Email, req.Password, req.FirstName, req.BirthDate, req.Bio, req.IdDepartment),
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

    //[AuthorizePermissionOrSelf("idUser")]
    [HttpPatch("/verification/{idUser:int}")]
    public async Task<ActionResult<UserResponse>> SetVerification([FromRoute] int idUser, CancellationToken ct)
    {
        try
        {
            // TODO : only allow if the user is really verified with an email or something like that
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

    [AuthorizePermission([PermissionNames.ManageRoles, PermissionNames.ManageUsers])]
    [HttpPatch("{idUser:int}")]
    public async Task<ActionResult> UpdateRoleOfUser([FromRoute] int idUser, [FromBody] UpdateUserRequest req, CancellationToken ct)
    {
        try
        {
            if (req.IdRole is null)
                return BadRequest(new { message = "IdRole is required" });
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

    [HttpPatch("modify-profile")]
    public async Task<ActionResult> UpdateOwnProfile([FromBody] UpdateOwnProfileRequest req, CancellationToken ct)
    {
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;
        if (req.Username is null && req.Email is null && req.FirstName is null && req.BirthDate is null && req.Bio is null && req.IdDepartment is null)
            return BadRequest(new { message = "Au moins un champ doit etre fourni pour la mise a jour." });
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
                    ClearBio: req.Bio is not null && string.IsNullOrWhiteSpace(req.Bio),
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

    [HttpDelete("me")]
    public async Task<ActionResult> SoftDelete(CancellationToken ct)
    {
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        var ok = await _service.SoftDeleteAsync(idUser, ct);
        return ok ? NoContent() : NotFound();
    }

    [AuthorizePermission(PermissionNames.BanUser)]
    [HttpPatch("manageable/{idUser:int}/ban")]
    public async Task<ActionResult> SetManageableUserBanStatus([FromRoute] int idUser, [FromBody] SetUserBanRequest req, CancellationToken ct)
    {
        try
        {
            await _service.SetManageableUserBanStatusAsync(idUser, req.IsBanned, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [AuthorizePermission(PermissionNames.BanUser)]
    [HttpDelete("manageable/{idUser:int}/ban")]
    public async Task<ActionResult> BanManageableUser([FromRoute] int idUser, CancellationToken ct)
    {
        try
        {
            await _service.BanManageableUserAsync(idUser, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private static UserResponse ToResponse(RESR.Models.Users.User u) => new(
        u.IdUser,
        u.Username,
        u.Email,
        u.FirstName,
        u.BirthDate,
        u.Bio,
        u.IsVerified,
        u.IsBanned,
        u.Department,
        u.IdRole
    );

    private static PublicUserProfileResponse ToPublicProfileResponse(RESR.Models.Users.User u) => new(
        u.IdUser,
        u.Username,
        u.FirstName,
        u.Bio,
        u.IsVerified,
        u.Department
    );
}
