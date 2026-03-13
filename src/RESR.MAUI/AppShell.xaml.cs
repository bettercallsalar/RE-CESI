namespace RESR.MAUI;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage)
	{
		InitializeComponent();
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
