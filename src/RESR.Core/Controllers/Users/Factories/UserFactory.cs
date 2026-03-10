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
        int idDepartment,
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
            IdDepartment = idDepartment,
            IdRole = idRole,
            IsVerified = false,
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
        DateTime? deletedAt,
        int idDepartment,
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
            DeletedAt = deletedAt,
            IdDepartment = idDepartment,
            IdRole = idRole
        };
}
