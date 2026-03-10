using RESR.Models.Users;

namespace RESR.Core.Controllers.Users.Factories;

public interface IUserFactory
{
    User CreateForRegistration(
        string username,
        string email,
        string hashedPassword,
        string firstName,
        DateOnly? birthDate,
        string? bio,
        int idDepartment,
        int idRole
    );

    User CreateFromPersistence(
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
    );
}
