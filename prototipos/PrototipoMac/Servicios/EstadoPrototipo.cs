namespace PrototipoMac.Servicios;

public sealed class EstadoPrototipo
{
    public bool CambiosPendientes { get; set; }
    public bool DialogoNativoAbierto { get; set; }
    public event Func<bool, Task>? CierreSolicitado;

    public Task SolicitarCierreAsync(bool salir) =>
        CierreSolicitado?.Invoke(salir) ?? Task.CompletedTask;

    public void Cerrar(bool salir)
    {
        var aplicacion = Application.Current;
        if (salir) aplicacion?.Quit();
        else if (aplicacion?.Windows.FirstOrDefault() is { } ventana) aplicacion.CloseWindow(ventana);
    }
}
