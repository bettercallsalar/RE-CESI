using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Profile;
using RESR.MAUI.Services;

namespace RESR.MAUI.Pages.Marks;

public partial class MarkResourcesPage : ContentPage, IQueryAttributable
{
    private readonly IMarkedResourcesService _markedResourcesService;
    private readonly IApiSession _session;

    private CancellationTokenSource? _loadCts;
    private IReadOnlyList<MarkedResourceItem> _allItems = Array.Empty<MarkedResourceItem>();
    private IReadOnlyList<MarkedResourceItem> _filteredItems = Array.Empty<MarkedResourceItem>();
    private MarkResourcesMode _mode = MarkResourcesMode.Favorite;

    public MarkResourcesPage(IMarkedResourcesService markedResourcesService, IApiSession session)
    {
        _markedResourcesService = markedResourcesService;
        _session = session;

        InitializeComponent();
        ApplyMode();
        ApplyState();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mode", out var rawMode)
            && Enum.TryParse<MarkResourcesMode>(rawMode?.ToString(), ignoreCase: true, out var parsedMode))
        {
            _mode = parsedMode;
        }
        else
        {
            _mode = MarkResourcesMode.Favorite;
        }

        ApplyMode();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_session.IsAuthenticated)
        {
            StatusLabel.Text = "Connecte-toi pour consulter tes marques.";
            await Shell.Current.GoToAsync(nameof(LoginPage));
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
        ApplyFilter();
        await Task.CompletedTask;
    }

    private async void OnSearchSubmitted(object? sender, EventArgs e)
    {
        ApplyFilter();
        await Task.CompletedTask;
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyFilter();
        await Task.CompletedTask;
    }

    private async void OnRefreshRequested(object? sender, EventArgs e)
    {
        await ReloadAsync(triggeredByRefresh: true);
    }

    private async void OnMarkedResourceTapped(object? sender, TappedEventArgs e)
    {
        await OpenMarkedResourceAsync(sender);
    }

    private async void OnMarkedResourceOpenClicked(object? sender, EventArgs e)
    {
        await OpenMarkedResourceAsync(sender);
    }

    private async void OnBackToProfileClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }

    private async Task ReloadAsync(bool triggeredByRefresh)
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true, triggeredByRefresh);
        StatusLabel.Text = BuildLoadingMessage();

        try
        {
            _allItems = _mode == MarkResourcesMode.Favorite
                ? await _markedResourcesService.GetFavoritesAsync(_loadCts.Token)
                : await _markedResourcesService.GetReadLaterAsync(_loadCts.Token);

            ApplyFilter();
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
            _allItems = Array.Empty<MarkedResourceItem>();
            _filteredItems = Array.Empty<MarkedResourceItem>();
            ApplyState();
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
            StatusLabel.Text = $"Erreur inattendue : {TrimMessage(ex.Message)}";
        }
        finally
        {
            _loadCts?.Dispose();
            _loadCts = null;
            SetLoadingState(false, triggeredByRefresh);
        }
    }

    private void ApplyFilter()
    {
        var keyword = KeywordSearchBar.Text?.Trim();
        _filteredItems = string.IsNullOrWhiteSpace(keyword)
            ? _allItems
            : _allItems
                .Where(item => item.SearchableText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();

        ApplyState();
        StatusLabel.Text = BuildStatusMessage(keyword);
    }

    private void ApplyMode()
    {
        var title = _mode == MarkResourcesMode.Favorite
            ? "Mes favoris"
            : "Ma liste lire plus tard";

        var subtitle = _mode == MarkResourcesMode.Favorite
            ? "Recherche parmi les ressources que tu as mises en favori."
            : "Recherche parmi les ressources que tu veux relire plus tard.";

        var emptyState = _mode == MarkResourcesMode.Favorite
            ? "Aucune ressource en favori."
            : "Aucune ressource a lire plus tard.";

        Title = title;
        TitleLabel.Text = title;
        SubtitleLabel.Text = subtitle;
        EmptyStateLabel.Text = emptyState;
    }

    private void ApplyState()
    {
        MarksCollectionView.ItemsSource = _filteredItems;
        MarksCollectionView.IsVisible = _filteredItems.Count > 0;
        EmptyState.IsVisible = _filteredItems.Count == 0;
    }

    private void SetLoadingState(bool isLoading, bool triggeredByRefresh)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        SearchButton.IsEnabled = !isLoading;

        if (triggeredByRefresh || !isLoading)
            RefreshContainer.IsRefreshing = isLoading && triggeredByRefresh;
    }

    private async Task OpenMarkedResourceAsync(object? sender)
    {
        var item = sender is BindableObject bindable
            ? bindable.BindingContext as MarkedResourceItem
            : null;

        if (item is null || string.IsNullOrWhiteSpace(item.Route) || Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync(item.Route);
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Navigation impossible : {TrimMessage(ex.Message)}";
        }
    }

    private string BuildLoadingMessage()
    {
        return _mode == MarkResourcesMode.Favorite
            ? "Chargement des favoris..."
            : "Chargement de la liste lire plus tard...";
    }

    private string BuildStatusMessage(string? keyword)
    {
        if (_filteredItems.Count == 0)
        {
            return string.IsNullOrWhiteSpace(keyword)
                ? (_mode == MarkResourcesMode.Favorite
                    ? "Aucune ressource en favori."
                    : "Aucune ressource a lire plus tard.")
                : $"Aucun resultat pour \"{keyword}\".";
        }

        var label = _mode == MarkResourcesMode.Favorite ? "favori(s)" : "ressource(s) a lire plus tard";
        return $"{_filteredItems.Count} {label} affiche(s).";
    }

    private static string TrimMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Erreur inconnue.";

        var normalized = message.Replace("\r", " ").Replace("\n", " ").Trim();
        return normalized.Length <= 180
            ? normalized
            : normalized[..177] + "...";
    }
}

public enum MarkResourcesMode
{
    Favorite,
    ReadLater
}
