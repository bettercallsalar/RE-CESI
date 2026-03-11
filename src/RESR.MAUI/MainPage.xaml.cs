using RESR.MAUI.Services;
using RESR.Models.Users;

namespace RESR.MAUI;

public partial class MainPage : ContentPage
{
    private readonly IUsersApiClient _usersApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;

    public MainPage(IUsersApiClient usersApiClient, IApiSession session)
    {
        _usersApiClient = usersApiClient;
        _session = session;
        InitializeComponent();
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        if (_loadCts is not null)
        {
            StatusLabel.Text = "Une requete est deja en cours...";
            return;
        }

        if (!TryBuildRegisterRequest(out var request, out var errorMessage))
        {
            StatusLabel.Text = errorMessage;
            return;
        }

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true);

        try
        {
            await _usersApiClient.RegisterAsync(request, _loadCts.Token);
            StatusLabel.Text = "Inscription reussie. Tu peux maintenant verifier le compte et te connecter.";
            EmailEntry.Text = request.Email;
            PasswordEntry.Text = request.Password;
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur register ({(int)ex.StatusCode}): {ex.Message}";
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
            SetLoadingState(false);
        }
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        if (_loadCts is not null)
        {
            StatusLabel.Text = "Une requete est deja en cours...";
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            StatusLabel.Text = "Email et mot de passe requis.";
            return;
        }

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true);

        try
        {
            await _usersApiClient.LoginAsync(new Login(EmailEntry.Text.Trim(), PasswordEntry.Text), _loadCts.Token);
            TokenEditor.Text = _session.Token;
            StatusLabel.Text = "Login reussi. Le token JWT est affiche et sera envoye au GET /api/users.";
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
            _loadCts.Dispose();
            _loadCts = null;
            SetLoadingState(false);
        }
    }

    private async void OnLoadClicked(object? sender, EventArgs e)
    {
        if (_loadCts is not null)
        {
            StatusLabel.Text = "Chargement deja en cours...";
            return;
        }

        if (!_session.IsAuthenticated)
        {
            StatusLabel.Text = "Fais d'abord un login pour obtenir un token.";
            return;
        }

        _loadCts = new CancellationTokenSource();
        SetLoadingState(true);

        try
        {
            var page = await _usersApiClient.GetUsersAsync(_loadCts.Token);
            UsersCollection.ItemsSource = page.Items;
            StatusLabel.Text = $"{page.Items.Count} user(s) charges. Page {page.Page}/{page.TotalPages}. Requete envoyee avec le bearer JWT courant.";
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur API ({(int)ex.StatusCode}): {ex.Message}";
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
            SetLoadingState(false);
        }
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
    }

    private void SetLoadingState(bool isLoading)
    {
        RegisterButton.IsEnabled = !isLoading;
        LoginButton.IsEnabled = !isLoading;
        LoadButton.IsEnabled = !isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }

    private bool TryBuildRegisterRequest(out RegisterUserRequest request, out string errorMessage)
    {
        request = default!;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(RegisterUsernameEntry.Text) ||
            string.IsNullOrWhiteSpace(RegisterFirstNameEntry.Text) ||
            string.IsNullOrWhiteSpace(RegisterEmailEntry.Text) ||
            string.IsNullOrWhiteSpace(RegisterPasswordEntry.Text))
        {
            errorMessage = "Username, prenom, email et mot de passe sont requis.";
            return false;
        }

        if (!int.TryParse(RegisterDepartmentEntry.Text, out var idDepartment) || idDepartment <= 0)
        {
            errorMessage = "Id department doit etre un entier strictement positif.";
            return false;
        }

        DateOnly? birthDate = null;
        if (!string.IsNullOrWhiteSpace(RegisterBirthDateEntry.Text))
        {
            if (!DateOnly.TryParse(RegisterBirthDateEntry.Text, out var parsedBirthDate))
            {
                errorMessage = "Date de naissance invalide. Format attendu: yyyy-MM-dd.";
                return false;
            }

            birthDate = parsedBirthDate;
        }

        request = new RegisterUserRequest(
            RegisterUsernameEntry.Text.Trim(),
            RegisterEmailEntry.Text.Trim(),
            RegisterPasswordEntry.Text,
            RegisterFirstNameEntry.Text.Trim(),
            birthDate,
            string.IsNullOrWhiteSpace(RegisterBioEntry.Text) ? null : RegisterBioEntry.Text.Trim(),
            idDepartment
        );

        return true;
    }
}
