using RESR.Core.Controllers.Articles.Ports;
using RESR.Core.Controllers.Resources.Ports;
using RESR.Core.Errors;
using RESR.Models.Resources;

namespace RESR.Core.Controllers.Articles;

public sealed class ArticleService : IArticleService
{
    private sealed class NullResourceFileRepository : IResourceFileRepository
    {
        public Task<IReadOnlyDictionary<int, IReadOnlyList<ResourceFile>>> GetByResourceIdsAsync(IReadOnlyCollection<int> resourceIds, CancellationToken ct) =>
            Task.FromResult<IReadOnlyDictionary<int, IReadOnlyList<ResourceFile>>>(new Dictionary<int, IReadOnlyList<ResourceFile>>());

        public Task ReplaceForResourceAsync(int idResource, IReadOnlyList<ResourceFile> files, CancellationToken ct) => Task.CompletedTask;

        public Task DeleteForResourceAsync(int idResource, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class NullResourceFileStorage : IResourceFileStorage
    {
        public Task<IReadOnlyList<ResourceFile>> SaveAsync(int idResource, int idUser, IReadOnlyList<Core.Controllers.Resources.ResourceFileUpload> uploads, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ResourceFile>>(Array.Empty<ResourceFile>());

        public Task DeleteAsync(IReadOnlyList<ResourceFile> files, CancellationToken ct) => Task.CompletedTask;
    }

    private readonly IArticleRepository _repo;
    private readonly IResourceFileRepository _fileRepository;
    private readonly IResourceFileStorage _fileStorage;

    public ArticleService(IArticleRepository repo)
        : this(repo, new NullResourceFileRepository(), new NullResourceFileStorage())
    {
    }

    public ArticleService(IArticleRepository repo, IResourceFileRepository fileRepository, IResourceFileStorage fileStorage)
    {
        _repo = repo;
        _fileRepository = fileRepository;
        _fileStorage = fileStorage;
    }

    public async Task<(IReadOnlyList<Article> Articles, int TotalCount)> GetPaginatedAsync(
        int page,
        int pageSize,
        ArticleListingFilters filters,
        CancellationToken ct)
    {
        var normalizedFilters = NormalizeListingFilters(filters);
        var articles = await _repo.GetPaginatedAsync(page, pageSize, normalizedFilters, ct);
        await AttachFilesAsync(articles, ct);
        var totalCount = await _repo.CountAsync(normalizedFilters, ct);
        return (articles, totalCount);
    }

    public async Task<Article?> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        var article = await _repo.GetByResourceIdAsync(idResource, ct);

        if (article is null)
            return null;

        await AttachFilesAsync(new[] { article }, ct);
        return article;
    }

    public async Task<int> CreateAsync(CreateArticleCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.Title))
            throw new ValidationException("Title is required.");
        if (string.IsNullOrWhiteSpace(cmd.Content))
            throw new ValidationException("Content is required.");
        if (cmd.IdUser <= 0)
            throw new ValidationException("IdUser must be greater than 0.");
        if (cmd.IdCategory <= 0)
            throw new ValidationException("IdCategory must be greater than 0.");

        var normalized = cmd with
        {
            Title = cmd.Title.Trim(),
            Description = NormalizeOptional(cmd.Description),
            Content = cmd.Content.Trim()
        };

        ValidateFiles(normalized.Files);

        var idResource = await _repo.CreateAsync(normalized, ct);

        if (normalized.Files is { Count: > 0 })
        {
            var storedFiles = await _fileStorage.SaveAsync(idResource, normalized.IdUser, normalized.Files, ct);
            await _fileRepository.ReplaceForResourceAsync(idResource, storedFiles, ct);
        }

        return idResource;
    }

    public async Task<Article> UpdateAsync(UpdateArticleCommand cmd, CancellationToken ct)
    {
        if (cmd.IdResource <= 0)
            throw new ValidationException("IdResource must be greater than 0.");

        var existing = await _repo.GetByResourceIdAsync(cmd.IdResource, ct);
        if (existing is null)
            throw new NotFoundException($"Article resource {cmd.IdResource} not found.");

        if (existing.IdUser != cmd.IdUser)
            throw new ForbiddenException("You do not have permission to update this article.");

        if (cmd.IdCategory is <= 0)
            throw new ValidationException("IdCategory must be greater than 0.");

        var normalizedContent = NormalizeOptional(cmd.Content);
        if (cmd.Content is not null && normalizedContent is null)
            throw new ValidationException("Content cannot be empty.");

        var normalized = cmd with
        {
            Title = NormalizeOptional(cmd.Title),
            Description = NormalizeOptional(cmd.Description),
            Content = normalizedContent
        };

        ValidateFiles(normalized.Files);

        var updatedArticle = await _repo.PatchAsync(normalized, ct)
            ?? throw new NotFoundException($"Article resource {cmd.IdResource} not found.");

        if (normalized.ReplaceFiles)
        {
            var existingFiles = await _fileRepository.GetByResourceIdsAsync(new[] { cmd.IdResource }, ct);

            if (existingFiles.TryGetValue(cmd.IdResource, out var filesToDelete) && filesToDelete.Count > 0)
                await _fileStorage.DeleteAsync(filesToDelete, ct);

            var storedFiles = normalized.Files is { Count: > 0 }
                ? await _fileStorage.SaveAsync(cmd.IdResource, cmd.IdUser, normalized.Files, ct)
                : Array.Empty<ResourceFile>();

            await _fileRepository.ReplaceForResourceAsync(cmd.IdResource, storedFiles, ct);
            return await _repo.GetByResourceIdAsync(cmd.IdResource, ct)
                ?? throw new NotFoundException($"Article resource {cmd.IdResource} not found.");
        }

        return updatedArticle;
    }

    public async Task<Article> SetApprovalAsync(SetArticleApprovalCommand cmd, CancellationToken ct)
    {
        if (cmd.IdResource <= 0)
            throw new ValidationException("IdResource must be greater than 0.");

        return await _repo.SetApprovalAsync(cmd, ct)
            ?? throw new NotFoundException($"Article resource {cmd.IdResource} not found.");
    }

    public async Task<bool> SoftDeleteAsync(int idResource, int idUser, CancellationToken ct)
    {
        var existingResource = await _repo.GetByResourceIdAsync(idResource, ct)
            ?? throw new NotFoundException($"Article resource {idResource} not found.");

        if (existingResource.IdUser != idUser)
            throw new ForbiddenException("You do not have permission to delete this article.");

        var deleted = await _repo.SoftDeleteAsync(idResource, ct);

        if (deleted)
        {
            var existingFiles = await _fileRepository.GetByResourceIdsAsync(new[] { idResource }, ct);

            if (existingFiles.TryGetValue(idResource, out var files) && files.Count > 0)
            {
                await _fileStorage.DeleteAsync(files, ct);
                await _fileRepository.DeleteForResourceAsync(idResource, ct);
            }
        }

        return deleted;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    private static ArticleListingFilters NormalizeListingFilters(ArticleListingFilters filters)
    {
        return filters with
        {
            Keyword = NormalizeOptional(filters.Keyword)
        };
    }

    private static void ValidateFiles(IReadOnlyList<Core.Controllers.Resources.ResourceFileUpload>? files)
    {
        if (files is null || files.Count == 0)
            return;

        if (files.Count > 6)
            throw new ValidationException("Vous ne pouvez pas envoyer plus de 6 images.");

        foreach (var file in files)
        {
            if (file.Size <= 0)
                throw new ValidationException("Une image envoyee est vide.");

            if (file.Size > 5 * 1024 * 1024)
                throw new ValidationException("Chaque image doit faire moins de 5 Mo.");

            if (!file.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Seules les images sont autorisees.");
        }
    }

    private async Task AttachFilesAsync(IReadOnlyList<Article> articles, CancellationToken ct)
    {
        if (articles.Count == 0)
            return;

        var filesByResource = await _fileRepository.GetByResourceIdsAsync(articles.Select(article => article.IdResource).ToArray(), ct);

        foreach (var article in articles)
            article.Files = filesByResource.TryGetValue(article.IdResource, out var files) ? files : Array.Empty<ResourceFile>();
    }
}
