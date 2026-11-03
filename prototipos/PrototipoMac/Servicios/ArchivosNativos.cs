using Foundation;
using UIKit;
using UniformTypeIdentifiers;

namespace PrototipoMac.Servicios;

public sealed class ArchivosNativos(EstadoPrototipo estado)
{
    private UIDocumentPickerViewController? _selector;
    private DelegadoSelector? _delegado;

    public async Task<(string Nombre, string Texto)?> AbrirAsync()
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var urls = await ElegirAsync(new UIDocumentPickerViewController([UTTypes.Data], true)
            {
                AllowsMultipleSelection = false,
                Title = "Abrir archivo de texto"
            });
            if (urls.Length == 0) return ((string Nombre, string Texto)?)null;
            var url = urls[0];
            var acceso = url.StartAccessingSecurityScopedResource();
            try
            {
                var texto = await File.ReadAllTextAsync(url.Path ?? throw new IOException("El archivo no tiene una ruta local."));
                return (url.LastPathComponent ?? "Archivo", texto);
            }
            finally { if (acceso) url.StopAccessingSecurityScopedResource(); }
        });
    }

    public async Task<string?> GuardarAsync(string texto)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var carpeta = Path.Combine(FileSystem.CacheDirectory, $"exportacion-{Guid.NewGuid():N}");
            Directory.CreateDirectory(carpeta);
            var ruta = Path.Combine(carpeta, "Consulta.txt");
            try
            {
                await File.WriteAllTextAsync(ruta, texto);
                using var url = NSUrl.FromFilename(ruta);
                var urls = await ElegirAsync(new UIDocumentPickerViewController([url], true)
                {
                    Title = "Guardar como"
                });
                return urls.FirstOrDefault()?.LastPathComponent;
            }
            finally
            {
                File.Delete(ruta);
                Directory.Delete(carpeta);
            }
        });
    }

    private async Task<NSUrl[]> ElegirAsync(UIDocumentPickerViewController selector)
    {
        if (estado.DialogoNativoAbierto) throw new InvalidOperationException("Ya tienes un diálogo nativo abierto.");
        var presentador = Microsoft.Maui.ApplicationModel.Platform.GetCurrentUIViewController()
            ?? throw new InvalidOperationException("No se encontró la ventana del prototipo.");
        estado.DialogoNativoAbierto = true;
        _selector = selector;
        var resultado = new TaskCompletionSource<NSUrl[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        _delegado = new DelegadoSelector(resultado);
        selector.Delegate = _delegado;
        try
        {
            await presentador.PresentViewControllerAsync(selector, true);
            return await resultado.Task;
        }
        finally
        {
            if (selector.PresentingViewController is not null) await selector.DismissViewControllerAsync(true);
            selector.Dispose();
            _delegado.Dispose();
            _delegado = null;
            _selector = null;
            estado.DialogoNativoAbierto = false;
        }
    }

    private sealed class DelegadoSelector(TaskCompletionSource<NSUrl[]> resultado) : UIDocumentPickerDelegate
    {
        public override void DidPickDocument(UIDocumentPickerViewController controller, NSUrl[] urls) => resultado.TrySetResult(urls);
        public override void WasCancelled(UIDocumentPickerViewController controller) => resultado.TrySetResult([]);
    }
}
