namespace RESR.Models.Users;

public sealed record UserResponse(
    int IdUser,
    string Username,
    string Email,
    string? FirstName,
    DateOnly? BirthDate,
    string? Bio,
    bool IsVerified,
    int? IdDepartment,
    int? IdRole
);
