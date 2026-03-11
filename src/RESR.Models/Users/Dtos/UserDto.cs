namespace RESR.Models.Users;

public sealed record RegisterUserRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    DateOnly? BirthDate,
    string? Bio,
    int IdDepartment
);

public sealed record UpdateUserRequest(
    string? Username,
    string? Email,
    string? FirstName,
    DateOnly? BirthDate,
    string? Bio,
    int? IdDepartment,
    int? IdRole
);

public sealed record UpdateOwnProfileRequest(
    string? Username,
    string? Email,
    string? FirstName,
    DateOnly? BirthDate,
    string? Bio,
    int? IdDepartment
);

public sealed record SetUserVerificationRequest(
    bool IsVerified
);

public sealed record UserResponse(
    int IdUser,
    string Username,
    string Email,
    string FirstName,
    DateOnly? BirthDate,
    string? Bio,
    bool IsVerified,
    int IdDepartment,
    int IdRole
);

public sealed record PaginatedUsersResponse(
    IReadOnlyList<UserResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public sealed record UserListingFilters(
    string? Keyword,
    IReadOnlyList<int>? DepartmentIds,
    IReadOnlyList<int>? RoleIds,
    DateOnly? BirthDate,
    bool? IsVerified,
    bool IncludeDeleted
);
