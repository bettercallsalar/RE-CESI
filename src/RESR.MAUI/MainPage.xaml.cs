using RESR.MAUI.Services;

namespace RESR.MAUI;

public partial class MainPage : ContentPage
{
	private readonly IUsersApiClient _usersApiClient;
	private CancellationTokenSource? _loadCts;

	public MainPage(IUsersApiClient usersApiClient)
	{
		_usersApiClient = usersApiClient;
		InitializeComponent();
	}

	private async void OnLoadClicked(object? sender, EventArgs e)
	{
		if (_loadCts is not null)
		{
			StatusLabel.Text = "Chargement deja en cours...";
			return;
		}

		_loadCts = new CancellationTokenSource();
		SetLoadingState(true);

		try
		{
			var users = await _usersApiClient.GetUsersAsync(_loadCts.Token);
			UsersCollection.ItemsSource = users;
			StatusLabel.Text = $"{users.Count} user(s) charges.";
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
		LoadButton.IsEnabled = !isLoading;
		LoadingIndicator.IsRunning = isLoading;
		LoadingIndicator.IsVisible = isLoading;
	}
}
