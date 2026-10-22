
/* ============================================================
   AUTOPARTS - DASHBOARD PRINCIPAL
   Archivo: app.js

   - Navegación del Dashboard
   - Iconos SVG
   - Accesos rápidos
   - Perfil de usuario
   - Buscador general
   - Comunicación con Windows Forms mediante WebView2

   IMPORTANTE:
   Categorías se abre en frmCategorias.
   Nueva categoría se abre desde frmCategoriasNuevo.
   Ya NO se utiliza un modal JavaScript para categorías.
   ============================================================ */


/* ============================================================
   1. ICONOS SVG
   ============================================================ */

const icons = {

    home: '<path d="m3 10 9-7 9 7v11h-6v-7H9v7H3z"/>',

    box: '<path d="m12 2 9 5-9 5-9-5 9-5Zm-9 5v10l9 5 9-5V7M12 12v10"/>',

    grid: '<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>',

    tag: '<path d="M20 13 13 20 3 10V3h7l10 10ZM7 7h.01"/>',

    package: '<path d="m12 2 9 5-9 5-9-5 9-5Zm-9 5v10l9 5 9-5V7M12 12v10M7.5 4.5l9 5"/>',

    truck: '<path d="M3 6h12v11H3zM15 10h4l3 4v3h-7M7 20a2 2 0 1 0 0-4 2 2 0 0 0 4 0Zm12 0a2 2 0 1 0 0-4 2 2 0 0 0 0 4Z"/>',

    cart: '<circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M2 3h2l3 14h14l2-10H5"/>',

    chart: '<path d="M3 21h18M5 18V9h4v9M11 18V3h4v15M17 18v-7h4v7"/>',

    users: '<circle cx="9" cy="8" r="3"/><path d="M2 21v-2a7 7 0 0 1 14 0v2M17 5a3 3 0 0 1 0 6M18 15a5 5 0 0 1 4 5"/>',

    user: '<circle cx="12" cy="7" r="4"/><path d="M4 22v-3a8 8 0 0 1 16 0v3z"/>',

    shield: '<path d="m12 2 9 4v6c0 6-4 9-9 11-5-2-9-5-9-11V6zM8 12l3 3 5-6"/>',

    settings: '<circle cx="12" cy="12" r="3"/><path d="M10 2h4l1 3 3 1 3-1 2 4-2 2v3l2 2-2 4-3-1-3 1-1 3h-4l-1-3-3-1-3 1-2-4 2-2v-3L1 9l2-4 3 1 3-1z"/>',

    search: '<circle cx="11" cy="11" r="7"/><path d="m16 16 5 5"/>',

    arrow: '<path d="M4 12h16m-7-7 7 7-7 7"/>',

    chevron: '<path d="m6 9 6 6 6-6"/>',

    menu: '<path d="M4 6h16M4 12h16M4 18h16"/>',

    'arrow-left': '<path d="m12 19-7-7 7-7M5 12h15"/>'

};


/* Crear icono SVG */

function crearIcono(nombre) {

    const contenido = icons[nombre] || icons.box;

    return `
        <svg
            viewBox="0 0 24 24"
            width="24"
            height="24"
            fill="none"
            stroke="currentColor"
            stroke-width="1.9"
            stroke-linecap="round"
            stroke-linejoin="round"
            aria-hidden="true"
        >
            ${contenido}
        </svg>
    `;
}


/* Colocar los iconos en el Dashboard */

document.querySelectorAll('[data-icon]').forEach(elemento => {

    elemento.innerHTML = crearIcono(elemento.dataset.icon);

});


/* ============================================================
   2. DATOS DEL DASHBOARD
   SIN REGISTROS FICTICIOS
   ============================================================ */

const sales = [];

const stock = [];


/* Formato de dólares */

const money = numero => {

    return new Intl.NumberFormat('en-US', {

        style: 'currency',
        currency: 'USD'

    }).format(numero);

};


/* Evitar HTML no deseado */

function esc(valor) {

    return String(valor ?? '').replace(
        /[&<>"']/g,
        caracter => ({
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;'
        }[caracter])
    );

}


/* ============================================================
   3. TABLAS DEL DASHBOARD
   ============================================================ */

function cargarTablasDashboard() {

    const tablaVentas = document.getElementById('salesRows');

    const tablaInventario = document.getElementById('stockRows');


    /* Ventas recientes */

    if (tablaVentas) {

        tablaVentas.innerHTML = sales.length === 0

            ? `
                <tr>
                    <td
                        colspan="5"
                        style="
                            text-align:center;
                            padding:30px;
                            color:#a9aab3;
                        "
                    >
                        No hay ventas registradas.
                    </td>
                </tr>
            `

            : sales.map(([factura, cliente, fecha, total, estado]) => `
                <tr>
                    <td>${esc(factura)}</td>
                    <td>${esc(cliente)}</td>
                    <td>${esc(fecha)}</td>
                    <td>${money(total)}</td>
                    <td>
                        <span class="status ${estado === 'En proceso' ? 'pending' : ''}">
                            ${esc(estado)}
                        </span>
                    </td>
                </tr>
            `).join('');

    }


    /* Productos con bajo stock */

    if (tablaInventario) {

        tablaInventario.innerHTML = stock.length === 0

            ? `
                <tr>
                    <td
                        colspan="4"
                        style="
                            text-align:center;
                            padding:30px;
                            color:#a9aab3;
                        "
                    >
                        No hay productos con bajo stock.
                    </td>
                </tr>
            `

            : stock.map(([emoji, nombre, detalle, sku, actual, minimo]) => `
                <tr>
                    <td>
                        <div class="product-info">
                            <span class="product-symbol">${esc(emoji)}</span>
                            <span>
                                <strong>${esc(nombre)}</strong>
                                <small>${esc(detalle)}</small>
                            </span>
                        </div>
                    </td>
                    <td>${esc(sku)}</td>
                    <td class="stock-low">${esc(actual)}</td>
                    <td>${esc(minimo)}</td>
                </tr>
            `).join('');

    }

}


/* Cargar las tablas inicialmente vacías */

cargarTablasDashboard();


/* ============================================================
   4. COMUNICACIÓN CON WINDOWS FORMS
   ============================================================ */

function enviarACSharp(mensaje) {

    if (window.chrome?.webview?.postMessage) {

        window.chrome.webview.postMessage(mensaje);

        return true;

    }

    toast('Esta función necesita ejecutarse desde AutoParts.');

    return false;

}


/* ============================================================
   5. ELEMENTOS DEL DASHBOARD
   ============================================================ */

const dashboard = document.getElementById('dashboardContent');

const sectionView = document.getElementById('sectionView');

const sectionTitle = document.getElementById('sectionTitle');

const sectionDescription = document.getElementById('sectionDescription');

const sectionBody = document.getElementById('sectionBody');


/* Descripciones de los módulos */

const moduleDescriptions = {

    Productos: 'Administración del catálogo de repuestos y sus precios.',

    Inventario: 'Control de existencias y niveles mínimos.',

    Ventas: 'Historial de ventas y facturas.',

    Categorías: 'Administración de categorías de repuestos.',

    Marcas: 'Marcas registradas en el catálogo.',

    Proveedores: 'Directorio de proveedores de repuestos.',

    Compras: 'Registro de compras a proveedores.',

    Clientes: 'Directorio de clientes.',

    Empleados: 'Gestión de empleados.',

    Garantías: 'Solicitudes y reemplazos por garantía.',

    Usuarios: 'Accesos y permisos del sistema.',

    Configuración: 'Opciones de AutoParts.'

};


/* ============================================================
   6. ABRIR FORMULARIOS WINDOWS FORMS
   ============================================================ */

/*
   MainForm.cs recibe este mensaje:

   type: "categorias:abrirLista"

   Y abre:

   frmCategorias

   Luego, dentro de frmCategorias,
   el botón Nueva categoría abre frmCategoriasNuevo.
*/

function abrirFormularioCategorias() {

    enviarACSharp({

        type: 'categorias:abrirLista'

    });

}


/* ============================================================
   7. NAVEGACIÓN PRINCIPAL
   ============================================================ */

function showSection(section) {

    const key = section || 'Dashboard';


    /* CATEGORÍAS SE ABRE EN WINDOWS FORMS */

    if (key === 'Categorías') {

        abrirFormularioCategorias();

        return;

    }


    /* RESTO DE NAVEGACIÓN DEL DASHBOARD */

    const esDashboard = key === 'Dashboard';

    dashboard.hidden = !esDashboard;

    sectionView.hidden = esDashboard;


    /* Actualizar enlace seleccionado */

    document.querySelectorAll('.nav-link').forEach(enlace => {

        enlace.classList.toggle(
            'active',
            enlace.dataset.section === key
        );

    });


    /* Cerrar menú lateral móvil */

    document.getElementById('sidebar').classList.remove('open');


    if (!esDashboard) {

        sectionTitle.textContent = key;

        sectionDescription.textContent =
            moduleDescriptions[key] || 'Módulo de AutoParts';


        sectionBody.innerHTML = `
            <div class="section-placeholder">

                <h3>Módulo ${esc(key)}</h3>

                <p>
                    Este módulo se implementará
                    próximamente con Windows Forms,
                    BusinessLogic y SQL Server.
                </p>

            </div>
        `;

    }


    /* Actualizar dirección interna */

    window.location.hash = esDashboard
        ? 'dashboard'
        : key
            .toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '');

}


/* ============================================================
   8. EVENTOS DEL MENÚ LATERAL
   ============================================================ */

document.querySelectorAll('.nav-link').forEach(enlace => {

    enlace.addEventListener('click', evento => {

        evento.preventDefault();

        const modulo = enlace.dataset.section;

        showSection(modulo);

    });

});


/* ============================================================
   9. ACCESOS RÁPIDOS
   ============================================================ */

document.querySelectorAll('[data-open]').forEach(boton => {

    boton.addEventListener('click', () => {

        showSection(boton.dataset.open);

    });

});


/* ============================================================
   10. REGRESAR AL DASHBOARD
   ============================================================ */

document.getElementById('backButton').onclick = () => {

    showSection('Dashboard');

};


/* ============================================================
   11. MENÚ LATERAL RESPONSIVE
   ============================================================ */

document.getElementById('menuToggle').onclick = () => {

    document.getElementById('sidebar')
        .classList.toggle('open');

};


/* ============================================================
   12. PERFIL DE USUARIO
   ============================================================ */

const profileButton = document.getElementById('profileButton');

const profileMenu = document.getElementById('profileMenu');


profileButton.onclick = () => {

    profileMenu.hidden = !profileMenu.hidden;

    profileButton.setAttribute(
        'aria-expanded',
        String(!profileMenu.hidden)
    );

};


/* Abrir configuración */

document.getElementById('profileSettings').onclick = () => {

    profileMenu.hidden = true;

    showSection('Configuración');

};


/* Cerrar sesión */

document.getElementById('logoutButton').onclick = () => {

    toast('El cierre de sesión se conectará con Windows Forms.');

};


/* Cerrar perfil al hacer clic fuera */

document.addEventListener('click', evento => {

    if (!evento.target.closest('.profile-wrap')) {

        profileMenu.hidden = true;

    }

});


/* ============================================================
   13. MENSAJES EMERGENTES
   ============================================================ */

let toastTimer;


function toast(mensaje) {

    const elemento = document.getElementById('toast');

    if (!elemento) return;

    elemento.textContent = mensaje;

    elemento.classList.add('visible');

    clearTimeout(toastTimer);

    toastTimer = setTimeout(() => {

        elemento.classList.remove('visible');

    }, 3200);

}


/* ============================================================
   14. BUSCADOR GENERAL
   ============================================================ */

const search = document.getElementById('searchInput');

const results = document.getElementById('searchResults');


search.addEventListener('input', () => {

    const texto = search.value
        .toLocaleLowerCase('es')
        .trim();


    if (!texto) {

        results.hidden = true;

        return;

    }


    /* Buscar módulos disponibles */

    const opciones = Object.keys(moduleDescriptions).map(nombre => ({

        title: nombre,

        sub: 'Abrir módulo',

        section: nombre

    }));


    const coincidencias = opciones.filter(opcion =>

        opcion.title
            .toLocaleLowerCase('es')
            .includes(texto)

    ).slice(0, 8);


    if (coincidencias.length === 0) {

        results.innerHTML = `
            <p>No se encontraron coincidencias.</p>
        `;

    } else {

        results.innerHTML = coincidencias.map(
            (opcion, indice) => `

                <button
                    type="button"
                    data-result="${indice}"
                >

                    ${esc(opcion.title)}

                    <small>${esc(opcion.sub)}</small>

                </button>

            `
        ).join('');

    }


    results.hidden = false;


    /* Abrir módulo seleccionado */

    results.querySelectorAll('[data-result]').forEach(boton => {

        boton.onclick = () => {

            const indice = Number(boton.dataset.result);

            showSection(
                coincidencias[indice].section
            );

            search.value = '';

            results.hidden = true;

        };

    });

});


/* ============================================================
   15. ATAJOS DEL TECLADO
   ============================================================ */

document.addEventListener('keydown', evento => {

    /* Ctrl + K: buscar */

    if (
        (evento.ctrlKey || evento.metaKey) &&
        evento.key.toLowerCase() === 'k'
    ) {

        evento.preventDefault();

        search.focus();

    }


    /* Escape: cerrar menús */

    if (evento.key === 'Escape') {

        results.hidden = true;

        profileMenu.hidden = true;

        document.getElementById('sidebar').classList.remove('open');

    }

});


/* ============================================================
   FIN DE APP.JS
   ============================================================ */
