using RESR.MAUI.Services;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Auth;

public partial class LoginPage : ContentPage
{
    private readonly IUsersApiClient _usersApiClient;
    private CancellationTokenSource? _loginCts;

    public LoginPage(IUsersApiClient usersApiClient)
    {
        _usersApiClient = usersApiClient;
        InitializeComponent();
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        if (_loginCts is not null)
        {
            StatusLabel.Text = "Une requete est deja en cours...";
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            StatusLabel.Text = "Email et mot de passe requis.";
            return;
        }

        _loginCts = new CancellationTokenSource();

        try
        {
            await _usersApiClient.LoginAsync(new Login(EmailEntry.Text.Trim(), PasswordEntry.Text), _loginCts.Token);
            StatusLabel.Text = "Login reussi.";
            await Shell.Current.GoToAsync("..");
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur login ({(int)ex.StatusCode}): {ex.Message}";
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
            _loginCts.Dispose();
            _loginCts = null;
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
}
