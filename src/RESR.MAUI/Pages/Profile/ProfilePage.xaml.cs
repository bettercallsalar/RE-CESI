using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Pages.Marks;
using RESR.MAUI.Services;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly IUsersApiClient _usersApiClient;
    private readonly IMarkedResourcesService _markedResourcesService;
    private readonly IApiSession _session;

    private CancellationTokenSource? _loadCts;
    private IReadOnlyList<MarkedResourceItem> _favoriteItems = Array.Empty<MarkedResourceItem>();
    private IReadOnlyList<MarkedResourceItem> _readLaterItems = Array.Empty<MarkedResourceItem>();

    public ProfilePage(
        IUsersApiClient usersApiClient,
        IMarkedResourcesService markedResourcesService,
        IApiSession session)
    {
        _usersApiClient = usersApiClient;
        _markedResourcesService = markedResourcesService;
        _session = session;

        InitializeComponent();
        ApplyCarouselsState();
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
        StatusLabel.Text = "Chargement du profil et des marques...";

        try
        {
            var profileTask = _usersApiClient.GetMeAsync(_loadCts.Token);
            var favoritesTask = _markedResourcesService.GetFavoritesAsync(_loadCts.Token);
            var readLaterTask = _markedResourcesService.GetReadLaterAsync(_loadCts.Token);

            await Task.WhenAll(profileTask, favoritesTask, readLaterTask);

            var me = await profileTask;
            if (me is null)
            {
                StatusLabel.Text = "Profil introuvable.";
                return;
            }

            BindProfile(me);

            _favoriteItems = await favoritesTask;
            _readLaterItems = await readLaterTask;

            ApplyCarouselsState();
            StatusLabel.Text = "Profil charge.";
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur profil ({(int)ex.StatusCode}) : {ex.Message}";
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
            StatusLabel.Text = $"Erreur inattendue: {ex.Message}";
        }
        finally
        {
            _loadCts.Dispose();
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

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        _session.Clear();
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
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
