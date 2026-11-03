using Microsoft.Maui.Platforms.MacOS.Essentials;
using Microsoft.Maui.Platforms.MacOS.Hosting;

namespace PrototipoMacAppKit;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiAppMacOS<App>()
			.AddMacOSEssentials()
			.AddMacOSBlazorWebView();

		builder.Services.AddMauiBlazorWebView();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

		return builder.Build();
	}
}
