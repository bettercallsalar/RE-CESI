using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI;

public partial class ArticlesPage : ContentPage
{
    private const int PageSize = 10;

    private readonly IResourcesApiClient _resourcesApiClient;
    private CancellationTokenSource? _loadCts;
    private bool _hasLoadedOnce;
    private int _currentPage;
    private int _totalPages;
    private int _totalCount;
    private string? _currentKeyword;
    private IReadOnlyList<ArticleListItem> _items = Array.Empty<ArticleListItem>();

    public ArticlesPage(IResourcesApiClient resourcesApiClient)
    {
        _resourcesApiClient = resourcesApiClient;
        InitializeComponent();
        ApplyState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasLoadedOnce)
            return;

        _hasLoadedOnce = true;
        await ReloadAsync(triggeredByRefresh: false);
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
    }

    private async void OnSearchButtonClicked(object? sender, EventArgs e)
    {
        await ReloadAsync(triggeredByRefresh: false);
    }

    private async void OnSearchSubmitted(object? sender, EventArgs e)
    {
        await ReloadAsync(triggeredByRefresh: false);
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.NewTextValue) || string.IsNullOrWhiteSpace(_currentKeyword))
            return;

        await ReloadAsync(triggeredByRefresh: false);
    }

    private async void OnRefreshRequested(object? sender, EventArgs e)
    {
        await ReloadAsync(triggeredByRefresh: true);
    }

    private async void OnLoadMoreClicked(object? sender, EventArgs e)
    {
        if (_loadCts is not null || _currentPage >= _totalPages)
            return;

        await LoadPageAsync(_currentPage + 1, append: true, triggeredByRefresh: false);
    }

    private async Task ReloadAsync(bool triggeredByRefresh)
    {
        _currentKeyword = NormalizeKeyword(KeywordSearchBar.Text);
        await LoadPageAsync(page: 1, append: false, triggeredByRefresh: triggeredByRefresh);
    }

    private async Task LoadPageAsync(int page, bool append, bool triggeredByRefresh)
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true, triggeredByRefresh);

        try
        {
            var response = await _resourcesApiClient.GetArticlesAsync(page, PageSize, _currentKeyword, _loadCts.Token);
            var mappedItems = response.Items.Select(ToListItem).ToList();

            _items = append
                ? _items.Concat(mappedItems).ToList()
                : mappedItems;

            _currentPage = response.Page;
            _totalPages = response.TotalPages;
            _totalCount = response.TotalCount;

            ApplyState();
            StatusLabel.Text = BuildStatusMessage();
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
            if (!append)
            {
                _items = Array.Empty<ArticleListItem>();
                _currentPage = 0;
                _totalPages = 0;
                _totalCount = 0;
                ApplyState();
            }
        }
        catch (TimeoutException ex)
        {
            StatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = "Chargement annule.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Erreur inattendue : {ex.Message}";
        }
        finally
        {
            _loadCts.Dispose();
            _loadCts = null;
            SetLoadingState(false, triggeredByRefresh);
        }
    }

    private void ApplyState()
    {
        ArticlesCollectionView.ItemsSource = _items;
        ArticlesCollectionView.IsVisible = _items.Count > 0;
        EmptyState.IsVisible = _items.Count == 0;
        LoadMoreButton.IsVisible = _items.Count > 0 && _currentPage < _totalPages;
    }

    private void SetLoadingState(bool isLoading, bool triggeredByRefresh)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        SearchButton.IsEnabled = !isLoading;
        LoadMoreButton.IsEnabled = !isLoading;

        if (triggeredByRefresh || !isLoading)
            RefreshContainer.IsRefreshing = isLoading && triggeredByRefresh;
    }

    private string BuildStatusMessage()
    {
        if (_totalCount == 0)
        {
            return string.IsNullOrWhiteSpace(_currentKeyword)
                ? "Aucun article public disponible."
                : $"Aucun article pour \"{_currentKeyword}\".";
        }

        return $"{_totalCount} article(s) trouves. Page {_currentPage}/{Math.Max(1, _totalPages)}.";
    }

    private static ArticleListItem ToListItem(ArticleResponse article)
    {
        var description = FirstNonEmpty(article.Description, article.Content, "Aucune description disponible.");

        return new ArticleListItem(
            article.Title,
            $"Publie le {article.CreatedAt:dd/MM/yyyy}",
            $"Auteur #{article.IdUser}  |  Visibilite {article.Visibility.ToLowerInvariant()}",
            ToExcerpt(description, 220));
    }

    private static string? NormalizeKeyword(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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

    private static string TrimMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Erreur inconnue.";

        return ToExcerpt(message, 180);
    }

    private sealed record ArticleListItem(
        string Title,
        string Subtitle,
        string Meta,
        string Summary)
    {
        public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);
    }
}
