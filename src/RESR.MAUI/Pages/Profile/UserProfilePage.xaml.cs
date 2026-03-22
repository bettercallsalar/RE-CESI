using RESR.MAUI.Pages.Articles;
using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Services;
using RESR.Models.Resources;

namespace RESR.MAUI.Pages.Profile;

public partial class UserProfilePage : ContentPage, IQueryAttributable
{
    private const int CarouselItemLimit = 5;
    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IFollowsApiClient _followsApiClient;
    private readonly IUsersApiClient _usersApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;
    private CancellationTokenSource? _followActionCts;
    private int? _idUser;
    private int? _currentUserId;
    private bool _isFollowing;
    private bool _shouldLoad;
    private string _username = string.Empty;
    private string _firstName = string.Empty;
    private IReadOnlyList<UserArticleCard> _articleCards = Array.Empty<UserArticleCard>();

    public UserProfilePage(
        IResourcesApiClient resourcesApiClient,
        IFollowsApiClient followsApiClient,
        IUsersApiClient usersApiClient,
        IApiSession session)
    {
        _resourcesApiClient = resourcesApiClient;
        _followsApiClient = followsApiClient;
        _usersApiClient = usersApiClient;
        _session = session;
        InitializeComponent();
        ApplyArticleState();
        UpdateFollowUi();
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

        _username = Uri.UnescapeDataString(query.TryGetValue("username", out var rawUsername)
            ? rawUsername?.ToString() ?? string.Empty
            : string.Empty);

        _firstName = Uri.UnescapeDataString(query.TryGetValue("firstName", out var rawFirstName)
            ? rawFirstName?.ToString() ?? string.Empty
            : string.Empty);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_shouldLoad || !_idUser.HasValue)
            return;

        _shouldLoad = false;
        await LoadProfileAsync(_idUser.Value);
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        _followActionCts?.Cancel();
        base.OnDisappearing();
    }

    private async Task LoadProfileAsync(int idUser)
    {
        if (_loadCts is not null)
            return;

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true);

        try
        {
            BindProfileHeader(idUser);

            if (_session.IsAuthenticated)
            {
                var me = await _usersApiClient.GetMeAsync(_loadCts.Token);
                _currentUserId = me?.IdUser;
            }
            else
            {
                _currentUserId = null;
            }

            if (_currentUserId.HasValue && _currentUserId.Value != idUser)
                _isFollowing = await _followsApiClient.ExistsAsync(_currentUserId.Value, idUser, _loadCts.Token);
            else
                _isFollowing = false;

            var articles = await _resourcesApiClient.GetArticlesByUserAsync(idUser, 1, CarouselItemLimit, keyword: null, _loadCts.Token);
            _articleCards = articles.Items
                .Take(CarouselItemLimit)
                .Select(ToArticleCard)
                .ToList();
            ApplyArticleState();

            ArticlesSummaryLabel.Text = _articleCards.Count == 0
                ? "Aucun article public pour le moment."
                : $"{articles.TotalCount} article(s) public(s) trouves.";

            StatusLabel.Text = string.Empty;
            UpdateFollowUi();
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = UserFeedback.FromApiException(ex, "Impossible de charger ce profil pour le moment.");
            _articleCards = Array.Empty<UserArticleCard>();
            ApplyArticleState();
        }
        catch (TimeoutException ex)
        {
            StatusLabel.Text = UserFeedback.FromTimeout(ex);
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = string.Empty;
        }
        catch (Exception)
        {
            StatusLabel.Text = UserFeedback.FromUnexpected("Impossible de charger ce profil pour le moment.");
        }
        finally
        {
            _loadCts?.Dispose();
            _loadCts = null;
            SetLoadingState(false);
        }
    }

    private void BindProfileHeader(int idUser)
    {
        var displayName = string.IsNullOrWhiteSpace(_firstName)
            ? (string.IsNullOrWhiteSpace(_username) ? "Utilisateur" : _username)
            : _firstName;

        DisplayNameLabel.Text = displayName;
        UsernameLabel.Text = string.IsNullOrWhiteSpace(_username)
            ? "Profil public"
            : $"@{_username}";
    }

    private void ApplyArticleState()
    {
        ArticlesCarousel.ItemsSource = _articleCards;
        ArticlesCarousel.IsVisible = _articleCards.Count > 0;
        ArticlesEmptyState.IsVisible = _articleCards.Count == 0;
        ArticlesIndicator.IsVisible = _articleCards.Count > 1;
        ViewAllArticlesButton.IsVisible = _articleCards.Count > 0;
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        FollowButton.IsEnabled = !isLoading;
    }

    private void UpdateFollowUi()
    {
        if (!_idUser.HasValue)
        {
            FollowButton.IsVisible = false;
            FollowHintLabel.Text = string.Empty;
            return;
        }

        var isOwnProfile = _currentUserId.HasValue && _currentUserId.Value == _idUser.Value;
        FollowButton.IsVisible = !isOwnProfile;

        if (isOwnProfile)
        {
            FollowHintLabel.Text = "Ceci est votre profil public.";
            return;
        }

        FollowButton.Text = _isFollowing ? "Ne plus suivre" : "Suivre";

        if (!_session.IsAuthenticated)
        {
            FollowHintLabel.Text = "Connectez-vous pour suivre cette personne.";
            return;
        }

        FollowHintLabel.Text = _isFollowing
            ? "Vous suivez deja cette personne."
            : "Suivez cette personne pour retrouver ses contenus.";
    }

    private async void OnFollowClicked(object? sender, EventArgs e)
    {
        if (!_idUser.HasValue || _followActionCts is not null)
            return;

        if (!_session.IsAuthenticated)
        {
            StatusLabel.Text = "Connectez-vous pour suivre cette personne.";
            if (Shell.Current is not null)
                await Shell.Current.GoToAsync(nameof(LoginPage));
            return;
        }

        if (!_currentUserId.HasValue)
        {
            var me = await _usersApiClient.GetMeAsync(CancellationToken.None);
            _currentUserId = me?.IdUser;
        }

        if (!_currentUserId.HasValue || _currentUserId.Value == _idUser.Value)
            return;

        _followActionCts = new CancellationTokenSource();
        FollowButton.IsEnabled = false;

        try
        {
            if (_isFollowing)
            {
                await _followsApiClient.UnfollowAsync(_currentUserId.Value, _idUser.Value, _followActionCts.Token);
                _isFollowing = false;
                StatusLabel.Text = "Vous ne suivez plus ce profil.";
            }
            else
            {
                await _followsApiClient.FollowAsync(_currentUserId.Value, _idUser.Value, _followActionCts.Token);
                _isFollowing = true;
                StatusLabel.Text = "Vous suivez maintenant ce profil.";
            }

            UpdateFollowUi();
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = UserFeedback.FromApiException(ex, "Impossible de mettre a jour l'abonnement pour le moment.");
        }
        catch (TimeoutException ex)
        {
            StatusLabel.Text = UserFeedback.FromTimeout(ex);
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = string.Empty;
        }
        catch (Exception)
        {
            StatusLabel.Text = UserFeedback.FromUnexpected("Impossible de mettre a jour l'abonnement pour le moment.");
        }
        finally
        {
            _followActionCts?.Dispose();
            _followActionCts = null;
            FollowButton.IsEnabled = true;
        }
    }

    private async void OnViewAllClicked(object? sender, EventArgs e)
    {
        if (!_idUser.HasValue || Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(UserArticlesPage)}?idUser={_idUser.Value}&username={Uri.EscapeDataString(_username)}&firstName={Uri.EscapeDataString(_firstName)}&isOwnProfile=false");
    }

    private async void OnArticleTapped(object? sender, TappedEventArgs e)
    {
        if (!TryGetBoundItem<UserArticleCard>(sender, out var item))
            return;

        await NavigateToArticleDetailAsync(item.IdResource, useOwnAccess: false);
    }

    private async void OnArticleOpenClicked(object? sender, EventArgs e)
    {
        if (!TryGetBoundItem<UserArticleCard>(sender, out var item))
            return;

        await NavigateToArticleDetailAsync(item.IdResource, useOwnAccess: false);
    }

    private async Task NavigateToArticleDetailAsync(int idResource, bool useOwnAccess)
    {
        if (Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(ArticleDetailPage)}?idResource={idResource}&useOwnAccess={useOwnAccess.ToString().ToLowerInvariant()}");
    }

    private static UserArticleCard ToArticleCard(ArticleResponse article)
    {
        var summary = FirstNonEmpty(article.Description, article.Content, "Aucune description disponible.");
        return new UserArticleCard(
            article.IdResource,
            article.Title,
            $"Publie le {article.CreatedAt:dd/MM/yyyy}",
            $"Visibilite {article.Visibility.ToLowerInvariant()}",
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

    private sealed record UserArticleCard(
        int IdResource,
        string Title,
        string Subtitle,
        string Meta,
        string Summary);
}
