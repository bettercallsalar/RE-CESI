using System.Collections.ObjectModel;
using System.Linq;
using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Services;
using RESR.Models.Departments;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly IUsersApiClient _usersApiClient;
    private readonly IDepartmentsApiClient _departmentsApiClient;
    private readonly IApiSession _session;
    private CancellationTokenSource? _loadCts;
    private UserResponse? _currentUser;

    public ObservableCollection<DepartmentOption> Departments { get; } = new();

    public ProfilePage(IUsersApiClient usersApiClient, IDepartmentsApiClient departmentsApiClient, IApiSession session)
    {
        _usersApiClient = usersApiClient;
        _departmentsApiClient = departmentsApiClient;
        _session = session;
        InitializeComponent();
        BindingContext = this;
        BirthDatePicker.Date = DateTime.Today.AddYears(-18);
        BirthDatePicker.IsEnabled = false;
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
        StatusLabel.TextColor = Colors.Black;
        StatusLabel.Text = "Chargement du profil...";

        try
        {
            var meTask = _usersApiClient.GetMeAsync(_loadCts.Token);
            var departmentsTask = _departmentsApiClient.GetDepartmentsAsync(_loadCts.Token);
            await Task.WhenAll(meTask, departmentsTask);

            var me = meTask.Result;
            if (me is null)
            {
                StatusLabel.Text = "Profil introuvable.";
                return;
            }

            _currentUser = me;
            Departments.Clear();
            foreach (var department in departmentsTask.Result.OrderBy(d => d.Code))
            {
                Departments.Add(new DepartmentOption(department.IdDepartment, department.Name, department.Code));
            }

            UsernameEntry.Text = me.Username;
            EmailEntry.Text = me.Email;
            FirstNameEntry.Text = me.FirstName;
            BioEditor.Text = me.Bio ?? string.Empty;
            HasBirthDateCheckBox.IsChecked = me.BirthDate.HasValue;
            BirthDatePicker.IsEnabled = me.BirthDate.HasValue;
            BirthDatePicker.Date = me.BirthDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today.AddYears(-18);
            DepartmentPicker.SelectedItem = Departments.FirstOrDefault(d => d.IdDepartment == me.Department.IdDepartment);
            VerifiedLabel.Text = me.IsVerified ? "Compte verifie" : "Compte non verifie";
            BanLabel.Text = me.IsBanned ? "Compte suspendu" : "Compte actif";

            StatusLabel.Text = "Profil charge. Tu peux maintenant modifier tes informations.";
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Erreur profil ({(int)ex.StatusCode}): {ex.Message}";
        }
        catch (TimeoutException ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex.Message;
        }
        catch (OperationCanceledException)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Requete annulee.";
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Erreur inattendue: {ex.Message}";
        }
        finally
        {
            _loadCts?.Dispose();
            _loadCts = null;
        }
    }

    private void OnHasBirthDateChanged(object? sender, CheckedChangedEventArgs e)
    {
        BirthDatePicker.IsEnabled = e.Value;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_currentUser is null)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = "Le profil n'est pas encore charge.";
            return;
        }

        SaveButton.IsEnabled = false;
        LogoutButton.IsEnabled = false;
        StatusLabel.TextColor = Colors.Black;
        StatusLabel.Text = "Enregistrement des modifications...";

        try
        {
            var username = UsernameEntry.Text?.Trim();
            var email = EmailEntry.Text?.Trim();
            var firstName = FirstNameEntry.Text?.Trim();
            var bio = BioEditor.Text ?? string.Empty;
            var department = DepartmentPicker.SelectedItem as DepartmentOption;
            var birthDateValue = BirthDatePicker.Date ?? DateTime.Today.AddYears(-18);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(firstName))
            {
                StatusLabel.TextColor = Colors.Red;
                StatusLabel.Text = "Nom d'utilisateur, email et prenom sont obligatoires.";
                return;
            }

            if (department is null)
            {
                StatusLabel.TextColor = Colors.Red;
                StatusLabel.Text = "Selectionne un departement.";
                return;
            }

            var updatedUser = await _usersApiClient.UpdateOwnProfileAsync(
                new UpdateOwnProfileRequest(
                    username,
                    email,
                    firstName,
                    HasBirthDateCheckBox.IsChecked ? DateOnly.FromDateTime(birthDateValue) : null,
                    bio,
                    department.IdDepartment),
                CancellationToken.None);

            _currentUser = updatedUser;
            VerifiedLabel.Text = updatedUser.IsVerified ? "Compte verifie" : "Compte non verifie";
            BanLabel.Text = updatedUser.IsBanned ? "Compte suspendu" : "Compte actif";
            StatusLabel.TextColor = Colors.Green;
            StatusLabel.Text = "Profil mis a jour avec succes.";
        }
        catch (ApiException ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Mise a jour impossible ({(int)ex.StatusCode}): {ex.Message}";
        }
        catch (TimeoutException ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = ex.Message;
        }
        catch (Exception ex)
        {
            StatusLabel.TextColor = Colors.Red;
            StatusLabel.Text = $"Erreur inattendue: {ex.Message}";
        }
        finally
        {
            SaveButton.IsEnabled = true;
            LogoutButton.IsEnabled = true;
        }
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        _session.Clear();
        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
    }

    protected override void OnDisappearing()
    {
        _loadCts?.Cancel();
        base.OnDisappearing();
    }

    public sealed record DepartmentOption(int IdDepartment, string Name, int Code)
    {
        public string DisplayLabel => $"{Code} - {Name}";
    }
}
