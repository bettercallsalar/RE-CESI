using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Departments;
using RESR.Models.Users;
using System.Collections.ObjectModel;
using System.Linq;

namespace RESR.MAUI.Pages.Auth;

public partial class RegisterPage : ContentPage
{
    private static readonly Color MutedStatusColor = Color.FromArgb("#5F5F66");
    private static readonly Color ErrorStatusColor = Color.FromArgb("#AB231E");
    private static readonly Color SuccessStatusColor = Color.FromArgb("#1D6B43");

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

        StatusLabel.TextColor = MutedStatusColor;
        RegisterBirthDatePicker.MinimumDate = new DateTime(1900, 1, 1);
        RegisterBirthDatePicker.MaximumDate = DateTime.Today;
        ToggleBirthDateField(false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (Departments.Count == 0)
        {
            await LoadDepartmentsAsync();
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await NavigateBackAsync();
    }

    private void OnBirthDateCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        ToggleBirthDateField(e.Value);
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        if (_registerCts is not null)
        {
            StatusLabel.TextColor = MutedStatusColor;
            StatusLabel.Text = "Une requete est deja en cours...";
            return;
        }

        if (!TryBuildRegisterRequest(out var request, out var errorMessage))
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = errorMessage;
            return;
        }

        _registerCts = new CancellationTokenSource();
        RegisterButton.IsEnabled = false;
        StatusLabel.TextColor = MutedStatusColor;
        StatusLabel.Text = "Creation du compte en cours...";

        try
        {
            await _usersApiClient.RegisterAsync(request, _registerCts.Token);
            StatusLabel.TextColor = SuccessStatusColor;
            StatusLabel.Text = "Inscription reussie. Vous pouvez maintenant vous connecter.";
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromApiException(ex, "Inscription impossible pour le moment.");
        }
        catch (TimeoutException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromTimeout(ex);
        }
        catch (OperationCanceledException)
        {
            StatusLabel.TextColor = MutedStatusColor;
            StatusLabel.Text = string.Empty;
        }
        catch (Exception)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromUnexpected("Inscription impossible pour le moment.");
        }
        finally
        {
            _registerCts.Dispose();
            _registerCts = null;
            RegisterButton.IsEnabled = true;
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
            errorMessage = "Nom d'utilisateur, prenom, email et mot de passe sont requis.";
            return false;
        }

        if (RegisterDepartmentPicker.SelectedItem is not DepartmentResponse selectedDepartment)
        {
            errorMessage = "Selectionnez un departement.";
            return false;
        }

        DateOnly? birthDate = null;
        if (BirthDateCheckBox.IsChecked && RegisterBirthDatePicker.Date is DateTime birthDateValue)
        {
            birthDate = DateOnly.FromDateTime(birthDateValue);
        }

        request = new RegisterUserRequest(
            RegisterUsernameEntry.Text.Trim(),
            RegisterEmailEntry.Text.Trim(),
            RegisterPasswordEntry.Text,
            RegisterFirstNameEntry.Text.Trim(),
            birthDate,
            string.IsNullOrWhiteSpace(RegisterBioEntry.Text) ? null : RegisterBioEntry.Text.Trim(),
            selectedDepartment.IdDepartment
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
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromApiException(ex, "Impossible de charger les departements pour le moment.");
        }
        catch (TimeoutException ex)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromTimeout(ex);
        }
        catch (Exception)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.FromUnexpected("Impossible de charger les departements pour le moment.");
        }
    }

    private void ToggleBirthDateField(bool isEnabled)
    {
        RegisterBirthDatePicker.IsEnabled = isEnabled;
        BirthDateFieldContainer.Opacity = isEnabled ? 1 : 0.5;
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
        catch (Exception)
        {
            StatusLabel.TextColor = ErrorStatusColor;
            StatusLabel.Text = UserFeedback.BackNavigationError;
        }
    }
}
