using System.Security.Cryptography;
using System.Text;
using RESR.Core.Users.Ports;
using RESR.Models.Users;

namespace RESR.Core.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo) => _repo = repo;

    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);
    public Task<User?> GetByIdAsync(int idUser, CancellationToken ct) => _repo.GetByIdAsync(idUser, ct);

    public async Task<int?> RegisterAsync(RegisterUserCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim();
        var username = cmd.Username.Trim();

        if (await _repo.GetByEmailAsync(email, ct) is not null)
            throw new InvalidOperationException("Email already exists");

        if (await _repo.GetByUsernameAsync(username, ct) is not null)
            throw new InvalidOperationException("Username already exists");

        var user = new User
        {
            Username = username,
            Email = email,
            HashedPassword = HashPassword(cmd.Password),
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

    public Task<bool> SoftDeleteAsync(int idUser, CancellationToken ct) => _repo.SoftDeleteAsync(idUser, ct);

    private static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}