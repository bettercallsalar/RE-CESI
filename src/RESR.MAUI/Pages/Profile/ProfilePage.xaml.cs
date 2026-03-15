using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Profile;

public partial class ProfilePage : ContentPage
{
    private static readonly Color MutedStatusColor = Color.FromArgb("#5F5F66");
    private static readonly Color ErrorStatusColor = Color.FromArgb("#AB231E");
    private static readonly Color SuccessStatusColor = Color.FromArgb("#1D6B43");

    private readonly IUsersApiClient _usersApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;

    public ProfilePage(IUsersApiClient usersApiClient, IApiSession session)
    {
        _usersApiClient = usersApiClient;
        _session = session;
        InitializeComponent();
        StatusLabel.TextColor = MutedStatusColor;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_session.IsAuthenticated)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = "Connectez-vous pour acceder a votre profil.";
            await Shell.Current.GoToAsync(nameof(LoginPage));
            return;
        }

        await LoadProfileAsync();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await NavigateBackAsync();
    }

    private async Task LoadProfileAsync()
    {
        if (_loadCts is not null)
        {
            StatusLabel.TextColor = MutedStatusColor;
            StatusLabel.Text = "Chargement deja en cours...";
            return;
        }

        _loadCts = new CancellationTokenSource();
        LogoutButton.IsEnabled = false;
        StatusLabel.TextColor = MutedStatusColor;
        StatusLabel.Text = "Chargement du profil...";

        try
        {
            UserResponse? me = await _usersApiClient.GetMeAsync(_loadCts.Token);
            if (me is null)
            {
                StatusLabel.TextColor = ErrorStatusColor;
                StatusLabel.Text = "Profil introuvable.";
                return;
            }

            UsernameLabel.Text = me.Username;
            EmailLabel.Text = me.Email;
            FirstNameLabel.Text = me.FirstName;
            BirthDateLabel.Text = me.BirthDate?.ToString("dd/MM/yyyy") ?? "Non renseignee";
            BioLabel.Text = string.IsNullOrWhiteSpace(me.Bio) ? "Non renseignee" : me.Bio;
            DepartmentLabel.Text = $"{me.Department.Name} ({me.Department.Code})";
            VerifiedLabel.Text = me.IsVerified ? "Compte verifie" : "Verification en attente";
            VerifiedBadge.Style = (Style)Application.Current!.Resources[me.IsVerified ? "FilledBadgeBorderStyle" : "OutlineBadgeBorderStyle"];
            VerifiedLabel.Style = (Style)Application.Current!.Resources[me.IsVerified ? "BadgeLabelStyle" : "OutlineBadgeLabelStyle"];

            StatusLabel.TextColor = SuccessStatusColor;
            StatusLabel.Text = "Profil charge.";
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Erreur profil ({(int)ex.StatusCode}) : {DisplayText.ToExcerpt(ex.Message, 180)}";
        }
        catch (TimeoutException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            StatusLabel.TextColor = MutedStatusColor;
            StatusLabel.Text = "Requete annulee.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Erreur inattendue : {DisplayText.ToExcerpt(ex.Message, 180)}";
        }
        finally
        {
            _loadCts.Dispose();
            _loadCts = null;
            LogoutButton.IsEnabled = true;
        }
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        _session.Clear();
        StatusLabel.TextColor = SuccessStatusColor;
        StatusLabel.Text = "Deconnexion reussie.";
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
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
            StatusLabel.Text = $"Retour impossible : {DisplayText.ToExcerpt(ex.Message, 160)}";
        }
    }
}
