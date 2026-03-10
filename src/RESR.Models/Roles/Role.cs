namespace RESR.Models.Roles;

public sealed class Role
{
    public int IdRole { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
