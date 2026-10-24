function abrirModuloEnDashboard(modulo, titulo, descripcion, archivo) {
    const dashboard = document.getElementById('dashboardContent');
    const vista = document.getElementById('sectionView');
    const contenido = document.getElementById('sectionBody');
    const encabezado = document.getElementById('sectionHeading');

    if (!dashboard || !vista || !contenido) return;

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

function abrirCategoriasEnDashboard() {
    abrirModuloEnDashboard('categorias', 'Categorías', 'Administración de categorías de repuestos.', 'modulos/categorias.html');
}

function abrirClientesEnDashboard() {
    abrirModuloEnDashboard('clientes', 'Clientes', 'Administración de clientes registrados.', 'modulos/clientes.html');
}

function abrirEstadosEnDashboard() {
    abrirModuloEnDashboard('estados', 'Estados', 'Consulta de estados registrados.', 'modulos/estados.html');
}

function abrirMarcasEnDashboard() {
    abrirModuloEnDashboard('marcas', 'Marcas', 'Administración de marcas registradas.', 'modulos/marcas.html');
}

function abrirProveedoresEnDashboard() {
    abrirModuloEnDashboard('proveedores', 'Proveedores', 'Administración de proveedores registrados.', 'modulos/proveedores.html');
}

function abrirPuestosEnDashboard() {
    abrirModuloEnDashboard('puestos', 'Puestos', 'Administración de puestos de trabajo.', 'modulos/puestos.html');
}

function abrirRolesEnDashboard() {
    abrirModuloEnDashboard('roles', 'Roles', 'Administración de roles de usuario.', 'modulos/roles.html');
}

function obtenerMarcoIntegrado() {
    return document.getElementById('moduloIntegrado');
}

function restaurarEncabezado() {
    const encabezado = document.getElementById('sectionHeading');
    if (encabezado) encabezado.hidden = false;
}

const botonVolver = document.getElementById('backButton');
if (botonVolver) {
    botonVolver.addEventListener('click', () => {
        restaurarEncabezado();
        const contenido = document.getElementById('sectionBody');
        if (contenido) contenido.innerHTML = '';
    });
}

const modulosIntegrados = ['Categorías', 'Clientes', 'Estados', 'Marcas', 'Proveedores', 'Puestos', 'Roles'];

document.querySelectorAll('.nav-link').forEach(enlace => {
    enlace.addEventListener('click', () => {
        if (!modulosIntegrados.includes(enlace.dataset.section)) restaurarEncabezado();
    });
});

document.querySelectorAll('[data-open]').forEach(boton => {
    boton.addEventListener('click', () => {
        if (!modulosIntegrados.includes(boton.dataset.open)) restaurarEncabezado();
    });
});

window.addEventListener('message', evento => {
    const marco = obtenerMarcoIntegrado();
    if (!marco || evento.source !== marco.contentWindow) return;

    const mensaje = evento.data;
    if (!mensaje || typeof mensaje !== 'object') return;

    const modulo = marco.dataset.modulo;

    if (modulo === 'categorias') {
        if (mensaje.type === 'categorias:abrirNuevo') {
            marco.src = 'modulos/categorias-nuevo.html';
        } else if (mensaje.type === 'cancelar') {
            marco.src = 'modulos/categorias.html';
        } else if (mensaje.type === 'guardar') {
            enviarACSharp({ type: 'categorias:guardarIntegrado', data: mensaje.data });
        } else if (mensaje.type === 'categorias:selectAll' || mensaje.type === 'obtenerEstados') {
            enviarACSharp(mensaje);
        }
    }

    if (modulo === 'clientes') {
        if (mensaje.type === 'clientes:abrirNuevo') {
            marco.src = 'modulos/clientes-nuevo.html';
        } else if (mensaje.type === 'cancelar') {
            marco.src = 'modulos/clientes.html';
        } else if (mensaje.type === 'guardar') {
            enviarACSharp({ type: 'clientes:guardarIntegrado', data: mensaje.data });
        } else if (mensaje.type === 'clientes:selectAll') {
            enviarACSharp(mensaje);
        }
    }

    if (modulo === 'estados' && mensaje.type === 'estados:selectAll') {
        enviarACSharp(mensaje);
    }

    if (modulo === 'marcas') {
        if (mensaje.type === 'marcas:abrirNuevo') {
            marco.src = 'modulos/marcas-nuevo.html';
        } else if (mensaje.type === 'cancelar') {
            marco.src = 'modulos/marcas.html';
        } else if (mensaje.type === 'guardar') {
            enviarACSharp({ type: 'marcas:guardarIntegrado', data: mensaje.data });
        } else if (mensaje.type === 'marcas:selectAll' || mensaje.type === 'obtenerEstados') {
            enviarACSharp(mensaje);
        }
    }

    if (modulo === 'proveedores') {
        if (mensaje.type === 'proveedores:abrirNuevo') {
            marco.src = 'modulos/proveedores-nuevo.html';
        } else if (mensaje.type === 'cancelar') {
            marco.src = 'modulos/proveedores.html';
        } else if (mensaje.type === 'guardar') {
            enviarACSharp({ type: 'proveedores:guardarIntegrado', data: mensaje.data });
        } else if (mensaje.type === 'proveedores:selectAll') {
            enviarACSharp(mensaje);
        }
    }

    if (modulo === 'puestos') {
        if (mensaje.type === 'puestos:abrirNuevo') {
            marco.src = 'modulos/puestos-nuevo.html';
        } else if (mensaje.type === 'cancelar') {
            marco.src = 'modulos/puestos.html';
        } else if (mensaje.type === 'guardar') {
            enviarACSharp({ type: 'puestos:guardarIntegrado', data: mensaje.data });
        } else if (mensaje.type === 'puestos:selectAll' || mensaje.type === 'obtenerEstados') {
            enviarACSharp(mensaje);
        }
    }

    if (modulo === 'roles') {
        if (mensaje.type === 'roles:abrirNuevo') {
            marco.src = 'modulos/roles-nuevo.html';
        } else if (mensaje.type === 'cancelar') {
            marco.src = 'modulos/roles.html';
        } else if (mensaje.type === 'guardar') {
            enviarACSharp({ type: 'roles:guardarIntegrado', data: mensaje.data });
        } else if (mensaje.type === 'roles:selectAll') {
            enviarACSharp(mensaje);
        }
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

        if (!mensaje || typeof mensaje !== 'object') return;

        const marco = obtenerMarcoIntegrado();
        if (!marco) return;
        const modulo = marco.dataset.modulo;

        if (modulo === 'categorias') {
            if (mensaje.type === 'categorias:guardado') {
                marco.src = 'modulos/categorias.html';
            } else if (mensaje.type === 'categorias:list' || mensaje.type === 'estados' || mensaje.type === 'error') {
                marco.contentWindow?.postMessage(mensaje, '*');
            }
        }

        if (modulo === 'clientes') {
            if (mensaje.type === 'clientes:guardado') {
                marco.src = 'modulos/clientes.html';
            } else if (mensaje.type === 'clientes:list' || mensaje.type === 'error') {
                marco.contentWindow?.postMessage(mensaje, '*');
            }
        }

        if (modulo === 'estados') {
            if (mensaje.type === 'estados:list' || mensaje.type === 'error') {
                marco.contentWindow?.postMessage(mensaje, '*');
            }
        }

        if (modulo === 'marcas') {
            if (mensaje.type === 'marcas:guardado') {
                marco.src = 'modulos/marcas.html';
            } else if (mensaje.type === 'marcas:list' || mensaje.type === 'estados' || mensaje.type === 'error') {
                marco.contentWindow?.postMessage(mensaje, '*');
            }
        }

        if (modulo === 'proveedores') {
            if (mensaje.type === 'proveedores:guardado') {
                marco.src = 'modulos/proveedores.html';
            } else if (mensaje.type === 'proveedores:list' || mensaje.type === 'error') {
                marco.contentWindow?.postMessage(mensaje, '*');
            }
        }

        if (modulo === 'puestos') {
            if (mensaje.type === 'puestos:guardado') {
                marco.src = 'modulos/puestos.html';
            } else if (mensaje.type === 'puestos:list' || mensaje.type === 'estados' || mensaje.type === 'error') {
                marco.contentWindow?.postMessage(mensaje, '*');
            }
        }

        if (modulo === 'roles') {
            if (mensaje.type === 'roles:guardado') {
                marco.src = 'modulos/roles.html';
            } else if (mensaje.type === 'roles:list' || mensaje.type === 'error') {
                marco.contentWindow?.postMessage(mensaje, '*');
            }
        }
    });
}