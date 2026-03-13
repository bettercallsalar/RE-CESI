using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly IUsersApiClient _usersApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;

    public ProfilePage(IUsersApiClient usersApiClient, IApiSession session)
    {
        _usersApiClient = usersApiClient;
        _session = session;
        InitializeComponent();
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

    private async Task LoadProfileAsync()
    {
        if (_loadCts is not null)
        {
            StatusLabel.Text = "Chargement deja en cours...";
            return;
        }

        _loadCts = new CancellationTokenSource();
        StatusLabel.Text = "Chargement du profil...";

        try
        {
            UserResponse? me = await _usersApiClient.GetMeAsync(_loadCts.Token);
            if (me is null)
            {
                StatusLabel.Text = "Profil introuvable.";
                return;
            }

            UsernameLabel.Text = me.Username;
            EmailLabel.Text = me.Email;
            FirstNameLabel.Text = me.FirstName;
            BirthDateLabel.Text = me.BirthDate?.ToString("yyyy-MM-dd") ?? "Non renseignee";
            BioLabel.Text = string.IsNullOrWhiteSpace(me.Bio) ? "Non renseignee" : me.Bio;
            DepartmentLabel.Text = $"{me.Department.Name} ({me.Department.Code})";
            VerifiedLabel.Text = me.IsVerified ? "Oui" : "Non";

            StatusLabel.Text = "Profil charge.";
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur profil ({(int)ex.StatusCode}): {ex.Message}";
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

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        _session.Clear();
        await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
    }
}
