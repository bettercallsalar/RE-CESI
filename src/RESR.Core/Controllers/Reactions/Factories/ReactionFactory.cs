using RESR.Models.Reactions;

namespace RESR.Core.Controllers.Reactions.Factories;

public sealed class ReactionFactory : IReactionFactory
{
    public Reaction CreateForCreation(string name, int idResource, int idUser) =>
        new()
        {
            Name = name,
            IdResource = idResource,
            IdUser = idUser
        };

    public Reaction CreateFromPersistence(
        int idReaction,
        string name,
        int idResource,
        int idUser,
        string? username = null,
        string? firstName = null) =>
        new()
        {
            IdReaction = idReaction,
            Name = name,
            IdResource = idResource,
            IdUser = idUser,
            Username = username ?? string.Empty,
            FirstName = firstName ?? string.Empty
        };
}
