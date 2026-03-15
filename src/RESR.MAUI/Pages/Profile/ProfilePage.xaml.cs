using RESR.MAUI.Pages.Articles;
using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Resources;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly IUsersApiClient _usersApiClient;
    private readonly IResourcesApiClient _resourcesApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;
    private UserResponse? _me;
    private IReadOnlyList<OwnArticleCard> _articleCards = Array.Empty<OwnArticleCard>();

    public ProfilePage(IUsersApiClient usersApiClient, IResourcesApiClient resourcesApiClient, IApiSession session)
    {
        _usersApiClient = usersApiClient;
        _resourcesApiClient = resourcesApiClient;
        _session = session;
        InitializeComponent();
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
        StatusLabel.Text = "Chargement du profil...";

        try
        {
            _me = await _usersApiClient.GetMeAsync(_loadCts.Token);
            if (_me is null)
            {
                StatusLabel.Text = "Profil introuvable.";
                return;
            }

            UsernameLabel.Text = _me.Username;
            EmailLabel.Text = _me.Email;
            FirstNameLabel.Text = _me.FirstName;
            BirthDateLabel.Text = _me.BirthDate?.ToString("yyyy-MM-dd") ?? "Non renseignee";
            BioLabel.Text = string.IsNullOrWhiteSpace(_me.Bio) ? "Non renseignee" : _me.Bio;
            DepartmentLabel.Text = $"{_me.Department.Name} ({_me.Department.Code})";
            VerifiedLabel.Text = _me.IsVerified ? "Oui" : "Non";

            await LoadMyArticlesAsync(_me.IdUser, _loadCts.Token);
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

    private async Task LoadMyArticlesAsync(int idUser, CancellationToken ct)
    {
        var response = await _resourcesApiClient.GetMyArticlesAsync(idUser, 1, 6, keyword: null, ct);
        _articleCards = response.Items.Select(ToOwnArticleCard).ToList();
        ApplyArticlesState();

        MyArticlesSummaryLabel.Text = _articleCards.Count == 0
            ? "Aucun article charge pour le moment."
            : $"{response.TotalCount} article(s) trouves.";
    }

    private void ApplyArticlesState()
    {
        MyArticlesCarousel.ItemsSource = _articleCards;
        MyArticlesCarousel.IsVisible = _articleCards.Count > 0;
        MyArticlesEmptyState.IsVisible = _articleCards.Count == 0;
        MyArticlesIndicator.IsVisible = _articleCards.Count > 1;
        ViewAllArticlesButton.IsVisible = _articleCards.Count > 0;
    }

    private async void OnViewAllArticlesClicked(object? sender, EventArgs e)
    {
        if (_me is null || Shell.Current is null)
            return;

        await Shell.Current.GoToAsync(
            $"{nameof(UserArticlesPage)}?idUser={_me.IdUser}&username={Uri.EscapeDataString(_me.Username)}&firstName={Uri.EscapeDataString(_me.FirstName)}&isOwnProfile=true");
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

        await Shell.Current.GoToAsync(
            $"{nameof(ArticleDetailPage)}?idResource={idResource}&useOwnAccess={useOwnAccess.ToString().ToLowerInvariant()}");
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
        string Summary);
}
