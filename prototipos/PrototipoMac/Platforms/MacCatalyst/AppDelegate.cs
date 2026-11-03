using Foundation;
using ObjCRuntime;
using UIKit;
using PrototipoMac.Servicios;

namespace PrototipoMac;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void BuildMenu(IUIMenuBuilder builder)
    {
        base.BuildMenu(builder);
        if (builder.System != UIMenuSystem.MainSystem) return;

        // Archivo reserva Command+N/O/S/W; Buscar reserva Command+F/G.
        // El contenido web será el único responsable de estos atajos.
        builder.RemoveMenu(UIMenuIdentifier.File.GetConstant()!.ToString());
        builder.RemoveMenu(UIMenuIdentifier.Find.GetConstant()!.ToString());
        // Retiramos Formato y sus submenús para liberar Command+B/I/U/T.
        builder.RemoveMenu(UIMenuIdentifier.Format.GetConstant()!.ToString());
        foreach (var identificador in new[] { UIMenuIdentifier.Edit, UIMenuIdentifier.View, UIMenuIdentifier.Window, UIMenuIdentifier.Help })
        {
            var nombre = identificador.GetConstant()!.ToString();
            var menu = builder.GetMenu(nombre);
            if (menu is not null) builder.ReplaceMenu(nombre, LimpiarAtajos(menu));
        }

        var salir = UIKeyCommand.Create(new NSString("q"), UIKeyModifierFlags.Command, new Selector("solicitarSalida:"));
        salir.Title = "Salir del prototipo";
        var menuSalir = UIMenu.Create("", null, UIMenuIdentifier.Quit, UIMenuOptions.DisplayInline, [salir]);
        builder.ReplaceMenu(UIMenuIdentifier.Quit.GetConstant()!.ToString(), menuSalir);
    }

    private static UIMenu LimpiarAtajos(UIMenu menu)
    {
        var hijos = new List<UIMenuElement>();
        foreach (var hijo in menu.Children)
        {
            if (hijo is UIMenu submenu) hijos.Add(LimpiarAtajos(submenu));
            else if (hijo is not UIKeyCommand comando ||
                     !comando.ModifierFlags.HasFlag(UIKeyModifierFlags.Command) ||
                     !new[] { "n", "t", "w", "s", "o", "f", "b", "y", "g", "\r" }.Contains(comando.Input?.ToString().ToLowerInvariant()))
                hijos.Add(hijo);
        }
        return menu.GetMenuByReplacingChildren(hijos.ToArray());
    }

    [Export("solicitarSalida:")]
    public void SolicitarSalida(NSObject remitente)
    {
        var estado = IPlatformApplication.Current?.Services.GetService<EstadoPrototipo>();
        if (estado is not null) _ = estado.SolicitarCierreAsync(true);
    }
}
