namespace RESR.Core.Controllers.Users;

public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    DateOnly? BirthDate,
    string? Bio,
    int IdDepartment,
    int IdRole = 1 // Default to "User" role, can be changed later by an admin
);

public sealed record UpdateUserCommand(
    int IdUser,
    string? Username = null,
    string? Email = null,
    string? FirstName = null,
    DateOnly? BirthDate = null,
    string? Bio = null,
    bool ClearBio = false,
    int? IdDepartment = null,
    int? IdRole = null
);

public sealed record SetUserVerificationCommand(
    int IdUser,
    bool IsVerified
);
