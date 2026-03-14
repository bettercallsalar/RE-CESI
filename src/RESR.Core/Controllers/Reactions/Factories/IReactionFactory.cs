using RESR.Models.Reactions;

namespace RESR.Core.Controllers.Reactions.Factories;

public interface IReactionFactory
{
    Reaction CreateForCreation(
        string name,
        int idResource,
        int idUser
    );

    Reaction CreateFromPersistence(
        int idReaction,
        string name,
        int idResource,
        int idUser,
        string? username = null,
        string? firstName = null
    );
}
