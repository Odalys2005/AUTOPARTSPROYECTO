// VERSIÓN CORREGIDA: Productos + 21 módulos; reemplaza TODO el JS viejo.
// Prueba en DevTools: window.AUTOPARTS_INTEGRADOS_VERSION
window.AUTOPARTS_INTEGRADOS_VERSION = '21-modulos-productos-corregido-20261024';
console.info('AutoParts módulos integrados cargados:', window.AUTOPARTS_INTEGRADOS_VERSION);

// ============================================================
// AUTOPARTS - MÓDULOS INTEGRADOS EN EL DASHBOARD
// 21 entidades | Un solo MainForm + WebView2
//
// Este archivo se carga DESPUÉS de app.js desde index.html.
// Mantiene los mensajes actuales y prepara los módulos futuros.
// Para un módulo nuevo se crean sus archivos en Web/modulos/.
// Las operaciones SQL todavía deben implementarse en MainForm.cs.
// ============================================================

// Nombres EXACTOS de data-section en index.html, vinculados
// con el nombre del archivo HTML en Web/modulos/.
const configuracionModulosIntegrados = {
    'Productos':                 { slug: 'productos',           descripcion: 'Administración del catálogo de repuestos y sus precios.' },
    'Categorías':                { slug: 'categorias',          descripcion: 'Administración de categorías de repuestos.' },
    'Subcategorías':             { slug: 'subcategorias',       descripcion: 'Administración de subcategorías de repuestos.' },
    'Marcas':                    { slug: 'marcas',              descripcion: 'Administración de marcas registradas.' },
    'Modelos':                   { slug: 'modelos',             descripcion: 'Gestión de modelos de vehículos.' },
    'Vehículos':                 { slug: 'vehiculos',           descripcion: 'Registro y administración de vehículos.' },
    'Compatibilidad':            { slug: 'compatibilidad',     descripcion: 'Compatibilidad entre repuestos y vehículos.' },
    'Inventario':                { slug: 'inventario',          descripcion: 'Control de existencias y niveles mínimos.' },
    'Proveedores':               { slug: 'proveedores',        descripcion: 'Directorio de proveedores.' },
    'Compras':                   { slug: 'compras',             descripcion: 'Registro de compras a proveedores.' },
    'Detalles de compra':        { slug: 'detalles-compra',     descripcion: 'Detalles de las compras registradas.' },
    'Clientes':                  { slug: 'clientes',            descripcion: 'Administración de clientes registrados.' },
    'Ventas':                    { slug: 'ventas',              descripcion: 'Registro de ventas y facturas.' },
    'Detalles de venta':         { slug: 'detalles-venta',      descripcion: 'Detalles de las ventas registradas.' },
    'Garantías':                 { slug: 'garantias',           descripcion: 'Administración de solicitudes de garantía.' },
    'Reemplazos de garantía':    { slug: 'reemplazos-garantia', descripcion: 'Registro de reemplazos por garantía.' },
    'Empleados':                 { slug: 'empleados',           descripcion: 'Administración de empleados.' },
    'Puestos':                   { slug: 'puestos',             descripcion: 'Administración de puestos de trabajo.' },
    'Usuarios':                  { slug: 'usuarios',            descripcion: 'Gestión de cuentas del sistema.' },
    'Roles':                     { slug: 'roles',               descripcion: 'Administración de roles de usuario.' },
    'Estados':                   { slug: 'estados',             descripcion: 'Consulta de estados registrados.', soloLectura: true }
};

const modulosIntegrados = Object.keys(configuracionModulosIntegrados);
const slugsModulosIntegrados = new Set(
    Object.values(configuracionModulosIntegrados).map(item => item.slug)
);

// ============================================================
// ABRIR MÓDULO DENTRO DEL DASHBOARD
// ============================================================
function abrirModuloEnDashboard(modulo, titulo, descripcion, archivo) {
    const dashboard = document.getElementById('dashboardContent');
    const vista = document.getElementById('sectionView');
    const contenido = document.getElementById('sectionBody');
    const encabezado = document.getElementById('sectionHeading');

    if (!dashboard || !vista || !contenido) return;
    if (!slugsModulosIntegrados.has(modulo)) return;

    dashboard.hidden = true;
    vista.hidden = false;

    const tituloElemento = document.getElementById('sectionTitle');
    const descripcionElemento = document.getElementById('sectionDescription');

    if (tituloElemento) tituloElemento.textContent = titulo;
    if (descripcionElemento) descripcionElemento.textContent = descripcion;
    if (encabezado) encabezado.hidden = true;

    contenido.innerHTML = '';

    const marco = document.createElement('iframe');
    marco.id = 'moduloIntegrado';
    marco.title = `Gestión de ${titulo}`;
    marco.dataset.modulo = modulo;
    marco.src = archivo;
    marco.style.display = 'block';
    marco.style.width = '100%';
    marco.style.height = 'calc(100dvh - 145px)';
    marco.style.minHeight = '650px';
    marco.style.border = '0';
    marco.style.borderRadius = '12px';
    marco.style.background = '#090a0d';

    contenido.appendChild(marco);

    document.querySelectorAll('.nav-link').forEach(enlace => {
        enlace.classList.toggle('active', enlace.dataset.section === titulo);
    });

    document.getElementById('sidebar')?.classList.remove('open');
    window.location.hash = modulo;
    window.scrollTo(0, 0);
}

// Forma alternativa de abrir cualquier módulo por su nombre visible.
function abrirModuloPorNombre(nombre) {
    const datos = configuracionModulosIntegrados[nombre];
    if (!datos) return;

    abrirModuloEnDashboard(
        datos.slug,
        nombre,
        datos.descripcion,
        `modulos/${datos.slug}.html`
    );
}

// Conservar los nombres de funciones usados por versiones anteriores de app.js.
function abrirProductosEnDashboard() { abrirModuloPorNombre('Productos'); }
function abrirCategoriasEnDashboard() { abrirModuloPorNombre('Categorías'); }
function abrirSubcategoriasEnDashboard() { abrirModuloPorNombre('Subcategorías'); }
function abrirClientesEnDashboard() { abrirModuloPorNombre('Clientes'); }
function abrirEstadosEnDashboard() { abrirModuloPorNombre('Estados'); }
function abrirMarcasEnDashboard() { abrirModuloPorNombre('Marcas'); }
function abrirProveedoresEnDashboard() { abrirModuloPorNombre('Proveedores'); }
function abrirPuestosEnDashboard() { abrirModuloPorNombre('Puestos'); }
function abrirRolesEnDashboard() { abrirModuloPorNombre('Roles'); }

// ============================================================
// FUNCIONES AUXILIARES
// ============================================================
function obtenerMarcoIntegrado() {
    return document.getElementById('moduloIntegrado');
}

function restaurarEncabezado() {
    const encabezado = document.getElementById('sectionHeading');
    if (encabezado) encabezado.hidden = false;
}

function buscarModuloPorSlug(slug) {
    return Object.values(configuracionModulosIntegrados)
        .find(item => item.slug === slug);
}

function rutaListado(modulo) {
    return `modulos/${modulo}.html`;
}

function rutaNuevo(modulo) {
    return `modulos/${modulo}-nuevo.html`;
}

// ============================================================
// VOLVER AL DASHBOARD
// ============================================================
const botonVolver = document.getElementById('backButton');
if (botonVolver) {
    botonVolver.addEventListener('click', () => {
        restaurarEncabezado();
        const contenido = document.getElementById('sectionBody');
        if (contenido) contenido.innerHTML = '';
    });
}

// Al entrar en una sección no integrada (Configuración, Inicio),
// volvemos a mostrar el encabezado habitual.
document.querySelectorAll('.nav-link').forEach(enlace => {
    enlace.addEventListener('click', () => {
        if (!modulosIntegrados.includes(enlace.dataset.section)) {
            restaurarEncabezado();
        }
    });
});

document.querySelectorAll('[data-open]').forEach(boton => {
    boton.addEventListener('click', () => {
        if (!modulosIntegrados.includes(boton.dataset.open)) {
            restaurarEncabezado();
        }
    });
});

// ============================================================
// MENSAJES DESDE EL HTML DEL MÓDULO HACIA C#
//
// Ejemplos:
//   productos:abrirNuevo   => abre productos-nuevo.html
//   guardar                => productos:guardarIntegrado
//   productos:selectAll    => reenvía consulta a MainForm.cs
//   categorias:selectAll   => reenvía consulta a MainForm.cs
//   obtenerEstados         => reenvía consulta a MainForm.cs
// ============================================================
window.addEventListener('message', evento => {
    const marco = obtenerMarcoIntegrado();
    if (!marco || evento.source !== marco.contentWindow) return;

    const mensaje = evento.data;
    if (!mensaje || typeof mensaje !== 'object' || typeof mensaje.type !== 'string') return;

    const modulo = marco.dataset.modulo;
    const configuracion = buscarModuloPorSlug(modulo);
    if (!configuracion) return;

    if (mensaje.type === `${modulo}:abrirNuevo`) {
        if (!configuracion.soloLectura) {
            marco.src = rutaNuevo(modulo);
        }
        return;
    }

    if (mensaje.type === 'cancelar') {
        marco.src = rutaListado(modulo);
        return;
    }

    if (mensaje.type === 'guardar') {
        if (!configuracion.soloLectura) {
            enviarACSharp({
                type: `${modulo}:guardarIntegrado`,
                data: mensaje.data
            });
        }
        return;
    }

    // Consultas de cualquier entidad, incluso cuando otro módulo
    // necesita categorías, subcategorías, proveedores o estados.
    const tipoConsulta = mensaje.type.endsWith(':selectAll')
        ? mensaje.type.slice(0, -':selectAll'.length)
        : '';

    if (slugsModulosIntegrados.has(tipoConsulta) || mensaje.type === 'obtenerEstados') {
        enviarACSharp(mensaje);
    }
});

// ============================================================
// RESPUESTAS DE C# HACIA EL HTML DEL MÓDULO
//
//   productos:guardado      => vuelve a productos.html
//   productos:list          => envía datos al iframe
//   subcategorias:list      => sirve como lista auxiliar
//   categorias:list         => sirve como lista auxiliar
//   estados                 => llena combos de estado
//   error                   => muestra error al iframe
// ============================================================
if (window.chrome?.webview?.addEventListener) {
    window.chrome.webview.addEventListener('message', evento => {
        let mensaje = evento.data;

        if (typeof mensaje === 'string') {
            try {
                mensaje = JSON.parse(mensaje);
            } catch {
                return;
            }
        }

        if (!mensaje || typeof mensaje !== 'object' || typeof mensaje.type !== 'string') return;

        const marco = obtenerMarcoIntegrado();
        if (!marco) return;

        const modulo = marco.dataset.modulo;
        if (!slugsModulosIntegrados.has(modulo)) return;

        if (mensaje.type === `${modulo}:guardado`) {
            marco.src = rutaListado(modulo);
            return;
        }

        const tipoListado = mensaje.type.endsWith(':list')
            ? mensaje.type.slice(0, -':list'.length)
            : '';

        if (
            mensaje.type === 'error' ||
            mensaje.type === 'estados' ||
            slugsModulosIntegrados.has(tipoListado)
        ) {
            marco.contentWindow?.postMessage(mensaje, '*');
        }
    });
}