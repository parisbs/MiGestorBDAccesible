// Comprobación de rutas JavaScript; no sustituye la prueba de WKWebView con VoiceOver.
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const escuchas = [];
const llamadas = [];
let dialogo = false;
const activo = { getAttribute: () => null, closest: () => null };
const document = {
    activeElement: activo,
    body: {},
    addEventListener(tipo, funcion, captura = false) { escuchas.push({ tipo, funcion, captura }); },
    querySelector(selector) { return dialogo && selector.includes("data-dialogo") ? {} : null; },
    getElementById(id) { return id === "editor-sql" ? { value: "texto", selectionStart: 0, selectionEnd: 5 } : null; }
};
const contexto = vm.createContext({ window: {}, document });
for (const archivo of ["../../MiGestorBDAccesible/wwwroot/js/app.js", "wwwroot/js/prototipo-mac.js"]) {
    vm.runInContext(fs.readFileSync(path.join(__dirname, archivo), "utf8"), contexto, { filename: archivo });
}
const gestor = contexto.window.miGestor;
const referencia = { invokeMethodAsync(metodo, ...argumentos) { llamadas.push({ metodo, argumentos }); return Promise.resolve(); } };
gestor.registrarEspacio(referencia);
gestor.registrarAyuda(referencia);
gestor._barra = referencia;
gestor._elementoBarra = { contains: () => false };

function emitir(key, modificadores = {}, tipo = "keydown") {
    const evento = {
        key, ctrlKey: false, metaKey: false, altKey: false, shiftKey: false, repeat: false,
        cancelado: false, detenido: false,
        preventDefault() { this.cancelado = true; },
        stopImmediatePropagation() { this.detenido = true; },
        ...modificadores
    };
    for (const escucha of escuchas.filter(x => x.tipo === tipo).sort((a, b) => Number(b.captura) - Number(a.captura))) {
        escucha.funcion(evento);
        if (evento.detenido) break;
    }
    return evento;
}

const casos = [
    ["n", {}, "NuevaConsulta", [null]], ["t", {}, "NuevaPestana", [null]],
    ["w", {}, "CerrarPestana", []], ["s", {}, "GuardarArchivo", ["texto", false]],
    ["S", { shiftKey: true }, "GuardarArchivo", ["texto", true]],
    ["o", {}, "AbrirArchivo", ["texto"]], ["f", {}, "AbrirBusqueda", []],
    ["b", {}, "BuscarObjeto", [null]], ["y", {}, "AbrirHistorial", []],
    ["g", {}, "BuscarSiguiente", [false]], ["Enter", {}, "Ejecutar", ["texto", 0, 5]],
    ["F3", { metaKey: false }, "BuscarSiguiente", [false]],
    ["F5", { metaKey: false }, "Ejecutar", ["texto", 0, 5]],
    ["Tab", { metaKey: false, ctrlKey: true }, "CambiarPestana", [1]],
    ["Tab", { metaKey: false, ctrlKey: true, shiftKey: true }, "CambiarPestana", [-1]],
    ["F10", { metaKey: false }, "Activar", []]
];
for (const [tecla, modificadores, metodo, argumentos] of casos) {
    llamadas.length = 0;
    const evento = emitir(tecla, { metaKey: true, ...modificadores });
    assert.equal(evento.cancelado, true, tecla);
    assert.deepEqual(llamadas, [{ metodo, argumentos }], tecla);
}
for (const [tecla, modificadores] of casos) {
    dialogo = true;
    llamadas.length = 0;
    emitir(tecla, { metaKey: true, ...modificadores });
    assert.equal(llamadas.length, 0, `Diálogo abierto: ${tecla}`);
}
dialogo = false;
llamadas.length = 0;
assert.equal(emitir("Alt", { altKey: true }).cancelado, false);
assert.equal(emitir("Alt", {}, "keyup").cancelado, false);
assert.equal(llamadas.length, 0, "Option sola no activa la barra");
assert.equal(emitir("ø", { altKey: true }).cancelado, false, "Option permite escribir");
assert.equal(emitir("q", { metaKey: true }).cancelado, false, "Command+Q queda para la orden nativa");
assert.equal(emitir("h", { metaKey: true }).cancelado, false, "Command+H queda para macOS");
console.log(`${casos.length} rutas de teclado verificadas, bloqueo en diálogo y Option sin capturar.`);
