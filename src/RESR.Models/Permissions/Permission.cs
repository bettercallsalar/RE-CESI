namespace RESR.Models.Permissions;

public sealed class Permission
{
    public int IdPermission { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}