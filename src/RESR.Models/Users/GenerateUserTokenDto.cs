namespace RESR.Models.Users;

public sealed class GenerateUserTokenDto
{
    public required long Id { get; set; }
    public required bool IsAdmin { get; set; }
}
