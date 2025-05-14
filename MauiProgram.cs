using CaTALK.MVVM.ViewModels;
using CaTALK.MVVM.Views;
using Microsoft.Extensions.Logging;

namespace CaTALK;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
        builder.Services.AddSingleton<UserManagement>();
        builder.Services.AddSingleton<Home>();
        builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
