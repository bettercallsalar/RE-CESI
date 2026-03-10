using RESR.Core.Errors;
using RESR.Core.Controllers.Roles.Ports;
using RESR.Core.Security.Token;
using RESR.Core.Security.Tools;
using RESR.Core.Controllers.Users.Factories;
using RESR.Core.Controllers.Users.Ports;
using RESR.Models.Users;
using RESR.Models.Permissions;
namespace RESR.Core.Controllers.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserFactory _userFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public UserService(
        IUserRepository repo,
        IRoleRepository roleRepository,
        IUserFactory userFactory,
        IPasswordHasher passwordHasher,
        ITokenService tokenService
    )
    {
        _repo = repo;
        _roleRepository = roleRepository;
        _userFactory = userFactory;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetUsersPaginatedAsync(int page, int pageSize, UserListingFilters filters, CancellationToken ct)
    {
        UserListingFilters normalizedFilters = NormalizeListingFilters(filters);
        IReadOnlyList<User> users = await _repo.GetUsersPaginatedAsync(page, pageSize, normalizedFilters, ct);
        int totalCount = await _repo.CountUsersAsync(normalizedFilters, ct);
        return (users, totalCount);
    }
    public Task<User?> GetByIdAsync(int idUser, CancellationToken ct) => _repo.GetByIdAsync(idUser, ct);

    public async Task<int?> RegisterAsync(RegisterUserCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim();
        var username = cmd.Username.Trim();
        var firstName = cmd.FirstName.Trim();

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ValidationException("First name is required");

        if (cmd.IdDepartment <= 0)
            throw new ValidationException("IdDepartment must be greater than 0");

        if (await _repo.GetByEmailAsync(email, ct) is not null)
            throw new ConflictException("Email already exists");

        if (await _repo.GetByUsernameAsync(username, ct) is not null)
            throw new ConflictException("Username already exists.");

        if (await _roleRepository.GetByIdAsync(cmd.IdRole, ct) is null)
            throw new ValidationException($"Role {cmd.IdRole} does not exist");

        // TODO: check if department exists
        var user = _userFactory.CreateForRegistration(
            username,
            email,
            _passwordHasher.HashPassword(cmd.Password),
            firstName,
            cmd.BirthDate,
            NormalizeOptional(cmd.Bio),
            cmd.IdDepartment,
            cmd.IdRole
        );

        return await _repo.CreateAsync(user, ct);
    }

    public async Task<User> UpdateAsync(UpdateUserCommand cmd, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(cmd.IdUser, ct)
            ?? throw new NotFoundException($"User {cmd.IdUser} not found");

        if (user.DeletedAt is not null)
            throw new ValidationException("User account is deleted");

        var nextEmail = NormalizeOptional(cmd.Email);
        var nextUsername = NormalizeOptional(cmd.Username);

        if (!string.IsNullOrWhiteSpace(nextEmail) &&
            !string.Equals(nextEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _repo.GetByEmailAsync(nextEmail, ct) is not null)
                throw new ConflictException("Email already exists");
        }

        if (!string.IsNullOrWhiteSpace(nextUsername) &&
            !string.Equals(nextUsername, user.Username, StringComparison.OrdinalIgnoreCase))
        {
            if (await _repo.GetByUsernameAsync(nextUsername, ct) is not null)
                throw new ConflictException("Username already exists");
        }

        if (cmd.IdRole is int idRole && idRole != user.IdRole && await _roleRepository.GetByIdAsync(idRole, ct) is null)
            throw new ValidationException($"Role {idRole} does not exist");

        var normalizedCommand = cmd with
        {
            Email = nextEmail,
            Username = nextUsername,
            FirstName = NormalizeOptional(cmd.FirstName),
            Bio = NormalizeOptional(cmd.Bio)
        };

        return await _repo.PatchAsync(normalizedCommand, ct);
    }

    public async Task<User> SetVerificationAsync(SetUserVerificationCommand cmd, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(cmd.IdUser, ct)
            ?? throw new NotFoundException($"User {cmd.IdUser} not found");

        if (user.DeletedAt is not null)
            throw new ValidationException("User account is deleted");

        if (user.IsVerified == cmd.IsVerified)
            return user;

        return await _repo.SetVerificationAsync(cmd.IdUser, cmd.IsVerified, ct);
    }

    public Task<bool> SoftDeleteAsync(int idUser, CancellationToken ct) => _repo.SoftDeleteAsync(idUser, ct);

    public async Task<string?> LoginUserAsync(Login loginDto, CancellationToken ct)
    {
        var email = loginDto.Email.Trim();
        var user = await _repo.GetByEmailAsync(email, ct)
            ?? throw new InvalidOperationException("Invalid email or password");

        if (!_passwordHasher.VerifyPassword(user.HashedPassword, loginDto.Password))
            throw new InvalidOperationException("Invalid email or password");
        if (!user.IsVerified) throw new InvalidOperationException("User email is not verified");
        if (user.DeletedAt is not null) throw new InvalidOperationException("User account is deleted");

        IReadOnlyList<Permission> permissions = await _roleRepository.GetPermissionsByRoleIdAsync(user.IdRole, ct);

        return _tokenService.GenerateUserToken(user, permissions);
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    private static UserListingFilters NormalizeListingFilters(UserListingFilters filters)
    {
        // Remove duplicates and invalid ids from filters
        var departmentIds = filters.DepartmentIds?.Where(id => id > 0).Distinct().ToArray();
        var roleIds = filters.RoleIds?.Where(id => id > 0).Distinct().ToArray();

        return filters with
        {
            Keyword = NormalizeOptional(filters.Keyword),
            DepartmentIds = departmentIds is { Length: > 0 } ? departmentIds : null,
            RoleIds = roleIds is { Length: > 0 } ? roleIds : null
        };
    }
}
