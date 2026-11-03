using AppKit;

namespace PrototipoMacAppKit.Servicios;

public sealed class EstadoPrototipo
{
    public bool CambiosPendientes { get; set; }
    public bool DialogoNativoAbierto { get; set; }
    public event Func<bool, Task>? CierreSolicitado;
    internal event Action? FocoSolicitado;

    internal NSWindow? Ventana { get; set; }
    internal bool SalidaAutorizada { get; private set; }
    internal bool CierreVentanaAutorizado { get; private set; }

    internal void RecuperarFoco() => FocoSolicitado?.Invoke();

    public Task SolicitarCierreAsync(bool salir) =>
        CierreSolicitado?.Invoke(salir) ?? Task.CompletedTask;

    // Los callbacks nativos vuelven al bucle principal antes de entrar en Blazor.
    internal void SolicitarDesdeAppKit(bool salir)
    {
        NSApplication.SharedApplication.BeginInvokeOnMainThread(async () =>
        {
            try { await SolicitarCierreAsync(salir); }
            catch (Exception ex) { Console.Error.WriteLine($"[AppKit] No se pudo solicitar el cierre: {ex}"); }
        });
    }

    public Task CerrarAsync(bool salir)
    {
        var resultado = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        NSApplication.SharedApplication.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (salir)
                {
                    SalidaAutorizada = true;
                    NSApplication.SharedApplication.Terminate(null);
                }
                else if (Ventana is { } ventana)
                {
                    CierreVentanaAutorizado = true;
                    try { ventana.PerformClose(null); }
                    finally { CierreVentanaAutorizado = false; }
                }
                resultado.TrySetResult();
            }
            catch (Exception ex) { resultado.TrySetException(ex); }
        });
        return resultado.Task;
    }
}
