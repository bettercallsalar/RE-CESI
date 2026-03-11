using RESR.Core.Controllers.Articles.Ports;
using RESR.Core.Errors;
using RESR.Models.Resources;

namespace RESR.Core.Controllers.Articles;

public sealed class ArticleService : IArticleService
{
    private readonly IArticleRepository _repo;

    public ArticleService(IArticleRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Article>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);

    public Task<Article?> GetByResourceIdAsync(int idResource, CancellationToken ct) => _repo.GetByResourceIdAsync(idResource, ct);

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

        return await _repo.CreateAsync(normalized, ct);
    }

    public async Task<Article> UpdateAsync(UpdateArticleCommand cmd, CancellationToken ct)
    {
        if (cmd.IdResource <= 0)
            throw new ValidationException("IdResource must be greater than 0.");

        var existing = await _repo.GetByResourceIdAsync(cmd.IdResource, ct);
        if (existing is null)
            throw new NotFoundException($"Article resource {cmd.IdResource} not found.");

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

        return await _repo.PatchAsync(normalized, ct)
            ?? throw new NotFoundException($"Article resource {cmd.IdResource} not found.");
    }

    public async Task<Article> SetApprovalAsync(SetArticleApprovalCommand cmd, CancellationToken ct)
    {
        if (cmd.IdResource <= 0)
            throw new ValidationException("IdResource must be greater than 0.");

        return await _repo.SetApprovalAsync(cmd, ct)
            ?? throw new NotFoundException($"Article resource {cmd.IdResource} not found.");
    }

    public Task<bool> SoftDeleteAsync(int idResource, CancellationToken ct) => _repo.SoftDeleteAsync(idResource, ct);

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }
}
