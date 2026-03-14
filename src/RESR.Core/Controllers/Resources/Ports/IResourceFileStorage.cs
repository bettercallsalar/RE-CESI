using RESR.Models.Resources;

namespace RESR.Core.Controllers.Resources.Ports;

public interface IResourceFileStorage
{
    Task<IReadOnlyList<ResourceFile>> SaveAsync(
        int idResource,
        int idUser,
        IReadOnlyList<ResourceFileUpload> uploads,
        CancellationToken ct);

    Task DeleteAsync(IReadOnlyList<ResourceFile> files, CancellationToken ct);
}
