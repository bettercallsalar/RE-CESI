namespace RESR.Models.Reactions;

public static class ReactionNames
{
    public const string Like = "like";
    public const string Dislike = "dislike";
    public const string Love = "love";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Like,
        Dislike,
        Love
    };
}
