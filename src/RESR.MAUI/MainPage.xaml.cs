using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI;

public partial class MainPage : ContentPage
{
    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;
    private bool _hasLoadedOnce;
    private IReadOnlyList<HomeResourceCard> _articleCards = Array.Empty<HomeResourceCard>();
    private IReadOnlyList<HomeResourceCard> _eventCards = Array.Empty<HomeResourceCard>();

    public MainPage(IResourcesApiClient resourcesApiClient, IApiSession session)
    {
        _resourcesApiClient = resourcesApiClient;
        _session = session;
        InitializeComponent();

        HeaderAccountLabel.Text = _session.IsAuthenticated ? "MonCompte" : "Connexion";
        ApplyArticleState();
        ApplyEventState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

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

    private async void OnArticlesHeaderTapped(object? sender, TappedEventArgs e)
    {
        await NavigateToAsync(nameof(ArticlesPage));
    }

    private async void OnEventsHeaderTapped(object? sender, TappedEventArgs e)
    {
        await NavigateToAsync(nameof(EventsPage));
    }

    private async void OnArticleCardTapped(object? sender, TappedEventArgs e)
    {
        await NavigateToAsync(nameof(ArticlesPage));
    }

    private async void OnEventCardTapped(object? sender, TappedEventArgs e)
    {
        await NavigateToAsync(nameof(EventsPage));
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
        StatusLabel.Text = "Utilise les liens Articles et Evenements pour ouvrir les listes de recherche.";
    }

    private async Task LoadResourcesAsync(bool triggeredByRefresh)
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true, triggeredByRefresh);

        try
        {
            var articleTask = _resourcesApiClient.GetArticlesAsync(1, 5, _loadCts.Token);
            var eventTask = _resourcesApiClient.GetEventsAsync(1, 5, _loadCts.Token);

            await Task.WhenAll(articleTask, eventTask);

            var articles = await articleTask;
            var events = await eventTask;

            _articleCards = articles.Items.Select(ToArticleCard).ToList();
            _eventCards = events.Items.Select(ToEventCard).ToList();

            ApplyArticleState();
            ApplyEventState();

            StatusLabel.Text = BuildStatusMessage(articles.TotalCount, events.TotalCount);
            HeaderAccountLabel.Text = _session.IsAuthenticated ? "MonCompte" : "Connexion";
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
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

    private static HomeResourceCard ToArticleCard(ArticleResponse article)
    {
        var description = FirstNonEmpty(article.Description, article.Content, "Aucune description disponible.");
        return new HomeResourceCard(
            Badge: "Article public",
            HeroCaption: "ARTICLE",
            Title: article.Title,
            Subtitle: $"Publie le {article.CreatedAt:dd/MM/yyyy}",
            Summary: ToExcerpt(description, 180),
            Meta: $"Auteur #{article.IdUser}  |  Visibilite {article.Visibility.ToLowerInvariant()}");
    }

    private static HomeResourceCard ToEventCard(EventResponse @event)
    {
        var description = FirstNonEmpty(@event.Description, @event.Subtitle, "Aucune description disponible.");
        var location = FirstNonEmpty(@event.Address, @event.Department?.Name, "Lieu a confirmer");

        return new HomeResourceCard(
            Badge: "Evenement public",
            HeroCaption: "EVENT",
            Title: @event.Title,
            Subtitle: BuildEventSubtitle(@event, location),
            Summary: ToExcerpt(description, 180),
            Meta: $"Organise par #{@event.IdUser}  |  {location}");
    }

    private static string BuildStatusMessage(int totalArticles, int totalEvents)
    {
        if (totalArticles == 0 && totalEvents == 0)
            return "Aucune ressource publique n'a ete trouvee pour le moment.";

        return $"{totalArticles} article(s) et {totalEvents} evenement(s) publics charges.";
    }

    private static string BuildEventSubtitle(EventResponse @event, string location)
    {
        var dateLine = @event.EndDate.HasValue
            ? $"Du {@event.StartDate:dd/MM/yyyy} au {@event.EndDate:dd/MM/yyyy}"
            : $"Le {@event.StartDate:dd/MM/yyyy}";

        return $"{dateLine}  |  {location}";
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

    private static async Task NavigateToAsync(string route)
    {
        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(route);
    }

    private sealed record HomeResourceCard(
        string Badge,
        string HeroCaption,
        string Title,
        string Subtitle,
        string Summary,
        string Meta)
    {
        public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);
    }
}
