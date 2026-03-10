namespace RESR.Models.Users;

public sealed class User
{
    public int IdUser { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Bio { get; set; }
    public required string Email { get; set; }
    public required string HashedPassword { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public required int IdDepartment { get; set; }
    public required int IdRole { get; set; }
}