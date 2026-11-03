using AppKit;

namespace PrototipoMacAppKit.Servicios;

public sealed class ArchivosNativos(EstadoPrototipo estado)
{
    public Task<(string Nombre, string Texto)?> AbrirAsync() => EnPrincipalAsync<(string Nombre, string Texto)?>(async () =>
    {
        using var selector = NSOpenPanel.OpenPanel;
        selector.Title = "Abrir archivo de texto";
        selector.CanChooseFiles = true;
        selector.CanChooseDirectories = false;
        selector.AllowsMultipleSelection = false;
        if (!await ElegirAsync(selector)) return null;
        var ruta = selector.Url?.Path ?? throw new IOException("El archivo no tiene una ruta local.");
        return (Path.GetFileName(ruta), await File.ReadAllTextAsync(ruta));
    });

    public Task<string?> GuardarAsync(string texto) => EnPrincipalAsync<string?>(async () =>
    {
        using var selector = NSSavePanel.SavePanel;
        selector.Title = "Guardar como";
        selector.NameFieldStringValue = "Consulta.txt";
        selector.CanCreateDirectories = true;
        if (!await ElegirAsync(selector)) return null;
        var ruta = selector.Url?.Path ?? throw new IOException("El archivo no tiene una ruta local.");
        await File.WriteAllTextAsync(ruta, texto);
        return Path.GetFileName(ruta);
    });

    private async Task<T> EnPrincipalAsync<T>(Func<Task<T>> accion)
    {
        var resultado = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        NSApplication.SharedApplication.BeginInvokeOnMainThread(async () =>
        {
            if (estado.DialogoNativoAbierto)
            {
                resultado.TrySetException(new InvalidOperationException("Ya tienes un diálogo nativo abierto."));
                return;
            }
            estado.DialogoNativoAbierto = true;
            try { resultado.TrySetResult(await accion()); }
            catch (Exception ex) { resultado.TrySetException(ex); }
            finally
            {
                estado.DialogoNativoAbierto = false;
                estado.RecuperarFoco();
            }
        });
        return await resultado.Task;
    }

    private Task<bool> ElegirAsync(NSSavePanel selector)
    {
        var ventana = estado.Ventana ?? throw new InvalidOperationException("No se encontró la ventana del prototipo.");
        var resultado = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        selector.BeginSheet(ventana, respuesta => resultado.TrySetResult(respuesta == (nint)NSModalResponse.OK));
        return resultado.Task;
    }
}
