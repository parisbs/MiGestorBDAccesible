using AppKit;
using Foundation;
using ObjCRuntime;
using PrototipoMacAppKit.Servicios;
using WebKit;

namespace PrototipoMacAppKit.Nativo;

internal sealed class ControladorVistaWeb : IDisposable
{
    private readonly WKWebView _vista;
    private readonly EstadoPrototipo _estado;
    private readonly NSObject? _anterior;
    private readonly DelegadoNavegacion _delegado;
    private readonly IDisposable _observadorVentana;
    private readonly NSObject _observadorFoco;
    private bool _liberado;

    public ControladorVistaWeb(WKWebView vista, EstadoPrototipo estado)
    {
        _vista = vista;
        _estado = estado;
        _estado.FocoSolicitado += Enfocar;
        _anterior = vista.WeakNavigationDelegate;
        _delegado = new DelegadoNavegacion(_anterior, Enfocar, RecuperarProceso);
        vista.WeakNavigationDelegate = _delegado;
        _observadorVentana = vista.AddObserver("window", NSKeyValueObservingOptions.New, _ => Enfocar());
        _observadorFoco = NSNotificationCenter.DefaultCenter.AddObserver(NSWindow.DidBecomeKeyNotification, notificacion =>
        {
            if (notificacion.Object == _vista.Window)
            {
                MenusNativos.Revisar();
                Enfocar();
            }
        });
        Enfocar();
    }

    private void Enfocar() => _vista.BeginInvokeOnMainThread(() =>
    {
        if (_liberado || _estado.DialogoNativoAbierto) return;
        if (_vista.Window is not { IsKeyWindow: true } ventana || ventana.AttachedSheet is not null) return;
        if (!ventana.MakeFirstResponder(_vista))
            Console.Error.WriteLine("[AppKit] WKWebView no aceptó el foco de teclado.");
    });

    private void RecuperarProceso()
    {
        Console.Error.WriteLine("[AppKit] Terminó WebContent. Se recarga la página; se pierde el estado no guardado.");
        _vista.BeginInvokeOnMainThread(() =>
        {
            if (!_liberado) _vista.Reload();
        });
    }

    public void Dispose()
    {
        if (_liberado) return;
        _liberado = true;
        _estado.FocoSolicitado -= Enfocar;
        _observadorVentana.Dispose();
        NSNotificationCenter.DefaultCenter.RemoveObserver(_observadorFoco);
        _observadorFoco.Dispose();
        if (_vista.WeakNavigationDelegate == _delegado) _vista.WeakNavigationDelegate = _anterior;
        _delegado.Dispose();
    }

    // preview.14 no instala un delegado de navegación. El proxy conserva cualquier
    // delegado previo, incluidas sus políticas opcionales, mediante reenvío de ObjC.
    private sealed class DelegadoNavegacion(NSObject? anterior, Action enfocar, Action recuperar) : NSObject, IWKNavigationDelegate
    {
        public override bool RespondsToSelector(Selector? selector) =>
            base.RespondsToSelector(selector) || (anterior?.RespondsToSelector(selector) ?? false);

        [Export("forwardingTargetForSelector:")]
        public NSObject? DestinoDeReenvio(Selector selector) =>
            anterior?.RespondsToSelector(selector) == true ? anterior : null;

        [Export("webView:didFinishNavigation:")]
        public void DidFinishNavigation(WKWebView vista, WKNavigation navegacion)
        {
            if (anterior is IWKNavigationDelegate delegado && anterior.RespondsToSelector(new Selector("webView:didFinishNavigation:")))
                delegado.DidFinishNavigation(vista, navegacion);
            enfocar();
        }

        [Export("webViewWebContentProcessDidTerminate:")]
        public void ContentProcessDidTerminate(WKWebView vista)
        {
            if (anterior is IWKNavigationDelegate delegado && anterior.RespondsToSelector(new Selector("webViewWebContentProcessDidTerminate:")))
                delegado.ContentProcessDidTerminate(vista);
            recuperar();
        }
    }
}
