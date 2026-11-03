using AppKit;

namespace PrototipoMacAppKit;

public class MainClass
{
	static void Main(string[] args)
	{
		NSApplication.Init();
		NSApplication.SharedApplication.Delegate = new PrototipoMacAppKitDelegate();
		NSApplication.Main(args);
	}
}
