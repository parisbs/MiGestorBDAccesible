using Foundation;
using Microsoft.Maui.Platforms.MacOS.Platform;

namespace PrototipoMacAppKit;

[Register("PrototipoMacAppKitDelegate")]
public class PrototipoMacAppKitDelegate : MacOSMauiApplication
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
