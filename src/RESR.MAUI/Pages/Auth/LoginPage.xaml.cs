using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Auth;

public partial class LoginPage : ContentPage
{
    private static readonly Color MutedStatusColor = Color.FromArgb("#5F5F66");
    private static readonly Color ErrorStatusColor = Color.FromArgb("#AB231E");
    private static readonly Color SuccessStatusColor = Color.FromArgb("#1D6B43");

    private readonly IUsersApiClient _usersApiClient;
    private CancellationTokenSource? _loginCts;

    public LoginPage(IUsersApiClient usersApiClient)
    {
        _usersApiClient = usersApiClient;
        InitializeComponent();
        StatusLabel.TextColor = MutedStatusColor;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await NavigateBackAsync();
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        if (_loginCts is not null)
        {
            StatusLabel.TextColor = MutedStatusColor;
            StatusLabel.Text = "Une requete est deja en cours...";
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = "Email et mot de passe requis.";
            return;
        }

        _loginCts = new CancellationTokenSource();
        LoginButton.IsEnabled = false;
        StatusLabel.TextColor = MutedStatusColor;
        StatusLabel.Text = "Connexion en cours...";

        try
        {
            await _usersApiClient.LoginAsync(new Login(EmailEntry.Text.Trim(), PasswordEntry.Text), _loginCts.Token);
            StatusLabel.TextColor = SuccessStatusColor;
            StatusLabel.Text = "Connexion reussie.";
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = $"Erreur login ({(int)ex.StatusCode}) : {DisplayText.ToExcerpt(ex.Message, 180)}";
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
            _loginCts.Dispose();
            _loginCts = null;
            LoginButton.IsEnabled = true;
        }
    }

    private async void OnGoToRegisterClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    protected override void OnDisappearing()
    {
        _loginCts?.Cancel();
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
