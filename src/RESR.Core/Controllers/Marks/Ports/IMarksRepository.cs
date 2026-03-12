using RESR.Models.Marks;

namespace RESR.Core.Controllers.Marks.Ports;

public interface IMarksRepository
{
    Task<bool> ResourceExistsAsync(int idResource, CancellationToken ct);
    Task<Mark> MarkAsFavoriteAsync(int idResource, int idUser, CancellationToken ct);
    Task<bool> UnmarkAsFavoriteAsync(int idResource, int idUser, CancellationToken ct);
    Task<Mark> MarkAsReadLaterAsync(int idResource, int idUser, CancellationToken ct);
    Task<bool> UnmarkAsReadLaterAsync(int idResource, int idUser, CancellationToken ct);
    Task<IReadOnlyList<Mark>> GetFavoriteRessourcesAsync(int idUser, CancellationToken ct);
    Task<IReadOnlyList<Mark>> GetReadLaterRessourcesAsync(int idUser, CancellationToken ct);
    Task<Mark?> GetFavoriteRessourceAsync(int idResource, int idUser, CancellationToken ct);
    Task<Mark?> GetReadLaterRessourceAsync(int idResource, int idUser, CancellationToken ct);
}
