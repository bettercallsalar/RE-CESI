using RESR.Core.Errors;
using RESR.Core.Controllers.Roles.Ports;
using RESR.Core.Security.Token;
using RESR.Core.Security.Tools;
using RESR.Core.Controllers.Users.Factories;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Controllers.Departments.Ports;
using RESR.Models.Users;
using RESR.Models.Departments;
using RESR.Models.Permissions;
namespace RESR.Core.Controllers.Users;

public sealed class UserService : IUserService
{
    private const int StandardUserRoleId = 1;
    private readonly IUserRepository _repo;
    private readonly IRoleRepository _roleRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUserFactory _userFactory;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public UserService(
        IUserRepository repo,
        IRoleRepository roleRepository,
        IDepartmentRepository departmentRepository,
        IUserFactory userFactory,
        IPasswordHasher passwordHasher,
        ITokenService tokenService
    )
    {
        _repo = repo;
        _roleRepository = roleRepository;
        _departmentRepository = departmentRepository;
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

    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetManageableUsersPaginatedAsync(int page, int pageSize, UserListingFilters filters, CancellationToken ct)
    {
        UserListingFilters normalizedFilters = NormalizeListingFilters(filters) with
        {
            RoleIds = new[] { StandardUserRoleId },
            IncludeDeleted = false
        };

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
            throw new ValidationException("Le prenom est obligatoire.");

        if (cmd.IdDepartment <= 0)
            throw new ValidationException("Le departement doit etre superieur a 0.");

        if (await _repo.GetByEmailAsync(email, ct) is not null)
            throw new ConflictException("Cette adresse e-mail existe deja.");

        if (await _repo.GetByUsernameAsync(username, ct) is not null)
            throw new ConflictException("Ce nom d'utilisateur existe deja.");

        if (await _roleRepository.GetByIdAsync(cmd.IdRole, ct) is null)
            throw new ValidationException($"Le role {cmd.IdRole} n'existe pas.");

        Department department = await _departmentRepository.GetByIdAsync(cmd.IdDepartment, ct)
            ?? throw new ValidationException($"Le departement {cmd.IdDepartment} n'existe pas.");

        var user = _userFactory.CreateForRegistration(
            username,
            email,
            _passwordHasher.HashPassword(cmd.Password),
            firstName,
            cmd.BirthDate,
            NormalizeOptional(cmd.Bio),
            department,
            cmd.IdRole
        );

        return await _repo.CreateAsync(user, ct);
    }

    public async Task<User> UpdateAsync(UpdateUserCommand cmd, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(cmd.IdUser, ct)
            ?? throw new NotFoundException($"Utilisateur {cmd.IdUser} introuvable.");

        if (user.DeletedAt is not null)
            throw new ValidationException("Le compte utilisateur est supprime.");

        var nextEmail = NormalizeOptional(cmd.Email);
        var nextUsername = NormalizeOptional(cmd.Username);

        if (!string.IsNullOrWhiteSpace(nextEmail) &&
            !string.Equals(nextEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _repo.GetByEmailAsync(nextEmail, ct) is not null)
                throw new ConflictException("Cette adresse e-mail existe deja.");
        }

        if (!string.IsNullOrWhiteSpace(nextUsername) &&
            !string.Equals(nextUsername, user.Username, StringComparison.OrdinalIgnoreCase))
        {
            if (await _repo.GetByUsernameAsync(nextUsername, ct) is not null)
                throw new ConflictException("Ce nom d'utilisateur existe deja.");
        }

        if (cmd.IdRole is int idRole && idRole != user.IdRole && await _roleRepository.GetByIdAsync(idRole, ct) is null)
            throw new ValidationException($"Le role {idRole} n'existe pas.");

        if (cmd.IdDepartment is int idDepartment)
        {
            if (idDepartment <= 0)
                throw new ValidationException("Le departement doit etre superieur a 0.");

            if (idDepartment != user.Department.IdDepartment &&
                await _departmentRepository.GetByIdAsync(idDepartment, ct) is null)
            {
                throw new ValidationException($"Le departement {idDepartment} n'existe pas.");
            }
        }

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
            ?? throw new NotFoundException($"Utilisateur {cmd.IdUser} introuvable.");

        if (user.DeletedAt is not null)
            throw new ValidationException("Le compte utilisateur est supprime.");

        if (user.IsBanned)
            throw new ValidationException("Le compte utilisateur est banni.");

        if (user.IsVerified == cmd.IsVerified)
            return user;

        return await _repo.SetVerificationAsync(cmd.IdUser, cmd.IsVerified, ct);
    }

    public Task<bool> SoftDeleteAsync(int idUser, CancellationToken ct) => _repo.SoftDeleteAsync(idUser, ct);

    public async Task SetManageableUserBanStatusAsync(int idUser, bool isBanned, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(idUser, ct);
        if (user is null || user.IdRole != StandardUserRoleId)
            throw new NotFoundException($"User {idUser} not found.");

        if (user.IsBanned == isBanned)
            return;

        await _repo.SetBannedAsync(idUser, isBanned, ct);
    }

    public async Task BanManageableUserAsync(int idUser, CancellationToken ct)
    {
        await SetManageableUserBanStatusAsync(idUser, true, ct);
    }

    public async Task<string?> LoginUserAsync(Login loginDto, CancellationToken ct)
    {
        var email = loginDto.Email.Trim();
        var user = await _repo.GetByEmailAsync(email, ct)
            ?? throw new InvalidOperationException("Adresse e-mail ou mot de passe invalide.");

        if (!_passwordHasher.VerifyPassword(user.HashedPassword, loginDto.Password))
            throw new InvalidOperationException("Adresse e-mail ou mot de passe invalide.");
        if (!user.IsVerified) throw new InvalidOperationException("L'adresse e-mail du compte n'est pas verifiee.");
        if (user.DeletedAt is not null) throw new InvalidOperationException("Le compte utilisateur est supprime.");
        if (user.IsBanned) throw new InvalidOperationException("Le compte utilisateur est banni.");

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
