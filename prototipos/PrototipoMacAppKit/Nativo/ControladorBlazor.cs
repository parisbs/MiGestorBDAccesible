using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Platforms.MacOS.Handlers;
using PrototipoMacAppKit.Servicios;
using WebKit;

namespace PrototipoMacAppKit.Nativo;

public sealed class ControladorBlazor : BlazorWebViewHandler
{
    private ControladorVistaWeb? _controlador;

    protected override void ConnectHandler(WKWebView vista)
    {
        base.ConnectHandler(vista);
        _controlador = new ControladorVistaWeb(vista, MauiContext!.Services.GetRequiredService<EstadoPrototipo>());
    }

    protected override void DisconnectHandler(WKWebView vista)
    {
        _controlador?.Dispose();
        _controlador = null;
        base.DisconnectHandler(vista);
    }
}
