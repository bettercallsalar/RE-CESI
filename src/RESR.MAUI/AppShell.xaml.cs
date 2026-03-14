using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Articles;
using RESR.MAUI.Pages.Events;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Pages.Profile;

namespace RESR.MAUI;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage)
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
		Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
		Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
		Routing.RegisterRoute(nameof(CreateArticlePage), typeof(CreateArticlePage));
		Routing.RegisterRoute(nameof(ArticleDetailPage), typeof(ArticleDetailPage));
		Routing.RegisterRoute(nameof(ArticlesPage), typeof(ArticlesPage));
		Routing.RegisterRoute(nameof(EventsPage), typeof(EventsPage));

		Items.Add(new ShellContent
		{
			Title = "Ressources",
			Route = nameof(MainPage),
			Content = mainPage
		});
	}
}

