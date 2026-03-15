using RESR.MAUI.Pages.Articles;
using RESR.Models.Marks;

namespace RESR.MAUI.Services;

public sealed class MarkedResourcesService : IMarkedResourcesService
{
    private const int PageSize = 50;

    private readonly IMarksApiClient _marksApiClient;
    private readonly IResourcesApiClient _resourcesApiClient;

    public MarkedResourcesService(IMarksApiClient marksApiClient, IResourcesApiClient resourcesApiClient)
    {
        _marksApiClient = marksApiClient;
        _resourcesApiClient = resourcesApiClient;
    }

    public async Task<IReadOnlyList<MarkedResourceItem>> GetFavoritesAsync(CancellationToken ct)
    {
        var marks = await LoadAllMarksAsync((page, pageSize, cancellationToken) =>
            _marksApiClient.GetFavoritesAsync(page, pageSize, cancellationToken), ct);

        return await ResolveMarkedResourcesAsync(marks, ct);
    }

    public async Task<IReadOnlyList<MarkedResourceItem>> GetReadLaterAsync(CancellationToken ct)
    {
        var marks = await LoadAllMarksAsync((page, pageSize, cancellationToken) =>
            _marksApiClient.GetReadLaterAsync(page, pageSize, cancellationToken), ct);

        return await ResolveMarkedResourcesAsync(marks, ct);
    }

    private static async Task<IReadOnlyList<MarkResponse>> LoadAllMarksAsync(
        Func<int, int, CancellationToken, Task<PaginatedMarksResponse>> fetchPageAsync,
        CancellationToken ct)
    {
        var items = new List<MarkResponse>();
        var page = 1;

        while (true)
        {
            var response = await fetchPageAsync(page, PageSize, ct);
            items.AddRange(response.Items);

            if (response.TotalPages <= 0 || page >= response.TotalPages)
                break;

            page++;
        }

        return items;
    }

    private async Task<IReadOnlyList<MarkedResourceItem>> ResolveMarkedResourcesAsync(
        IReadOnlyList<MarkResponse> marks,
        CancellationToken ct)
    {
        if (marks.Count == 0)
            return [];

        var items = await Task.WhenAll(marks.Select(mark => ResolveMarkedResourceAsync(mark, ct)));
        return items
            .Where(item => item is not null)
            .Select(item => item!)
            .ToList();
    }

    private async Task<MarkedResourceItem?> ResolveMarkedResourceAsync(MarkResponse mark, CancellationToken ct)
    {
        var article = await _resourcesApiClient.GetArticleByIdAsync(mark.IdRessource, ct);
        if (article is not null)
        {
            var summary = FirstNonEmpty(article.Description, article.Content, "Aucune description disponible.");

            return new MarkedResourceItem(
                mark.IdRessource,
                "Article",
                article.Title,
                $"Publie le {article.CreatedAt:dd/MM/yyyy}",
                ToExcerpt(summary, 180),
                $"{nameof(ArticleDetailPage)}?idResource={mark.IdRessource}");
        }

        var @event = await _resourcesApiClient.GetEventByIdAsync(mark.IdRessource, ct);
        if (@event is not null)
        {
            var summary = FirstNonEmpty(@event.Description, @event.Subtitle, "Aucune description disponible.");
            var location = FirstNonEmpty(@event.Address, @event.Department?.Name, "Lieu a confirmer");

            return new MarkedResourceItem(
                mark.IdRessource,
                "Evenement",
                @event.Title,
                @event.EndDate.HasValue
                    ? $"Du {@event.StartDate:dd/MM/yyyy} au {@event.EndDate:dd/MM/yyyy}"
                    : $"Le {@event.StartDate:dd/MM/yyyy}",
                ToExcerpt($"{summary}  |  {location}", 180),
                Route: null);
        }

        return new MarkedResourceItem(
            mark.IdRessource,
            "Ressource",
            "Ressource indisponible",
            "Ressource indisponible",
            "Cette ressource n'est plus accessible depuis l'application.",
            Route: null);
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return string.Empty;
    }

    private static string ToExcerpt(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Replace("\r", " ").Replace("\n", " ").Trim();
        if (normalized.Length <= maxLength)
            return normalized;

        return normalized[..Math.Max(0, maxLength - 3)].TrimEnd() + "...";
    }
}
