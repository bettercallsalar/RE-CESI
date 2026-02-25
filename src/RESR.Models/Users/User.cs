namespace RESR.Models.Users;

public sealed class User
{
    public int IdUser { get; set; }
    public required string Username { get; set; }
    public string? FirstName { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Bio { get; set; }
    public required string Email { get; set; }
    public required string HashedPassword { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? IdDepartment { get; set; }
    public int? IdRole { get; set; }
}