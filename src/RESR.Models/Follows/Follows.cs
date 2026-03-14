namespace RESR.Models.Follows;

public sealed class Follow
{
    public int IdFollower {get; set;}
    public int IdFollowing {get; set;}
}

public sealed class FollowUser
{
    public int IdUser { get; set; }
    public required string Username { get; set; }
    public required string FirstName { get; set; }
}
