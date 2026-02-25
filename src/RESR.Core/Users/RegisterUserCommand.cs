namespace RESR.Core.Users;

public sealed record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string? FirstName,
    DateOnly? BirthDate,
    string? Bio,
    int? IdDepartment,
    int? IdRole
);