using Microsoft.AspNetCore.Components.WebView;
using WebKit;

namespace PrototipoMac;

public partial class MainPage : ContentPage
{
    private ControladorVistaWeb? _controlador;

    public MainPage()
    {
        InitializeComponent();
        blazorWebView.HandlerChanging += (_, e) =>
        {
            if (e.OldHandler is not null) LiberarVistaWeb();
        };
    }

    private void AlInicializarVistaWeb(object? remitente, BlazorWebViewInitializedEventArgs e)
    {
        // Este evento precede a IOSWebViewManager: esperamos a que instale su delegado.
        Dispatcher.Dispatch(PrepararVistaWeb);
    }

    private void AlCargarVistaWeb(object? remitente, EventArgs e) => Dispatcher.Dispatch(PrepararVistaWeb);

    private void PrepararVistaWeb()
    {
        if (!blazorWebView.IsLoaded || _controlador is not null ||
            blazorWebView.Handler?.PlatformView is not WKWebView vista) return;
        if (vista.NavigationDelegate is not { } delegado)
        {
            Console.Error.WriteLine("[PrototipoMac] La vista web aún no tiene el delegado de Blazor.");
            return;
        }
        _controlador = new ControladorVistaWeb(vista, delegado);
    }

    private void AlDescargarVistaWeb(object? remitente, EventArgs e) => LiberarVistaWeb();

    private void LiberarVistaWeb()
    {
        _controlador?.Dispose();
        _controlador = null;
    }
}
