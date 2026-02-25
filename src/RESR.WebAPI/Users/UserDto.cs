namespace RESR.WebAPI.Users;

public sealed record RegisterUserRequest(
    string Username,
    string Email,
    string Password,
    string? FirstName,
    DateOnly? BirthDate,
    string? Bio,
    int? IdDepartment,
    int? IdRole
);

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
