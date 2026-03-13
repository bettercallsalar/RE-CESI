using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Articles;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Pages.Profile;

namespace RESR.MAUI;

public partial class AppShell : Shell
{
	public AppShell(HomePage homePage)
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
		Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
		Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
		Routing.RegisterRoute(nameof(CreateArticlePage), typeof(CreateArticlePage));

		Items.Add(new ShellContent
		{
			Title = "Accueil",
			Route = nameof(HomePage),
			Content = homePage
		});
	}
}

