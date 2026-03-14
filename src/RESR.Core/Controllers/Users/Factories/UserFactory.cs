using RESR.Models.Departments;
using RESR.Models.Users;

namespace RESR.Core.Controllers.Users.Factories;

public sealed class UserFactory : IUserFactory
{
    public User CreateForRegistration(
        string username,
        string email,
        string hashedPassword,
        string firstName,
        DateOnly? birthDate,
        string? bio,
        Department department,
        int idRole
    ) =>
        new()
        {
            Username = username,
            Email = email,
            HashedPassword = hashedPassword,
            FirstName = firstName,
            BirthDate = birthDate,
            Bio = bio,
            Department = department,
            IdRole = idRole,
            IsVerified = false,
            IsBanned = false,
            DeletedAt = null
        };

    public User CreateFromPersistence(
        int idUser,
        string username,
        string email,
        string hashedPassword,
        string firstName,
        DateOnly? birthDate,
        string? bio,
        bool isVerified,
        bool isBanned,
        DateTime? deletedAt,
        Department department,
        int idRole
    ) =>
        new()
        {
            IdUser = idUser,
            Username = username,
            Email = email,
            HashedPassword = hashedPassword,
            FirstName = firstName,
            BirthDate = birthDate,
            Bio = bio,
            IsVerified = isVerified,
            IsBanned = isBanned,
            DeletedAt = deletedAt,
            Department = department,
            IdRole = idRole
        };
}
