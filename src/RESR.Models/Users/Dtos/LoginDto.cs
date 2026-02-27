namespace RESR.Models.Users;

public sealed record Login(
    string Email,
    string Password
);