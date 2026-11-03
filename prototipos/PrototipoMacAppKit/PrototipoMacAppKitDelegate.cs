using AppKit;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platforms.MacOS.Platform;
using PrototipoMacAppKit.Nativo;
using PrototipoMacAppKit.Servicios;

namespace PrototipoMacAppKit;

[Register("PrototipoMacAppKitDelegate")]
public class PrototipoMacAppKitDelegate : MacOSMauiApplication
{
    private ControladorVentana? _controladorVentana;

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void DidFinishLaunching(NSNotification notificacion)
    {
        base.DidFinishLaunching(notificacion);
        // CreatePlatformWindow instancia WindowHandler directamente en preview.14;
        // por eso se envuelve la ventana creada, sin registrar otro handler.
        if (Windows.FirstOrDefault()?.Handler?.PlatformView is NSWindow ventana)
            _controladorVentana = new ControladorVentana(ventana, Services.GetRequiredService<EstadoPrototipo>());
        // La base ya construyó el menú y la ventana. Se inspecciona el menú real.
        MenusNativos.Revisar(registrar: true);
    }

    public override NSApplicationTerminateReply ApplicationShouldTerminate(NSApplication aplicacion)
    {
        var estado = Services.GetRequiredService<EstadoPrototipo>();
        if (estado.SalidaAutorizada || estado.Ventana is null) return NSApplicationTerminateReply.Now;
        estado.SolicitarDesdeAppKit(true);
        return NSApplicationTerminateReply.Cancel;
    }
}
