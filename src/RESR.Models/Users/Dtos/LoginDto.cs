namespace RESR.Models.Users;

public sealed record LoginDto(
    string Email,
    string Password
);