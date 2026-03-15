using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Events;

public partial class EventsPage : ContentPage
{
    private static readonly Color MutedStatusColor = Color.FromArgb("#5F5F66");
    private static readonly Color ErrorStatusColor = Color.FromArgb("#AB231E");

    private const int PageSize = 10;

    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;
    private bool _hasLoadedOnce;
    private int _currentPage;
    private int _totalPages;
    private int _totalCount;
    private string? _currentKeyword;
    private IReadOnlyList<EventListItem> _items = Array.Empty<EventListItem>();

    public EventsPage(IResourcesApiClient resourcesApiClient, IApiSession session)
    {
        _resourcesApiClient = resourcesApiClient;
        _session = session;
        InitializeComponent();
        StatusLabel.TextColor = MutedStatusColor;
        ApplyState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        CreateEventButton.IsVisible = _session.IsAuthenticated;

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

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await NavigateBackAsync();
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

    private async void OnCreateEventClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync(nameof(CreateEventPage));
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Navigation impossible : {TrimMessage(ex.Message)}";
        }
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
            var response = await _resourcesApiClient.GetEventsAsync(page, PageSize, _currentKeyword, _loadCts.Token);
            var mappedItems = response.Items.Select(ToListItem).ToList();

            _items = append
                ? _items.Concat(mappedItems).ToList()
                : mappedItems;

            _currentPage = response.Page;
            _totalPages = response.TotalPages;
            _totalCount = response.TotalCount;

            ApplyState();
            StatusLabel.TextColor = MutedStatusColor;
            StatusLabel.Text = BuildStatusMessage();
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
            if (!append)
            {
                _items = Array.Empty<EventListItem>();
                _currentPage = 0;
                _totalPages = 0;
                _totalCount = 0;
                ApplyState();
            }
        }
        catch (TimeoutException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            StatusLabel.TextColor = MutedStatusColor;
            StatusLabel.Text = "Chargement annule.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Erreur inattendue : {TrimMessage(ex.Message)}";
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
        EventsCollectionView.ItemsSource = _items;
        EventsCollectionView.IsVisible = _items.Count > 0;
        EmptyState.IsVisible = _items.Count == 0;
        LoadMoreButton.IsVisible = _items.Count > 0 && _currentPage < _totalPages;
    }

    private void SetLoadingState(bool isLoading, bool triggeredByRefresh)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        SearchButton.IsEnabled = !isLoading;
        CreateEventButton.IsEnabled = !isLoading;
        LoadMoreButton.IsEnabled = !isLoading;

        if (triggeredByRefresh || !isLoading)
            RefreshContainer.IsRefreshing = isLoading && triggeredByRefresh;
    }

    private string BuildStatusMessage()
    {
        if (_totalCount == 0)
        {
            return string.IsNullOrWhiteSpace(_currentKeyword)
                ? "Aucun evenement public disponible."
                : $"Aucun evenement pour \"{_currentKeyword}\".";
        }

        return $"{_totalCount} evenement(s) trouves. Page {_currentPage}/{Math.Max(1, _totalPages)}.";
    }

    private static EventListItem ToListItem(EventResponse @event)
    {
        var author = GetAuthorLabel(@event.Author);
        var location = DisplayText.FirstNonEmpty(@event.Address, @event.Department?.Name, "Lieu a confirmer");
        var summary = DisplayText.FirstNonEmpty(@event.Description, @event.Subtitle, "Aucune description disponible.");

        return new EventListItem(
            Badge: @event.Visibility.Equals("PUBLIC", StringComparison.OrdinalIgnoreCase) ? "Evenement public" : "Evenement prive",
            DateLabel: @event.StartDate.ToString("dd/MM/yyyy"),
            Eyebrow: "EVENT",
            Title: DisplayText.Normalize(@event.Title),
            Subtitle: BuildEventSubtitle(@event, location),
            Meta: $"Par {author} | {location}",
            Summary: DisplayText.ToExcerpt(summary, 220),
            AccessibilityText: $"Evenement {DisplayText.Normalize(@event.Title)}, prevu {BuildEventSubtitle(@event, location)}. {DisplayText.ToExcerpt(summary, 160)}");
    }

    private static string BuildEventSubtitle(EventResponse @event, string location)
    {
        var dateLine = @event.EndDate.HasValue
            ? $"Du {@event.StartDate:dd/MM/yyyy} au {@event.EndDate:dd/MM/yyyy}"
            : $"Le {@event.StartDate:dd/MM/yyyy}";

        return $"{dateLine} | {location}";
    }

    private static string GetAuthorLabel(ResourceAuthorResponse author)
    {
        var firstName = DisplayText.Normalize(author.FirstName);
        var username = DisplayText.Normalize(author.Username);

        if (!string.IsNullOrWhiteSpace(firstName))
            return firstName;

        if (!string.IsNullOrWhiteSpace(username))
            return username;

        return "un utilisateur";
    }

    private static string? NormalizeKeyword(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string TrimMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Erreur inconnue.";

        return DisplayText.ToExcerpt(message, 180);
    }

    private async Task NavigateBackAsync()
    {
        if (Shell.Current is null)
            return;

        try
        {
            if (Shell.Current.Navigation.NavigationStack.Count > 1)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Retour impossible : {TrimMessage(ex.Message)}";
        }
    }

    private sealed record EventListItem(
        string Badge,
        string DateLabel,
        string Eyebrow,
        string Title,
        string Subtitle,
        string Meta,
        string Summary,
        string AccessibilityText)
    {
        public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);
    }
}
