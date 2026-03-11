namespace RESR.MAUI;

public partial class AppShell : Shell
{
	public AppShell(MainPage mainPage)
	{
		InitializeComponent();

		Items.Add(new ShellContent
		{
			Title = "Users",
			Route = nameof(MainPage),
			Content = mainPage
		});
	}
}
