using RESR.MAUI.Services;
using RESR.Models.Departments;
using RESR.Models.Users;
using System.Collections.ObjectModel;
using System.Linq;

namespace RESR.MAUI.Pages.Auth;

public partial class RegisterPage : ContentPage
{
    private readonly IUsersApiClient _usersApiClient;
    private readonly IDepartmentsApiClient _departmentsApiClient;
    private CancellationTokenSource? _registerCts;

    public ObservableCollection<DepartmentResponse> Departments { get; } = new();

    public RegisterPage(IUsersApiClient usersApiClient, IDepartmentsApiClient departmentsApiClient)
    {
        _usersApiClient = usersApiClient;
        _departmentsApiClient = departmentsApiClient;
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Departments.Count == 0)
        {
            await LoadDepartmentsAsync();
        }
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        if (_registerCts is not null)
        {
            StatusLabel.Text = "Une requete est deja en cours...";
            return;
        }

        if (!TryBuildRegisterRequest(out var request, out var errorMessage))
        {
            StatusLabel.Text = errorMessage;
            return;
        }

        _registerCts = new CancellationTokenSource();

        try
        {
            await _usersApiClient.RegisterAsync(request, _registerCts.Token);
            StatusLabel.Text = "Inscription reussie. Tu peux maintenant te connecter.";
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
            _registerCts.Dispose();
            _registerCts = null;
        }
    }

    private async void OnGoToLoginClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }

    protected override void OnDisappearing()
    {
        _registerCts?.Cancel();
        base.OnDisappearing();
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

        if (RegisterDepartmentPicker.SelectedItem is not DepartmentResponse selectedDepartment)
        {
            errorMessage = "Selectionne un departement.";
            return false;
        }

        var idDepartment = selectedDepartment.IdDepartment;

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

    private async Task LoadDepartmentsAsync()
    {
        try
        {
            var departments = await _departmentsApiClient.GetDepartmentsAsync(CancellationToken.None);
            Departments.Clear();

            foreach (var department in departments.OrderBy(d => d.Code))
            {
                Departments.Add(department);
            }
        }
        catch (ApiException ex)
        {
            StatusLabel.Text = $"Erreur departements ({(int)ex.StatusCode}): {ex.Message}";
        }
        catch (TimeoutException ex)
        {
            StatusLabel.Text = ex.Message;
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Erreur inattendue: {ex.Message}";
        }
    }
}
