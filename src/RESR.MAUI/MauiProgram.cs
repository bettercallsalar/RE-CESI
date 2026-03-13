using Microsoft.Extensions.Logging;
using Microsoft.Maui.Devices;
using RESR.MAUI.Services;
using RESR.MAUI.Pages.Auth;
using RESR.MAUI.Pages.Home;
using RESR.MAUI.Pages.Profile;

namespace RESR.MAUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<ProfilePage>();
		builder.Services.AddSingleton<IApiSession, ApiSession>();

		var baseAddress = DeviceInfo.Platform == DevicePlatform.Android
			? new Uri("http://10.0.2.2:8080/")
			: new Uri("http://localhost:8080/");

		builder.Services.AddHttpClient<IUsersApiClient, UsersApiClient>(httpClient =>
		{
			httpClient.BaseAddress = baseAddress;
			httpClient.Timeout = TimeSpan.FromSeconds(10);
		});
		builder.Services.AddHttpClient<IDepartmentsApiClient, DepartmentsApiClient>(httpClient =>
		{
			httpClient.BaseAddress = baseAddress;
			httpClient.Timeout = TimeSpan.FromSeconds(10);
		});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}


