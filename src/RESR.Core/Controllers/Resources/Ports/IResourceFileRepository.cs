using RESR.Models.Resources;

namespace RESR.Core.Controllers.Resources.Ports;

public interface IResourceFileRepository
{
    Task<IReadOnlyDictionary<int, IReadOnlyList<ResourceFile>>> GetByResourceIdsAsync(IReadOnlyCollection<int> resourceIds, CancellationToken ct);
    Task ReplaceForResourceAsync(int idResource, IReadOnlyList<ResourceFile> files, CancellationToken ct);
    Task DeleteForResourceAsync(int idResource, CancellationToken ct);
}
