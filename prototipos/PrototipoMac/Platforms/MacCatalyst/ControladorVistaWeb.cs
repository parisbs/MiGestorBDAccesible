using Foundation;
using PrototipoMac.Servicios;
using UIKit;
using WebKit;

namespace PrototipoMac;

internal sealed class ControladorVistaWeb : IDisposable
{
    private readonly WKWebView _vista;
    private readonly IWKNavigationDelegate _delegadoBlazor;
    private readonly DelegadoNavegacion _delegado;
    private readonly List<NSObject> _observadores = [];
    private bool _liberado;

    public ControladorVistaWeb(WKWebView vista, IWKNavigationDelegate delegadoBlazor)
    {
        _vista = vista;
        _delegadoBlazor = delegadoBlazor;
        // WKWebView retiene débilmente su delegado; conservamos ambos delegados.
        _delegado = new DelegadoNavegacion(delegadoBlazor, SolicitarFoco, RecuperarProceso);
        vista.NavigationDelegate = _delegado;
        var centro = NSNotificationCenter.DefaultCenter;
        _observadores.Add(centro.AddObserver(UIWindow.DidBecomeKeyNotification, aviso =>
        {
            if (aviso.Object == _vista.Window) SolicitarFoco();
        }));
        foreach (var nombre in new[] { UIScene.WillEnterForegroundNotification, UIScene.DidActivateNotification })
        {
            _observadores.Add(centro.AddObserver(nombre, aviso =>
            {
                if (aviso.Object == _vista.Window?.WindowScene) SolicitarFoco();
            }));
        }
        SolicitarFoco();
    }

    private void SolicitarFoco() => _vista.BeginInvokeOnMainThread(() =>
    {
        if (_liberado || _vista.Window is not { IsKeyWindow: true } ventana ||
            ventana.WindowScene?.ActivationState != UISceneActivationState.ForegroundActive) return;
        var estado = IPlatformApplication.Current?.Services.GetService<EstadoPrototipo>();
        if (estado?.DialogoNativoAbierto == true) return;
        if (!_vista.BecomeFirstResponder())
            Console.Error.WriteLine("[PrototipoMac] WKWebView no aceptó el foco del teclado.");
    });

    private void RecuperarProceso()
    {
        Console.Error.WriteLine("[PrototipoMac] Terminó el proceso WebContent. Se recarga la página y se reinicia Blazor.");
        _vista.BeginInvokeOnMainThread(() =>
        {
            if (_liberado) return;
            _vista.Reload();
        });
    }

    public void Dispose()
    {
        if (_liberado) return;
        _liberado = true;
        foreach (var observador in _observadores)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver(observador);
            observador.Dispose();
        }
        _observadores.Clear();
        if (_vista.NavigationDelegate == _delegado) _vista.NavigationDelegate = _delegadoBlazor;
        _delegado.Dispose();
    }

    // Se conservan los siete callbacks que instala IOSWebViewManager en MAUI 10.0.110.
    private sealed class DelegadoNavegacion(
        IWKNavigationDelegate anterior, Action enfocar, Action recuperar) : WKNavigationDelegate
    {
        public override void DecidePolicy(WKWebView vista, WKNavigationAction accion, Action<WKNavigationActionPolicy> decidir)
            => anterior.DecidePolicy(vista, accion, decidir);

        public override void DidStartProvisionalNavigation(WKWebView vista, WKNavigation navegacion)
            => anterior.DidStartProvisionalNavigation(vista, navegacion);

        public override void DidReceiveServerRedirectForProvisionalNavigation(WKWebView vista, WKNavigation navegacion)
            => anterior.DidReceiveServerRedirectForProvisionalNavigation(vista, navegacion);

        public override void DidFailProvisionalNavigation(WKWebView vista, WKNavigation navegacion, NSError error)
            => anterior.DidFailProvisionalNavigation(vista, navegacion, error);

        public override void DidFailNavigation(WKWebView vista, WKNavigation navegacion, NSError error)
            => anterior.DidFailNavigation(vista, navegacion, error);

        public override void DidCommitNavigation(WKWebView vista, WKNavigation navegacion)
            => anterior.DidCommitNavigation(vista, navegacion);

        public override void DidFinishNavigation(WKWebView vista, WKNavigation navegacion)
        {
            anterior.DidFinishNavigation(vista, navegacion);
            enfocar();
        }

        public override void ContentProcessDidTerminate(WKWebView vista) => recuperar();
    }
}
