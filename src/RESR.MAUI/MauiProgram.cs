using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using RESR.MAUI.Services;

namespace RESR.MAUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<ArticlesPage>();
		builder.Services.AddTransient<EventsPage>();
		builder.Services.AddSingleton<IApiSession, ApiSession>();
		var apiBaseAddress = ResolveApiBaseAddress();
		builder.Services.AddHttpClient<IUsersApiClient, UsersApiClient>(httpClient =>
		{
			httpClient.BaseAddress = apiBaseAddress;
			httpClient.Timeout = TimeSpan.FromSeconds(10);
		});
		builder.Services.AddHttpClient<IResourcesApiClient, ResourcesApiClient>(httpClient =>
		{
			httpClient.BaseAddress = apiBaseAddress;
			httpClient.Timeout = TimeSpan.FromSeconds(10);
		});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}

	private static Uri ResolveApiBaseAddress()
	{
		var host = DeviceInfo.Current.Platform == DevicePlatform.Android
			? "10.0.2.2"
			: "localhost";

		return new Uri($"http://{host}:8080/");
	}
}
