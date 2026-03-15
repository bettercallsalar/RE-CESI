using RESR.MAUI.Pages.Articles;
using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Pages.Marks;
using RESR.MAUI.Services;
using RESR.Models.Resources;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly IUsersApiClient _usersApiClient;
    private readonly IMarkedResourcesService _markedResourcesService;
    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IApiSession _session;

    private CancellationTokenSource? _loadCts;
    private UserResponse? _me;
    private IReadOnlyList<MarkedResourceItem> _favoriteItems = Array.Empty<MarkedResourceItem>();
    private IReadOnlyList<MarkedResourceItem> _readLaterItems = Array.Empty<MarkedResourceItem>();
    private IReadOnlyList<OwnArticleCard> _articleCards = Array.Empty<OwnArticleCard>();

    public ProfilePage(
        IUsersApiClient usersApiClient,
        IMarkedResourcesService markedResourcesService,
        IResourcesApiClient resourcesApiClient,
        IApiSession session)
    {
        _usersApiClient = usersApiClient;
        _markedResourcesService = markedResourcesService;
        _resourcesApiClient = resourcesApiClient;
        _session = session;

        InitializeComponent();
        ApplyCarouselsState();
        ApplyArticlesState();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_session.IsAuthenticated)
        {
            StatusLabel.Text = "Connecte-toi pour acceder a ton profil.";
            await Shell.Current.GoToAsync(nameof(LoginPage));
            return;
        }

        await LoadProfileAsync();
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
    }

    private async Task LoadProfileAsync()
    {
        if (_loadCts is not null)
        {
            StatusLabel.Text = "Chargement deja en cours...";
            return;
        }

        _loadCts = new CancellationTokenSource();
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        StatusLabel.Text = "Chargement du profil, des marques et des articles...";

        try
        {
            var profileTask = _usersApiClient.GetMeAsync(_loadCts.Token);
            var favoritesTask = _markedResourcesService.GetFavoritesAsync(_loadCts.Token);
            var readLaterTask = _markedResourcesService.GetReadLaterAsync(_loadCts.Token);

            _me = await profileTask;
            if (_me is null)
            {
                StatusLabel.Text = "Profil introuvable.";
                return;
            }

            BindProfile(_me);

            var articlesTask = _resourcesApiClient.GetMyArticlesAsync(_me.IdUser, 1, 6, keyword: null, _loadCts.Token);

            await Task.WhenAll(favoritesTask, readLaterTask, articlesTask);

            _favoriteItems = await favoritesTask;
            _readLaterItems = await readLaterTask;
            var articles = await articlesTask;

            _articleCards = articles.Items.Select(ToOwnArticleCard).ToList();

            ApplyCarouselsState();
            ApplyArticlesState();

            MyArticlesSummaryLabel.Text = _articleCards.Count == 0
                ? "Aucun article charge pour le moment."
                : $"{articles.TotalCount} article(s) trouves.";

            StatusLabel.Text = "Profil charge.";
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur profil ({(int)ex.StatusCode}) : {TrimMessage(ex.Message)}";
        }
        catch (TimeoutException ex)
        {
            StatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = "Requete annulee.";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Erreur inattendue : {TrimMessage(ex.Message)}";
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            _loadCts?.Dispose();
            _loadCts = null;
        }
    }

    private void BindProfile(UserResponse me)
    {
        UsernameLabel.Text = me.Username;
        EmailLabel.Text = me.Email;
        FirstNameLabel.Text = me.FirstName;
        BirthDateLabel.Text = me.BirthDate?.ToString("yyyy-MM-dd") ?? "Non renseignee";
        BioLabel.Text = string.IsNullOrWhiteSpace(me.Bio) ? "Non renseignee" : me.Bio;
        DepartmentLabel.Text = $"{me.Department.Name} ({me.Department.Code})";
        VerifiedLabel.Text = me.IsVerified ? "Oui" : "Non";
    }

    private void ApplyCarouselsState()
    {
        FavoritesCarousel.ItemsSource = _favoriteItems;
        FavoritesCarousel.IsVisible = _favoriteItems.Count > 0;
        FavoritesEmptyState.IsVisible = _favoriteItems.Count == 0;
        FavoritesIndicator.IsVisible = _favoriteItems.Count > 1;

        ReadLaterCarousel.ItemsSource = _readLaterItems;
        ReadLaterCarousel.IsVisible = _readLaterItems.Count > 0;
        ReadLaterEmptyState.IsVisible = _readLaterItems.Count == 0;
        ReadLaterIndicator.IsVisible = _readLaterItems.Count > 1;
    }

    private void ApplyArticlesState()
    {
        MyArticlesCarousel.ItemsSource = _articleCards;
        MyArticlesCarousel.IsVisible = _articleCards.Count > 0;
        MyArticlesEmptyState.IsVisible = _articleCards.Count == 0;
        MyArticlesIndicator.IsVisible = _articleCards.Count > 1;
        ViewAllArticlesButton.IsVisible = _articleCards.Count > 0 && _me is not null;
    }

    private async void OnMarkedResourceTapped(object? sender, TappedEventArgs e)
    {
        await OpenMarkedResourceAsync(sender);
    }

    private async void OnMarkedResourceOpenClicked(object? sender, EventArgs e)
    {
        await OpenMarkedResourceAsync(sender);
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

    private async void OnSeeAllFavoritesClicked(object? sender, EventArgs e)
    {
        await NavigateToMarksAsync(MarkResourcesMode.Favorite);
    }

    private async void OnSeeAllReadLaterClicked(object? sender, EventArgs e)
    {
        await NavigateToMarksAsync(MarkResourcesMode.ReadLater);
    }

    private async Task NavigateToMarksAsync(MarkResourcesMode mode)
    {
        if (Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync($"{nameof(MarkResourcesPage)}?mode={mode}");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Navigation impossible : {TrimMessage(ex.Message)}";
        }
    }

    private async void OnViewAllArticlesClicked(object? sender, EventArgs e)
    {
        if (_me is null || Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync(
                $"{nameof(UserArticlesPage)}?idUser={_me.IdUser}&username={Uri.EscapeDataString(_me.Username)}&firstName={Uri.EscapeDataString(_me.FirstName)}&isOwnProfile=true");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Navigation impossible : {TrimMessage(ex.Message)}";
        }
    }

    private async void OnMyArticleTapped(object? sender, TappedEventArgs e)
    {
        if (!TryGetBoundItem<OwnArticleCard>(sender, out var item))
            return;

        await NavigateToArticleDetailAsync(item.IdResource, useOwnAccess: true);
    }

    private async void OnMyArticleOpenClicked(object? sender, EventArgs e)
    {
        if (!TryGetBoundItem<OwnArticleCard>(sender, out var item))
            return;

        await NavigateToArticleDetailAsync(item.IdResource, useOwnAccess: true);
    }

    private async Task NavigateToArticleDetailAsync(int idResource, bool useOwnAccess)
    {
        if (Shell.Current is null)
            return;

        try
        {
            await Shell.Current.GoToAsync(
                $"{nameof(ArticleDetailPage)}?idResource={idResource}&useOwnAccess={useOwnAccess.ToString().ToLowerInvariant()}");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Navigation impossible : {TrimMessage(ex.Message)}";
        }
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        _session.Clear();
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }

    private static OwnArticleCard ToOwnArticleCard(ArticleResponse article)
    {
        var summary = FirstNonEmpty(article.Description, article.Content, "Aucune description disponible.");
        var metaParts = new List<string>
        {
            $"Visibilite {article.Visibility.ToLowerInvariant()}"
        };

        if (!article.IsApproved)
            metaParts.Add("non approuve");

        return new OwnArticleCard(
            article.IdResource,
            article.Title,
            $"Publie le {article.CreatedAt:dd/MM/yyyy}",
            string.Join("  |  ", metaParts),
            ToExcerpt(summary, 180));
    }

    private static bool TryGetBoundItem<TItem>(object? sender, out TItem item) where TItem : class
    {
        item = ((sender as BindableObject)?.BindingContext as TItem)!;
        return item is not null;
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

    private sealed record OwnArticleCard(
        int IdResource,
        string Title,
        string Subtitle,
        string Meta,
        string Summary)
    {
        public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);
    }
}
