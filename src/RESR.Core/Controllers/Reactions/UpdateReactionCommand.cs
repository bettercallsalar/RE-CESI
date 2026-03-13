namespace RESR.Core.Controllers.Reactions;

public sealed record UpdateReactionCommand(
    int IdReaction,
    string Name,
    int ActorUserId
);
