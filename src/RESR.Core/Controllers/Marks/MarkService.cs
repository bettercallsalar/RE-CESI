using RESR.Core.Controllers.Marks.Ports;
using RESR.Core.Errors;
using RESR.Models.Marks;

namespace RESR.Core.Controllers.Marks;

public sealed class MarkService : IMarkService
{
    private readonly IMarksRepository _repo;

    public MarkService(IMarksRepository repo) => _repo = repo;

    public async Task<Mark> MarkAsFavoriteAsync(int idResource, int idUser, CancellationToken ct)
    {
        ValidateIds(idResource, idUser);
        await EnsureResourceExistsAsync(idResource, ct);
        return await _repo.MarkAsFavoriteAsync(idResource, idUser, ct);
    }

    public async Task UnmarkAsFavoriteAsync(int idResource, int idUser, CancellationToken ct)
    {
        ValidateIds(idResource, idUser);
        var removed = await _repo.UnmarkAsFavoriteAsync(idResource, idUser, ct);
        if (!removed)
            throw new NotFoundException($"Favorite mark for resource {idResource} not found");
    }

    public async Task<Mark> MarkAsReadLaterAsync(int idResource, int idUser, CancellationToken ct)
    {
        ValidateIds(idResource, idUser);
        await EnsureResourceExistsAsync(idResource, ct);
        return await _repo.MarkAsReadLaterAsync(idResource, idUser, ct);
    }

    public async Task UnmarkAsReadLaterAsync(int idResource, int idUser, CancellationToken ct)
    {
        ValidateIds(idResource, idUser);
        var removed = await _repo.UnmarkAsReadLaterAsync(idResource, idUser, ct);
        if (!removed)
            throw new NotFoundException($"Read later mark for resource {idResource} not found");
    }

    public async Task<IReadOnlyList<Mark>> GetFavoriteRessourcesAsync(int idUser, CancellationToken ct)
    {
        if (idUser <= 0)
            throw new ValidationException("IdUser must be greater than 0");

        return await _repo.GetFavoriteRessourcesAsync(idUser, ct);
    }

    public async Task<IReadOnlyList<Mark>> GetReadLaterRessourcesAsync(int idUser, CancellationToken ct)
    {
        if (idUser <= 0)
            throw new ValidationException("IdUser must be greater than 0");

        return await _repo.GetReadLaterRessourcesAsync(idUser, ct);
    }

    public async Task<Mark?> GetFavoriteRessourceAsync(int idResource, int idUser, CancellationToken ct)
    {
        ValidateIds(idResource, idUser);
        return await _repo.GetFavoriteRessourceAsync(idResource, idUser, ct);
    }

    public async Task<Mark?> GetReadLaterRessourceAsync(int idResource, int idUser, CancellationToken ct)
    {
        ValidateIds(idResource, idUser);
        return await _repo.GetReadLaterRessourceAsync(idResource, idUser, ct);
    }

    private async Task EnsureResourceExistsAsync(int idResource, CancellationToken ct)
    {
        if (!await _repo.ResourceExistsAsync(idResource, ct))
            throw new NotFoundException($"Resource {idResource} not found");
    }

    private static void ValidateIds(int idResource, int idUser)
    {
        if (idResource <= 0)
            throw new ValidationException("IdResource must be greater than 0");

        if (idUser <= 0)
            throw new ValidationException("IdUser must be greater than 0");
    }
}
