using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Articles;
using RESR.MAUI.Pages.Events;
using RESR.MAUI.Pages.Profile;
using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Home;

public partial class MainPage : ContentPage
{
    private const int CarouselItemLimit = 5;
    private static readonly Color MutedStatusColor = Color.FromArgb("#5F5F66");
    private static readonly Color ErrorStatusColor = Color.FromArgb("#AB231E");
    private static readonly Color SuccessStatusColor = Color.FromArgb("#1D6B43");

    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;
    private bool _hasLoadedOnce;
    private bool _isMobileMenuOpen;
    private IReadOnlyList<HomeResourceCard> _articleCards = Array.Empty<HomeResourceCard>();
    private IReadOnlyList<HomeResourceCard> _eventCards = Array.Empty<HomeResourceCard>();

    public MainPage(IResourcesApiClient resourcesApiClient, IApiSession session)
    {
        _resourcesApiClient = resourcesApiClient;
        _session = session;
        InitializeComponent();

        StatusLabel.TextColor = MutedStatusColor;
        UpdateAuthState();
        SetMobileMenuState(false);
        ApplyArticleState();
        ApplyEventState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateAuthState();

        if (_hasLoadedOnce)
            return;

        _hasLoadedOnce = true;
        await LoadResourcesAsync(triggeredByRefresh: false);
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
    }

    private async void OnRefreshButtonClicked(object? sender, EventArgs e)
    {
        await LoadResourcesAsync(triggeredByRefresh: false);
    }

    private async void OnRefreshRequested(object? sender, EventArgs e)
    {
        await LoadResourcesAsync(triggeredByRefresh: true);
    }

    private async void OnArticlesNavigationClicked(object? sender, EventArgs e)
    {
        await NavigateToAsync(nameof(ArticlesPage));
    }

    private async void OnEventsNavigationClicked(object? sender, EventArgs e)
    {
        await NavigateToAsync(nameof(EventsPage));
    }

    private async void OnAccountNavigationClicked(object? sender, EventArgs e)
    {
        await NavigateToAsync(_session.IsAuthenticated ? nameof(ProfilePage) : nameof(LoginPage));
    }

    private async void OnCreateArticleNavigationClicked(object? sender, EventArgs e)
    {
        await NavigateToAsync(nameof(CreateArticlePage));
    }

    private async void OnRegisterNavigationClicked(object? sender, EventArgs e)
    {
        await NavigateToAsync(nameof(RegisterPage));
    }

    private async void OnLogoutNavigationClicked(object? sender, EventArgs e)
    {
        _session.Clear();
        UpdateAuthState();
        SetMobileMenuState(false);
        StatusLabel.TextColor = SuccessStatusColor;
        StatusLabel.Text = "Deconnexion reussie.";
        await NavigateToRootAsync();
    }

    private async void OnArticleSeeMoreClicked(object? sender, EventArgs e)
    {
        await NavigateToAsync(nameof(ArticlesPage));
    }

    private async void OnEventSeeMoreClicked(object? sender, EventArgs e)
    {
        await NavigateToAsync(nameof(EventsPage));
    }

    private void OnMenuClicked(object? sender, EventArgs e)
    {
        SetMobileMenuState(!_isMobileMenuOpen);
    }

    private async Task LoadResourcesAsync(bool triggeredByRefresh)
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true, triggeredByRefresh);

        try
        {
            var articleTask = _resourcesApiClient.GetArticlesAsync(1, CarouselItemLimit, _loadCts.Token);
            var eventTask = _resourcesApiClient.GetEventsAsync(1, CarouselItemLimit, _loadCts.Token);

            await Task.WhenAll(articleTask, eventTask);

            var articles = await articleTask;
            var events = await eventTask;

            _articleCards = articles.Items
                .Take(CarouselItemLimit)
                .Select(ToArticleCard)
                .ToList();

            _eventCards = events.Items
                .Take(CarouselItemLimit)
                .Select(ToEventCard)
                .ToList();

            ApplyArticleState();
            ApplyEventState();

            StatusLabel.TextColor = MutedStatusColor;
            StatusLabel.Text = BuildStatusMessage(articles.TotalCount, events.TotalCount);
            UpdateAuthState();
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
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

    private void ApplyArticleState()
    {
        ArticleCarousel.ItemsSource = _articleCards;
        ArticleCarousel.IsVisible = _articleCards.Count > 0;
        ArticleEmptyState.IsVisible = _articleCards.Count == 0;
        ArticleIndicator.IsVisible = _articleCards.Count > 1;
    }

    private void ApplyEventState()
    {
        EventCarousel.ItemsSource = _eventCards;
        EventCarousel.IsVisible = _eventCards.Count > 0;
        EventEmptyState.IsVisible = _eventCards.Count == 0;
        EventIndicator.IsVisible = _eventCards.Count > 1;
    }

    private void SetLoadingState(bool isLoading, bool triggeredByRefresh)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        ReloadButton.IsEnabled = !isLoading;

        if (triggeredByRefresh || !isLoading)
            RefreshContainer.IsRefreshing = isLoading && triggeredByRefresh;
    }

    private void UpdateAuthState()
    {
        var isAuthenticated = _session.IsAuthenticated;
        var accountLabel = isAuthenticated ? "Mon compte" : "Connexion";

        HeaderAccountButton.Text = accountLabel;
        MobileAccountButton.Text = accountLabel;

        HeaderRegisterButton.IsVisible = !isAuthenticated;
        MobileRegisterButton.IsVisible = !isAuthenticated;

        HeaderCreateArticleButton.IsVisible = isAuthenticated;
        MobileCreateArticleButton.IsVisible = isAuthenticated;

        HeaderLogoutButton.IsVisible = isAuthenticated;
        MobileLogoutButton.IsVisible = isAuthenticated;
    }

    private void SetMobileMenuState(bool isOpen)
    {
        _isMobileMenuOpen = isOpen;
        MobileMenuPanel.IsVisible = isOpen;
        MenuButton.Text = isOpen ? "Fermer" : "Menu";
    }

    private static HomeResourceCard ToArticleCard(ArticleResponse article)
    {
        var author = GetAuthorLabel(article.Author);
        var subtitle = string.IsNullOrWhiteSpace(DisplayText.Normalize(article.Description))
            ? string.Empty
            : DisplayText.ToExcerpt(article.Description, 82);
        var summary = DisplayText.FirstNonEmpty(article.Content, article.Description, "Aucune description disponible.");

        return new HomeResourceCard(
            Badge: article.Visibility.Equals("PUBLIC", StringComparison.OrdinalIgnoreCase) ? "Article public" : "Article prive",
            DateLabel: article.CreatedAt.ToString("dd/MM/yyyy"),
            HeroCaption: "ARTICLE",
            Title: DisplayText.Normalize(article.Title),
            Subtitle: subtitle,
            Summary: DisplayText.ToExcerpt(summary, 180),
            Meta: $"Par {author}",
            ActionLabel: "Voir les articles",
            AccessibilityText: $"Article {DisplayText.Normalize(article.Title)}, publie le {article.CreatedAt:dd/MM/yyyy}, par {author}. {DisplayText.ToExcerpt(summary, 140)}");
    }

    private static HomeResourceCard ToEventCard(EventResponse @event)
    {
        var author = GetAuthorLabel(@event.Author);
        var location = DisplayText.FirstNonEmpty(@event.Address, @event.Department?.Name, "Lieu a confirmer");
        var summary = DisplayText.FirstNonEmpty(@event.Description, @event.Subtitle, "Aucune description disponible.");

        return new HomeResourceCard(
            Badge: @event.Visibility.Equals("PUBLIC", StringComparison.OrdinalIgnoreCase) ? "Evenement public" : "Evenement prive",
            DateLabel: @event.StartDate.ToString("dd/MM/yyyy"),
            HeroCaption: "EVENT",
            Title: DisplayText.Normalize(@event.Title),
            Subtitle: BuildEventSubtitle(@event, location),
            Summary: DisplayText.ToExcerpt(summary, 180),
            Meta: $"Par {author} | {location}",
            ActionLabel: "Voir les evenements",
            AccessibilityText: $"Evenement {DisplayText.Normalize(@event.Title)}, prevu {BuildEventSubtitle(@event, location)}. {DisplayText.ToExcerpt(summary, 140)}");
    }

    private static string BuildStatusMessage(int totalArticles, int totalEvents)
    {
        if (totalArticles == 0 && totalEvents == 0)
            return "Aucune ressource publique n'a ete trouvee pour le moment.";

        return $"{totalArticles} article(s) et {totalEvents} evenement(s) publics disponibles.";
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

    private static string TrimMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return "Erreur inconnue.";

        return DisplayText.ToExcerpt(message, 180);
    }

    private async Task NavigateToAsync(string route)
    {
        if (Shell.Current is null)
            return;

        SetMobileMenuState(false);

        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Navigation impossible : {TrimMessage(ex.Message)}";
        }
    }

    private async Task NavigateToRootAsync()
    {
        if (Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Retour impossible : {TrimMessage(ex.Message)}";
        }
    }

    private sealed record HomeResourceCard(
        string Badge,
        string DateLabel,
        string HeroCaption,
        string Title,
        string Subtitle,
        string Summary,
        string Meta,
        string ActionLabel,
        string AccessibilityText)
    {
        public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);
    }
}
