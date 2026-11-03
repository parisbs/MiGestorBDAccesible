using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platforms.MacOS.Controls;
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
			.AddMacOSBlazorWebView()
			.ConfigureMauiHandlers(controladores =>
			{
				controladores.AddHandler<MacOSBlazorWebView, Nativo.ControladorBlazor>();
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<Servicios.AnunciadorService>();
		builder.Services.AddSingleton<Servicios.EstadoPrototipo>();
		builder.Services.AddSingleton<Servicios.ArchivosNativos>();
#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

		return builder.Build();
	}
}
