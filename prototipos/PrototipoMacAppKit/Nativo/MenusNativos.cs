using AppKit;

namespace PrototipoMacAppKit.Nativo;

internal static class MenusNativos
{
    private static readonly HashSet<string> _teclas = ["n", "t", "w", "s", "o", "f", "b", "y", "g", "\r", "\n"];

    public static void Revisar(bool registrar = false)
    {
        if (NSApplication.SharedApplication.MainMenu is { } menu) Revisar(menu, "", registrar);
        else Console.Error.WriteLine("[AppKit] El backend todavía no tiene un menú principal.");
    }

    private static void Revisar(NSMenu menu, string ruta, bool registrar)
    {
        foreach (var elemento in menu.Items)
        {
            var nombre = $"{ruta}/{elemento.Title}";
            var tecla = elemento.KeyEquivalent.ToLowerInvariant();
            var modificadores = elemento.KeyEquivalentModifierMask &
                (NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ControlKeyMask |
                 NSEventModifierMask.AlternateKeyMask | NSEventModifierMask.ShiftKeyMask);
            if (registrar)
                Console.Error.WriteLine($"[AppKit menú] {nombre}: tecla={System.Text.Json.JsonSerializer.Serialize(tecla)}, modificadores={modificadores}, acción={elemento.Action?.Name}");

            var conflicto = modificadores == NSEventModifierMask.CommandKeyMask && _teclas.Contains(tecla) ||
                modificadores == (NSEventModifierMask.CommandKeyMask | NSEventModifierMask.ShiftKeyMask) && tecla == "s";
            if (conflicto)
            {
                elemento.KeyEquivalent = "";
                Console.Error.WriteLine($"[AppKit menú] Atajo liberado para la página: {nombre}.");
            }
            if (elemento.Submenu is { } submenu) Revisar(submenu, nombre, registrar);
        }
    }
}
