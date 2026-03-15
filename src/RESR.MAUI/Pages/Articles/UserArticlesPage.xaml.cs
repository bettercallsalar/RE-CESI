using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Articles;

public partial class UserArticlesPage : ContentPage, IQueryAttributable
{
    private const int PageSize = 10;

    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;
    private int? _idUser;
    private bool _isOwnProfile;
    private bool _shouldLoad;
    private int _currentPage;
    private int _totalPages;
    private int _totalCount;
    private string? _currentKeyword;
    private string _username = string.Empty;
    private string _firstName = string.Empty;
    private IReadOnlyList<ArticleListItem> _items = Array.Empty<ArticleListItem>();

    public UserArticlesPage(IResourcesApiClient resourcesApiClient, IApiSession session)
    {
        _resourcesApiClient = resourcesApiClient;
        _session = session;
        InitializeComponent();
        ApplyState();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("idUser", out var rawId) &&
            int.TryParse(rawId?.ToString(), out var idUser) &&
            idUser > 0)
        {
            _idUser = idUser;
            _shouldLoad = true;
        }

        if (query.TryGetValue("isOwnProfile", out var rawOwnProfile) &&
            bool.TryParse(rawOwnProfile?.ToString(), out var isOwnProfile))
        {
            _isOwnProfile = isOwnProfile;
        }
        else
        {
            _isOwnProfile = false;
        }

        _username = Uri.UnescapeDataString(query.TryGetValue("username", out var rawUsername)
            ? rawUsername?.ToString() ?? string.Empty
            : string.Empty);

        _firstName = Uri.UnescapeDataString(query.TryGetValue("firstName", out var rawFirstName)
            ? rawFirstName?.ToString() ?? string.Empty
            : string.Empty);

        UpdateHeader();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_shouldLoad || !_idUser.HasValue)
            return;

        _shouldLoad = false;

        if (_isOwnProfile && !_session.IsAuthenticated)
        {
            StatusLabel.Text = "Connectez-vous pour consulter vos articles.";
            return;
        }

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

    private async void OnArticleTapped(object? sender, TappedEventArgs e)
    {
        if (!TryGetBoundItem<ArticleListItem>(sender, out var item))
            return;

        await NavigateToArticleDetailAsync(item.IdResource, _isOwnProfile);
    }

    private async void OnArticleOpenClicked(object? sender, EventArgs e)
    {
        if (!TryGetBoundItem<ArticleListItem>(sender, out var item))
            return;

        await NavigateToArticleDetailAsync(item.IdResource, _isOwnProfile);
    }

    private async Task ReloadAsync(bool triggeredByRefresh)
    {
        _currentKeyword = NormalizeKeyword(KeywordSearchBar.Text);
        await LoadPageAsync(page: 1, append: false, triggeredByRefresh: triggeredByRefresh);
    }

    private async Task LoadPageAsync(int page, bool append, bool triggeredByRefresh)
    {
        if (_loadCts is not null || !_idUser.HasValue)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true, triggeredByRefresh);

        try
        {
            PaginatedArticlesResponse response = _isOwnProfile
                ? await _resourcesApiClient.GetMyArticlesAsync(_idUser.Value, page, PageSize, _currentKeyword, _loadCts.Token)
                : await _resourcesApiClient.GetArticlesByUserAsync(_idUser.Value, page, PageSize, _currentKeyword, _loadCts.Token);

            var mappedItems = response.Items.Select(article => ToListItem(article, _isOwnProfile)).ToList();

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
            _loadCts?.Dispose();
            _loadCts = null;
            SetLoadingState(false, triggeredByRefresh);
        }
    }

    private void UpdateHeader()
    {
        if (_isOwnProfile)
        {
            PageTitleLabel.Text = "Mes articles";
            PageCaptionLabel.Text = "Retrouve ici les articles que tu as crees.";
            return;
        }

        var displayName = string.IsNullOrWhiteSpace(_firstName)
            ? (string.IsNullOrWhiteSpace(_username) ? "cet utilisateur" : _username)
            : _firstName;

        PageTitleLabel.Text = $"Articles de {displayName}";
        PageCaptionLabel.Text = "Parcourez les articles publics de ce profil.";
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
                ? "Aucun article disponible."
                : $"Aucun article pour \"{_currentKeyword}\".";
        }

        return $"{_totalCount} article(s) trouves. Page {_currentPage}/{Math.Max(1, _totalPages)}.";
    }

    private async Task NavigateToArticleDetailAsync(int idResource, bool useOwnAccess)
    {
        if (Shell.Current is null)
            return;

        try
        {
            var route = $"{nameof(ArticleDetailPage)}?idResource={idResource}&useOwnAccess={useOwnAccess.ToString().ToLowerInvariant()}";
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Navigation impossible : {TrimMessage(ex.Message)}";
        }
    }

    private static ArticleListItem ToListItem(ArticleResponse article, bool includeOwnerMeta)
    {
        var description = FirstNonEmpty(article.Description, article.Content, "Aucune description disponible.");
        var metaParts = new List<string>
        {
            $"Auteur #{article.IdUser}",
            $"Visibilite {article.Visibility.ToLowerInvariant()}"
        };

        if (includeOwnerMeta && !article.IsApproved)
            metaParts.Add("non approuve");

        return new ArticleListItem(
            article.IdResource,
            article.Title,
            $"Publie le {article.CreatedAt:dd/MM/yyyy}",
            string.Join("  |  ", metaParts),
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

    private static bool TryGetBoundItem<TItem>(object? sender, out TItem item) where TItem : class
    {
        item = ((sender as BindableObject)?.BindingContext as TItem)!;
        return item is not null;
    }

    private sealed record ArticleListItem(
        int IdResource,
        string Title,
        string Subtitle,
        string Meta,
        string Summary)
    {
        public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);
    }
}
