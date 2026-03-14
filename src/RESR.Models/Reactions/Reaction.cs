namespace RESR.Models.Reactions;

public sealed class Reaction
{
    public int IdReaction { get; set; }
    public string Name { get; set; } = string.Empty;
    public int IdResource { get; set; }
    public int IdUser { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
}
