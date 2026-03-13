namespace RESR.Models.Reactions;

public sealed record CreateReactionRequest(
    string Name
);

public sealed record UpdateReactionRequest(
    string Name
);

public sealed record ReactionUserResponse(
    int IdUser,
    string Username,
    string FirstName
);

public sealed record ReactionResponse(
    int IdReaction,
    string Name,
    int IdResource,
    int IdUser,
    ReactionUserResponse User
);

public sealed record UserReactionsResponse(
    int IdUser,
    int TotalCount,
    IReadOnlyList<ReactionResponse> Items
);
