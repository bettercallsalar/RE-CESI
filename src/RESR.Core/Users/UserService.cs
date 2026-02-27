using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Core.Security.Tools;
using RESR.Core.Users.Ports;
using RESR.Models.Users;
namespace RESR.Core.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public UserService(
        IUserRepository repo,
        IPasswordHasher passwordHasher,
        ITokenService tokenService
    )
    {
        _repo = repo;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);
    public Task<User?> GetByIdAsync(int idUser, CancellationToken ct) => _repo.GetByIdAsync(idUser, ct);

    public async Task<int?> RegisterAsync(RegisterUserCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim();
        var username = cmd.Username.Trim();

        if (await _repo.GetByEmailAsync(email, ct) is not null)
            throw new ConflictException("Email already exists");

        if (await _repo.GetByUsernameAsync(username, ct) is not null)
            throw new ConflictException("Username already exists.");

        var user = new User
        {
            Username = username,
            Email = email,
            HashedPassword = _passwordHasher.HashPassword(cmd.Password),
            FirstName = cmd.FirstName?.Trim(),
            BirthDate = cmd.BirthDate,
            Bio = cmd.Bio,
            IdDepartment = cmd.IdDepartment,
            IdRole = cmd.IdRole,
            IsVerified = false,
            DeletedAt = null
        };

        return await _repo.CreateAsync(user, ct);
    }

    public async Task<User> UpdateAsync(UpdateUserCommand cmd, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(cmd.IdUser, ct)
            ?? throw new NotFoundException($"User {cmd.IdUser} not found");

        if (user.DeletedAt is not null)
            throw new ValidationException("User account is deleted");

        if (!string.IsNullOrWhiteSpace(cmd.Email) &&
            !string.Equals(cmd.Email.Trim(), user.Email, StringComparison.OrdinalIgnoreCase))
        {
            if (await _repo.GetByEmailAsync(cmd.Email.Trim(), ct) is not null)
                throw new ConflictException("Email already exists");
        }

        if (!string.IsNullOrWhiteSpace(cmd.Username) &&
            !string.Equals(cmd.Username.Trim(), user.Username, StringComparison.OrdinalIgnoreCase))
        {
            if (await _repo.GetByUsernameAsync(cmd.Username.Trim(), ct) is not null)
                throw new ConflictException("Username already exists");
        }

        return await _repo.PatchAsync(cmd, ct);
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

        return _tokenService.GenerateUserToken(user);
    }
}
