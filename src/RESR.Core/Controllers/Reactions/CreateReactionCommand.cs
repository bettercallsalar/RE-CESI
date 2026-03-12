namespace RESR.Core.Controllers.Reactions;

public sealed record CreateReactionCommand(
    int IdResource,
    string Name,
    int IdUser
);
