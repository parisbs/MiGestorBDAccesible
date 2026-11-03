using AppKit;
using Foundation;
using PrototipoMacAppKit.Servicios;

namespace PrototipoMacAppKit.Nativo;

internal sealed class ControladorVentana : IDisposable
{
    private readonly NSWindow _ventana;
    private readonly EstadoPrototipo _estado;
    private readonly INSWindowDelegate? _anterior;
    private DelegadoVentana? _delegado;

    public ControladorVentana(NSWindow ventana, EstadoPrototipo estado)
    {
        _ventana = ventana;
        _estado = estado;
        _estado.Ventana = ventana;
        _anterior = ventana.Delegate;
        _delegado = new DelegadoVentana(this);
        ventana.Delegate = _delegado;
    }

    public void Dispose()
    {
        if (_delegado is null) return;
        if (_ventana.Delegate == _delegado) _ventana.Delegate = _anterior;
        if (_estado.Ventana == _ventana) _estado.Ventana = null;
        _delegado.Dispose();
        _delegado = null;
    }

    // preview.14 implementa estos dos callbacks. Se restaura el delegado antes de
    // WillClose para que WindowHandler conserve su limpieza y su coordinador de cierre.
    private sealed class DelegadoVentana(ControladorVentana controlador) : NSWindowDelegate
    {
        public override bool WindowShouldClose(NSObject remitente)
        {
            var estado = controlador._estado;
            if (!estado.CierreVentanaAutorizado && !estado.SalidaAutorizada)
            {
                estado.SolicitarDesdeAppKit(false);
                return false;
            }
            return controlador._anterior?.WindowShouldClose(remitente) ?? true;
        }

        public override void WillClose(NSNotification notificacion)
        {
            controlador._estado.Ventana = null;
            controlador._ventana.Delegate = controlador._anterior;
            controlador._anterior?.WillClose(notificacion);
            controlador._ventana.BeginInvokeOnMainThread(controlador.Dispose);
        }
    }
}
