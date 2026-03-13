using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Articles;
using RESR.MAUI.Pages.Profile;
using RESR.MAUI.Services;
using RESR.Models.Users;

namespace RESR.MAUI.Pages.Home;

public partial class HomePage : ContentPage
{
    private readonly IApiSession _session;

    public HomePage(IApiSession session)
    {
        _session = session;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateButtons();
    }

    private async void OnGoToLoginClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }

    private async void OnGoToRegisterClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    private async void OnGoToProfileClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }

    private async void OnGoToCreateArticleClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateArticlePage));
    }

    private void UpdateButtons()
    {
        var isAuthenticated = _session.IsAuthenticated;
        LoginButton.IsVisible = !isAuthenticated;
        RegisterButton.IsVisible = !isAuthenticated;
        ProfileButton.IsVisible = isAuthenticated;
        CreateArticleButton.IsVisible = isAuthenticated;
    }
}
