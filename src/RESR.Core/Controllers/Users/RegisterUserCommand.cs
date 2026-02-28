namespace RESR.Core.Controllers.Users;

public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    DateOnly? BirthDate,
    string? Bio,
    int IdDepartment,
    int IdRole
);

public sealed record UpdateUserCommand(
    int IdUser,
    string? Username,
    string? Email,
    string? FirstName,
    DateOnly? BirthDate,
    bool? IsVerified,
    string? Bio,
    int? IdDepartment,
    int? IdRole
);
