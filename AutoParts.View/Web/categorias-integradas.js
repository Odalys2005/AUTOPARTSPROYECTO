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

function abrirProductosEnDashboard() { abrirModuloPorNombre('Productos'); }
function abrirCategoriasEnDashboard() { abrirModuloPorNombre('Categorías'); }
function abrirSubcategoriasEnDashboard() { abrirModuloPorNombre('Subcategorías'); }
function abrirClientesEnDashboard() { abrirModuloPorNombre('Clientes'); }
function abrirEstadosEnDashboard() { abrirModuloPorNombre('Estados'); }
function abrirMarcasEnDashboard() { abrirModuloPorNombre('Marcas'); }
function abrirProveedoresEnDashboard() { abrirModuloPorNombre('Proveedores'); }
function abrirPuestosEnDashboard() { abrirModuloPorNombre('Puestos'); }
function abrirRolesEnDashboard() { abrirModuloPorNombre('Roles'); }

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

const botonVolver = document.getElementById('backButton');
if (botonVolver) {
    botonVolver.addEventListener('click', () => {
        restaurarEncabezado();
        const contenido = document.getElementById('sectionBody');
        if (contenido) contenido.innerHTML = '';
    });
}

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

    if (modulo === 'compras' && mensaje.type === 'compras:registrarCompleta') {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'ventas' && mensaje.type === 'ventas:registrarCompleta') {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'ventas' && mensaje.type === 'ventas:imprimirTicket') {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'ventas' && mensaje.type === 'ventas:generarPdf') {
        enviarACSharp(mensaje);
        return;
    }

    const operacionesGarantia = {
        'garantias:buscarPorFactura': 'garantias',
        'garantias:registrarSolicitud': 'garantias',
        'garantias:evaluar': 'garantias',
        'reemplazos-garantia:entregar': 'reemplazos-garantia'
    };
    if (Object.prototype.hasOwnProperty.call(operacionesGarantia, mensaje.type)) {
        if (operacionesGarantia[mensaje.type] === modulo) {
            enviarACSharp(mensaje);
        }
        return;
    }

    if (modulo === 'categorias' && (
        mensaje.type === 'categorias:actualizarIntegrado' ||
        mensaje.type === 'categorias:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }
  

    if (modulo === 'subcategorias' && (
        mensaje.type === 'subcategorias:actualizarIntegrado' ||
        mensaje.type === 'subcategorias:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }
    if (modulo === 'productos' && (
        mensaje.type === 'productos:actualizarIntegrado' ||
        mensaje.type === 'productos:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }
    if (modulo === 'empleados' && (
        mensaje.type === 'empleados:actualizarIntegrado' ||
        mensaje.type === 'empleados:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'marcas' && (
        mensaje.type === 'marcas:actualizarIntegrado' ||
        mensaje.type === 'marcas:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'modelos' && (
        mensaje.type === 'modelos:actualizarIntegrado' ||
        mensaje.type === 'modelos:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'vehiculos' && (
        mensaje.type === 'vehiculos:actualizarIntegrado' ||
        mensaje.type === 'vehiculos:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'compatibilidad' && (
        mensaje.type === 'compatibilidad:actualizarIntegrado' ||
        mensaje.type === 'compatibilidad:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'usuarios' && (
        mensaje.type === 'usuarios:actualizarIntegrado' ||
        mensaje.type === 'usuarios:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'proveedores' && (
        mensaje.type === 'proveedores:actualizarIntegrado' ||
        mensaje.type === 'proveedores:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'clientes' && (
        mensaje.type === 'clientes:actualizarIntegrado' ||
        mensaje.type === 'clientes:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'puestos' && (
        mensaje.type === 'puestos:actualizarIntegrado' ||
        mensaje.type === 'puestos:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }

    if (modulo === 'roles' && (
        mensaje.type === 'roles:actualizarIntegrado' ||
        mensaje.type === 'roles:desactivarIntegrado'
    )) {
        enviarACSharp(mensaje);
        return;
    }


    if (mensaje.type === 'guardar') {
        if (modulo === 'garantias' || modulo === 'reemplazos-garantia') {
            marco.contentWindow?.postMessage({
                type: 'error',
                message: 'Utiliza la operación específica de solicitud, evaluación o entrega de garantía.'
            }, '*');
            return;
        }
        if (!configuracion.soloLectura) {
            enviarACSharp({
                type: `${modulo}:guardarIntegrado`,
                data: mensaje.data
            });
        }
        return;
    }

    const tipoConsulta = mensaje.type.endsWith(':selectAll')
        ? mensaje.type.slice(0, -':selectAll'.length)
        : '';

    if (slugsModulosIntegrados.has(tipoConsulta) || mensaje.type === 'obtenerEstados') {
        enviarACSharp(mensaje);
    }
});

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

        if (modulo === 'compras' && (
            mensaje.type === 'compras:registrada' ||
            mensaje.type === 'compras:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'ventas' && mensaje.type === 'ventas:registrada') {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'ventas' && (
            mensaje.type === 'ventas:ticketImpreso' ||
            mensaje.type === 'ventas:ticketError'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'ventas' && (
            mensaje.type === 'ventas:pdfGenerado' ||
            mensaje.type === 'ventas:pdfError'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        const respuestasGarantias = new Set([
            'garantias:ventaEncontrada',
            'garantias:ventaNoEncontrada',
            'garantias:busquedaError',
            'garantias:solicitudRegistrada',
            'garantias:solicitudError',
            'garantias:evaluada',
            'garantias:evaluacionError'
        ]);
        const respuestasReemplazos = new Set([
            'reemplazos-garantia:resultadoEntrega',
            'reemplazos-garantia:entregaError'
        ]);
        if ((modulo === 'garantias' && respuestasGarantias.has(mensaje.type)) ||
            (modulo === 'reemplazos-garantia' && respuestasReemplazos.has(mensaje.type))) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'categorias' && (
            mensaje.type === 'categorias:actualizada' ||
            mensaje.type === 'categorias:desactivada' ||
            mensaje.type === 'categorias:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }
        

        if (modulo === 'subcategorias' && (
            mensaje.type === 'subcategorias:actualizada' ||
            mensaje.type === 'subcategorias:desactivada' ||
            mensaje.type === 'subcategorias:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }
        

        if (modulo === 'productos' && (
            mensaje.type === 'productos:actualizada' ||
            mensaje.type === 'productos:desactivada' ||
            mensaje.type === 'productos:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'proveedores' && (
            mensaje.type === 'proveedores:actualizada' ||
            mensaje.type === 'proveedores:desactivada' ||
            mensaje.type === 'proveedores:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'clientes' && (
            mensaje.type === 'clientes:actualizada' ||
            mensaje.type === 'clientes:desactivada' ||
            mensaje.type === 'clientes:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'empleados' && (
            mensaje.type === 'empleados:actualizada' ||
            mensaje.type === 'empleados:desactivada' ||
            mensaje.type === 'empleados:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'marcas' && (
            mensaje.type === 'marcas:actualizada' ||
            mensaje.type === 'marcas:desactivada' ||
            mensaje.type === 'marcas:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'modelos' && (
            mensaje.type === 'modelos:actualizada' ||
            mensaje.type === 'modelos:desactivada' ||
            mensaje.type === 'modelos:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'vehiculos' && (
            mensaje.type === 'vehiculos:actualizada' ||
            mensaje.type === 'vehiculos:desactivada' ||
            mensaje.type === 'vehiculos:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'compatibilidad' && (
            mensaje.type === 'compatibilidad:actualizada' ||
            mensaje.type === 'compatibilidad:desactivada' ||
            mensaje.type === 'compatibilidad:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'usuarios' && (
            mensaje.type === 'usuarios:actualizada' ||
            mensaje.type === 'usuarios:desactivada' ||
            mensaje.type === 'usuarios:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'puestos' && (
            mensaje.type === 'puestos:actualizada' ||
            mensaje.type === 'puestos:desactivada' ||
            mensaje.type === 'puestos:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'roles' && (
            mensaje.type === 'roles:actualizada' ||
            mensaje.type === 'roles:desactivada' ||
            mensaje.type === 'roles:error'
        )) {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'roles' && mensaje.type === 'roles:guardado') {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'puestos' && mensaje.type === 'puestos:guardado') {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'empleados' && mensaje.type === 'empleados:guardado') {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'marcas' && mensaje.type === 'marcas:guardado') {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'modelos' && mensaje.type === 'modelos:guardado') {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'vehiculos' && mensaje.type === 'vehiculos:guardado') {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'compatibilidad' && mensaje.type === 'compatibilidad:guardado') {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (modulo === 'usuarios' && mensaje.type === 'usuarios:guardado') {
            marco.contentWindow?.postMessage(mensaje, '*');
            return;
        }

        if (mensaje.type === `${modulo}:guardado`) {
            if (modulo === 'proveedores' || modulo === 'subcategorias' || modulo === 'categorias' || modulo === 'productos' || modulo === 'clientes') {
                marco.contentWindow?.postMessage(mensaje, '*');
            } else {
                marco.src = rutaListado(modulo);
            }
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