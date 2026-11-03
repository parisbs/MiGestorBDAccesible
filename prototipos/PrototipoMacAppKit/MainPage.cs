using Microsoft.Maui.Platforms.MacOS.Controls;

namespace PrototipoMacAppKit;

public class MainPage : ContentPage
{
	public MainPage()
	{
		Title = "Prototipo accesible para macOS (AppKit)";
		var vista = new MacOSBlazorWebView { HostPage = "wwwroot/index.html" };
		vista.RootComponents.Add(new BlazorRootComponent { Selector = "#app", ComponentType = typeof(Components.Routes) });
		Content = vista;
	}
}
