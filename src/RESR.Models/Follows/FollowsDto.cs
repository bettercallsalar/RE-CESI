namespace RESR.Models.Follows;

public sealed record FollowRequest(
    int IdFollower,
    int IdFollowing
);

public sealed record FollowResponse(
    int IdFollower,
    int IdFollowing
);

public sealed record FollowUserResponse(
    int IdUser,
    string Username,
    string FirstName
);

public sealed record PaginatedFollowUsersResponse(
    IReadOnlyList<FollowUserResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
