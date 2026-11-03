// Adaptaciones exclusivas del prototipo. app.js permanece enlazado sin cambios.
window.prototipoMac = {
    referencia: null,
    registrar(referencia) { this.referencia = referencia; },
    cerrarVentana() { this.referencia?.invokeMethodAsync("SolicitarCierre", false); }
};

// Se escucha en captura, antes de que app.js interprete Alt como tecla de menú.
// No se cancela su acción predeterminada: Option debe seguir escribiendo caracteres.
for (const tipo of ["keydown", "keyup"]) {
    document.addEventListener(tipo, e => {
        if (e.key === "Alt") e.stopImmediatePropagation();
    }, true);
}

document.addEventListener("keydown", e => {
    if (!e.metaKey || e.ctrlKey || e.altKey) return;
    const tecla = e.key.toLowerCase();
    const gestor = window.miGestor;
    let accion;
    if (tecla === "s") accion = () => gestor.guardarArchivo(e.shiftKey);
    else if (!e.shiftKey) {
        accion = {
            n: () => gestor.nuevaConsulta(), t: () => gestor.nuevaPestana(),
            w: () => gestor.cerrarPestana(), o: () => gestor.abrirArchivo(),
            f: () => gestor.abrirBusqueda(), b: () => gestor.buscarObjeto(),
            y: () => gestor.abrirHistorial(), g: () => gestor.buscarSiguiente(false),
            enter: () => gestor.ejecutarConsulta()
        }[tecla];
    }
    if (!accion) return;
    e.preventDefault();
    e.stopImmediatePropagation();
    if (!gestor.dialogoAbierto()) accion();
}, true);
