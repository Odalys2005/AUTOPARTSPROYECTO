using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoParts.Entity;
using AutoParts.Entity.Productos;
using AutoParts.DataAccess;

namespace AutoParts.View
{
    public partial class MainForm : Form
    {
        private readonly CategoriaBL _categoriaBL;
        private readonly SubcategoriaBL _subcategoriaBL;
        private readonly EstadoBL _estadoBL;
        private readonly RolBL _rolBL;
        private readonly ProveedorBL _proveedorBL;
        private readonly ClienteBL _clienteBL;
        private readonly MarcaBL _marcaBL;
        private readonly PuestoBL _puestoBL;
        private readonly ProductoBL _productoBL;
        private readonly InventarioBL _inventarioBL;
        private readonly ModeloBL _modeloBL;
        private readonly VehiculoBL _vehiculoBL;
        private readonly CompatibilidadRepuestoBL _compatibilidadBL;
        private readonly CompraBL _compraBL;
        private readonly Detalle_CompraBL _detalleCompraBL;
        private readonly VentaBL _ventaBL;
        private readonly Detalle_VentaBL _detalleVentaBL;
        private readonly GarantiaBL _garantiaBL;
        private readonly ReemplazoGarantiaBL _reemplazoGarantiaBL;
        private readonly EmpleadoBL _empleadoBL;
        private readonly UsuarioBL _usuarioBL;
        private SesionUsuario? _sesionActual;
        private bool _loginEnProceso;

        private const string HostImagenes = "autoparts-imagenes.local";
        private const int MaxImagenBytes = 5 * 1024 * 1024;
        private static readonly string CarpetaImagenes = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutoParts", "Productos");


        public MainForm(
            CategoriaBL categoriaBL,
            SubcategoriaBL subcategoriaBL,
            EstadoBL estadoBL,
            RolBL rolBL,
            ProveedorBL proveedorBL,
            ClienteBL clienteBL,
            MarcaBL marcaBL,
            PuestoBL puestoBL,
            ProductoBL productoBL,
            InventarioBL inventarioBL,
            ModeloBL modeloBL,
            VehiculoBL vehiculoBL,
            CompatibilidadRepuestoBL compatibilidadBL,
            CompraBL compraBL,
            Detalle_CompraBL detalleCompraBL,
            VentaBL ventaBL,
            Detalle_VentaBL detalleVentaBL,
            GarantiaBL garantiaBL,
            ReemplazoGarantiaBL reemplazoGarantiaBL,
            EmpleadoBL empleadoBL,
            UsuarioBL usuarioBL)
        {
            InitializeComponent();

            _categoriaBL = categoriaBL;
            _subcategoriaBL = subcategoriaBL;
            _estadoBL = estadoBL;
            _rolBL = rolBL;
            _proveedorBL = proveedorBL;
            _clienteBL = clienteBL;
            _marcaBL = marcaBL;
            _puestoBL = puestoBL;
            _productoBL = productoBL;
            _inventarioBL = inventarioBL;
            _modeloBL = modeloBL;
            _vehiculoBL = vehiculoBL;
            _compatibilidadBL = compatibilidadBL;
            _compraBL = compraBL;
            _detalleCompraBL = detalleCompraBL;
            _ventaBL = ventaBL;
            _detalleVentaBL = detalleVentaBL;
            _garantiaBL = garantiaBL;
            _reemplazoGarantiaBL = reemplazoGarantiaBL;
            _empleadoBL = empleadoBL;
            _usuarioBL = usuarioBL;

            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            webViewMain.Dock = DockStyle.Fill;

            Load += MainForm_Load;
        }

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            try
            {
                await webViewMain.EnsureCoreWebView2Async();

                Directory.CreateDirectory(CarpetaImagenes);
                webViewMain.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    HostImagenes,
                    CarpetaImagenes,
                    CoreWebView2HostResourceAccessKind.DenyCors);
                webViewMain.CoreWebView2.WebMessageReceived += WebViewMain_WebMessageReceived;
                webViewMain.CoreWebView2.NavigationStarting += (s, args) =>
                {

                    if (!Uri.TryCreate(args.Uri, UriKind.Absolute, out Uri? destino)
                        || !destino.IsFile
                        || (!RutaCoincide(destino, "login.html")
                            && !RutaCoincide(destino, "index.html"))
                        || (_sesionActual == null && RutaCoincide(destino, "index.html")))
                    {
                        args.Cancel = true;
                    }
                };
                webViewMain.CoreWebView2.NavigationCompleted += (s, args) =>
                {
                    if (args.IsSuccess && _sesionActual != null && PaginaActualEs("index.html"))
                        EnviarSesion();
                };
                NavegarPagina("login.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo iniciar el login:\n" + ex.Message,
                    "AutoParts", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool RutaCoincide(Uri uri, string archivo)
        {
            if (!uri.IsFile) return false;
            string esperado = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "Web", archivo));
            return string.Equals(Path.GetFullPath(uri.LocalPath), esperado,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool PaginaActualEs(string archivo)
        {
            Uri? actual = webViewMain.Source;
            return actual != null && RutaCoincide(actual, archivo);
        }

        private void NavegarPagina(string archivo)
        {
            string ruta = Path.Combine(AppContext.BaseDirectory, "Web", archivo);
            if (!File.Exists(ruta))
                throw new FileNotFoundException("No se encontró la página de AutoParts", ruta);
            webViewMain.Source = new Uri(Path.GetFullPath(ruta));
        }

        private void EnviarSesion()
        {
            if (_sesionActual == null) return;
            EnviarAlDashboard(new
            {
                type = "auth:sesion",
                data = new
                {
                    _sesionActual.UsuarioId,
                    _sesionActual.EmpleadoId,
                    _sesionActual.RolId,
                    _sesionActual.NombreUsuario
                }
            });
        }

        private async Task ProcesarLoginAsync(JsonElement raiz)
        {
            if (_loginEnProceso) return;
            _loginEnProceso = true;
            try
            {
                if (!raiz.TryGetProperty("data", out JsonElement datos)
                    || datos.ValueKind != JsonValueKind.Object)
                    throw new ArgumentException("Debes ingresar usuario y contraseña.");

                string nombre = datos.TryGetProperty("usuario", out var campoUsuario)
                    && campoUsuario.ValueKind == JsonValueKind.String
                    ? (campoUsuario.GetString() ?? "").Trim() : "";
                string clave = datos.TryGetProperty("contrasena", out var campoClave)
                    && campoClave.ValueKind == JsonValueKind.String
                    ? campoClave.GetString() ?? "" : "";

                if (nombre.Length == 0 || nombre.Length > 50 || clave.Length == 0)
                {
                    EnviarAlDashboard(new
                    {
                        type = "auth:error",
                        message = "Ingresa un usuario y una contraseña válidos."
                    });
                    return;
                }

                SesionUsuario? sesion = await Task.Run(() =>
                    _usuarioBL.IniciarSesion(nombre, clave));

                if (sesion == null)
                {
                    EnviarAlDashboard(new
                    {
                        type = "auth:error",
                        message = "Usuario o contraseña incorrectos, o cuenta inactiva."
                    });
                    return;
                }

                _sesionActual = sesion;
                NavegarPagina("index.html");
            }
            catch (Exception)
            {
                EnviarAlDashboard(new
                {
                    type = "auth:error",
                    message = "No fue posible validar el acceso. Revisa la conexión con la base de datos."
                });
            }
            finally
            {
                _loginEnProceso = false;
            }
        }

        private async void WebViewMain_WebMessageReceived(
            object? sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using JsonDocument documento =
                    JsonDocument.Parse(e.WebMessageAsJson);

                JsonElement raiz = documento.RootElement;

                string tipo = raiz
                    .GetProperty("type")
                    .GetString() ?? "";


                if (_sesionActual == null)
                {
                    if (tipo == "auth:login" && PaginaActualEs("login.html"))
                        await ProcesarLoginAsync(raiz);
                    return;
                }


                if (!PaginaActualEs("index.html")) return;
                if (tipo == "auth:logout")
                {
                    _sesionActual = null;
                    NavegarPagina("login.html");
                    return;
                }
                SesionUsuario? sesionVigente;
                try
                {
                    sesionVigente = _usuarioBL.ValidarSesion(_sesionActual.UsuarioId);
                }
                catch (Exception)
                {
                    EnviarAlDashboard(new
                    {
                        type = "error",
                        message = "No fue posible comprobar la sesión. Revisa la conexión con la base de datos."
                    });
                    return;
                }

                if (sesionVigente == null ||
                    sesionVigente.UsuarioId != _sesionActual.UsuarioId ||
                    sesionVigente.EmpleadoId != _sesionActual.EmpleadoId ||
                    sesionVigente.RolId != _sesionActual.RolId)
                {
                    _sesionActual = null;
                    NavegarPagina("login.html");
                    return;
                }

                _sesionActual = sesionVigente;

                if (tipo == "auth:sesionSolicitar")
                {
                    EnviarSesion();
                    return;
                }
                if (tipo == "auth:login") return;

                if (!PermisosSistema.EsAdministrador(_sesionActual) &&
                    !AutorizarMensajePorRol(tipo))
                {
                    EnviarAlDashboard(new
                    {
                        type = "error",
                        message = "No tienes permisos para realizar esta operación."
                    });
                    return;
                }

                if (!PermisosSistema.EsAdministrador(_sesionActual) &&
                    tipo == "empleados:selectAll")
                {
                    try
                    {
                        var empleadosConsulta = _empleadoBL.SelectAll()
                            .Select(empleado => new
                            {
                                empleado.EmpleadoId,
                                empleado.Nombre,
                                empleado.Apellido,
                                empleado.EstadoId
                            })
                            .ToList();
                        EnviarAlDashboard(new { type = "empleados:list", data = empleadosConsulta });
                    }
                    catch (Exception ex)
                    {
                        EnviarAlDashboard(new { type = "error", message = ex.Message });
                    }
                    return;
                }


                if (ProcesarCrudMaestros(tipo, raiz))
                    return;

                switch (tipo)
                {
                    case "categorias:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirCategorias();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Categorías:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "estados:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirEstados();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Estados:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "roles:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirRoles();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Roles:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "proveedores:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirProveedores();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Proveedores:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "clientes:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirClientes();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Clientes:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "marcas:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirMarcas();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Marcas:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "puestos:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirPuestos();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Puestos:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;


                    case "dashboard:solicitarResumen":
                        try
                        {
                            var resumen = await Task.Run(() => ObtenerResumenDashboard());
                            EnviarAlDashboard(new
                            {
                                type = "dashboard:resumen",
                                data = resumen
                            });
                        }
                        catch (Exception)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "dashboard:error",
                                message = "No se pudieron cargar las ventas recientes o el inventario. Revisa la conexión con SQL Server."
                            });
                        }
                        break;


                    case "compras:registrarCompleta":
                        try
                        {
                            if (!raiz.TryGetProperty("data", out JsonElement datosCompra) ||
                                datosCompra.ValueKind != JsonValueKind.Object)
                                throw new ArgumentException("No se recibieron datos validos de la compra.");

                            if (!datosCompra.TryGetProperty("Compra", out JsonElement cabeceraCompra) ||
                                cabeceraCompra.ValueKind != JsonValueKind.Object)
                                throw new ArgumentException("Falta la cabecera de la compra.");
                            if (!datosCompra.TryGetProperty("Detalles", out JsonElement lineasCompra) ||
                                lineasCompra.ValueKind != JsonValueKind.Array)
                                throw new ArgumentException("Faltan los productos de la compra.");

                            var opcionesCompra = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };
                            Compra compraNueva = cabeceraCompra.Deserialize<Compra>(opcionesCompra)
                                ?? throw new ArgumentException("La cabecera de compra no es valida.");
                            List<Detalle_Compra> detallesNuevos =
                                lineasCompra.Deserialize<List<Detalle_Compra>>(opcionesCompra)
                                ?? throw new ArgumentException("Los detalles de compra no son validos.");


                            CompraRegistroResultado resultadoCompra =
                                _compraBL.RegistrarCompleta(compraNueva, detallesNuevos);
                            EnviarAlDashboard(new
                            {
                                type = "compras:registrada",
                                data = new
                                {
                                    resultadoCompra.CompraId,
                                    resultadoCompra.NumeroComprobante,
                                    resultadoCompra.TotalCompra,
                                    resultadoCompra.AdvertenciasStockMaximo
                                }
                            });
                        }
                        catch (Exception ex)
                        {


                            EnviarAlDashboard(new
                            {
                                type = "compras:error",
                                message = "Compra no registrada: " + ex.Message
                            });
                        }
                        break;


                    case "ventas:registrarCompleta":
                        try
                        {
                            if (!raiz.TryGetProperty("data", out JsonElement datosVenta) ||
                                datosVenta.ValueKind != JsonValueKind.Object)
                                throw new ArgumentException("No se recibieron datos válidos de la venta.");

                            VentaRegistroCompleto solicitudVenta =
                                datosVenta.Deserialize<VentaRegistroCompleto>(
                                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                ?? throw new ArgumentException("La venta recibida no es válida.");

                            VentaRegistroResultado resultadoVenta = _ventaBL.RegistrarCompleta(solicitudVenta);
                            EnviarAlDashboard(new
                            {
                                type = "ventas:registrada",
                                data = new { resultadoVenta.VentaId, resultadoVenta.NumeroFactura, resultadoVenta.Total }
                            });
                            await ProcesarCorreoFacturaNuevaAsync(resultadoVenta.VentaId);
                        }
                        catch (Exception ex)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Venta no registrada: " + ex.Message
                            });
                        }
                        break;


                    case "ventas:generarPdf":
                        try
                        {
                            if (!raiz.TryGetProperty("data", out JsonElement datosPdf) ||
                                datosPdf.ValueKind != JsonValueKind.Object ||
                                !datosPdf.TryGetProperty("VentaId", out JsonElement idPdfJson) ||
                                !idPdfJson.TryGetInt32(out int idVentaPdf) || idVentaPdf <= 0)
                                throw new ArgumentException("Selecciona una venta registrada válida.");

                            var servicioPdf = new FacturaPdfService(
                                _ventaBL, _detalleVentaBL, _productoBL, _clienteBL, _empleadoBL);
                            string rutaPdf = await Task.Run(() => servicioPdf.Generar(idVentaPdf));
                            string? enlaceDigital = null;
                            string mensajePdf = "Factura PDF generada correctamente.";
                            try
                            {
                                var ventaPdf = _ventaBL.SelectAll()
                                    .FirstOrDefault(v => v.VentaId == idVentaPdf)
                                    ?? throw new InvalidOperationException("No se encontró la venta para publicar.");
                                enlaceDigital = await ObtenerOPublicarFacturaDigitalAsync(
                                    idVentaPdf, rutaPdf, ventaPdf.NumeroFactura ?? "");
                                mensajePdf = "Factura PDF generada y publicada correctamente en Supabase.";
                            }
                            catch (Exception errorPublicacion)
                            {
                                mensajePdf = "PDF guardado localmente. No se pudo publicar en Supabase: "
                                    + errorPublicacion.Message;
                            }
                            EnviarAlDashboard(new
                            {
                                type = "ventas:pdfGenerado",
                                data = new { VentaId = idVentaPdf, RutaPdf = rutaPdf, EnlaceDigital = enlaceDigital },
                                message = mensajePdf
                            });
                        }
                        catch (Exception ex)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "ventas:pdfError",
                                message = "No se pudo generar el PDF: " + ex.Message
                            });
                        }
                        break;

                    case "ventas:imprimirTicket":
                        try
                        {
                            if (!raiz.TryGetProperty("data", out JsonElement datosTicket) ||
                                datosTicket.ValueKind != JsonValueKind.Object ||
                                !datosTicket.TryGetProperty("VentaId", out JsonElement idTicketJson) ||
                                !idTicketJson.TryGetInt32(out int idVentaTicket) || idVentaTicket <= 0)
                                throw new ArgumentException("Selecciona una venta registrada válida.");


                            await Task.Run(() => ImprimirTicketVentaPt210(idVentaTicket));
                            EnviarAlDashboard(new
                            {
                                type = "ventas:ticketImpreso",
                                message = "Ticket enviado a la cola de impresión AutoParts PT-210."
                            });
                        }
                        catch (Exception ex)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "ventas:ticketError",
                                message = "No se pudo enviar el ticket: " + ex.Message
                            });
                        }
                        break;


                    case "garantias:buscarPorFactura":
                        try
                        {
                            if (!raiz.TryGetProperty("data", out JsonElement datosBusqueda) ||
                                datosBusqueda.ValueKind != JsonValueKind.Object ||
                                !datosBusqueda.TryGetProperty("NumeroFactura", out JsonElement facturaJson) ||
                                facturaJson.ValueKind != JsonValueKind.String)
                                throw new ArgumentException("Escanea o ingresa un número de factura válido.");

                            string numeroFactura = (facturaJson.GetString() ?? string.Empty).Trim();
                            int? ventaIdEncontrada = await Task.Run(() =>
                                _ventaBL.BuscarVentaIdPorFactura(numeroFactura));

                            if (!ventaIdEncontrada.HasValue)
                            {
                                EnviarAlDashboard(new
                                {
                                    type = "garantias:ventaNoEncontrada",
                                    message = "No se encontró ninguna venta con ese número de factura."
                                });
                                break;
                            }

                            var ventaEncontrada = _ventaBL.SelectAll()
                                .FirstOrDefault(v => v.VentaId == ventaIdEncontrada.Value)
                                ?? throw new InvalidOperationException("No fue posible recuperar la venta encontrada.");
                            var detallesVenta = _detalleVentaBL.SelectAll()
                                .Where(d => d.VentaId == ventaEncontrada.VentaId)
                                .ToList();
                            var productosVenta = _productoBL.SelectAll()
                                .ToDictionary(p => p.ProductoId);
                            var clienteVenta = _clienteBL.SelectAll()
                                .FirstOrDefault(c => c.ClienteId == ventaEncontrada.ClienteId);

                            EnviarAlDashboard(new
                            {
                                type = "garantias:ventaEncontrada",
                                data = new
                                {
                                    ventaEncontrada.VentaId,
                                    ventaEncontrada.NumeroFactura,
                                    ventaEncontrada.FechaVenta,
                                    ventaEncontrada.ClienteId,
                                    ClienteNombre = clienteVenta == null
                                        ? "Cliente no identificado"
                                        : (clienteVenta.Nombre + " " + clienteVenta.Apellido).Trim(),
                                    ventaEncontrada.Total,
                                    Detalles = detallesVenta.Select(d => new
                                    {
                                        d.DetalleVentaId,
                                        d.ProductoId,
                                        ProductoNombre = productosVenta.TryGetValue(d.ProductoId, out var producto)
                                            ? producto.Nombre
                                            : "Producto #" + d.ProductoId,
                                        d.Cantidad,
                                        d.PrecioUnitario,
                                        d.Subtotal,
                                        d.DiasGarantiaVenta
                                    }).ToList()
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "garantias:busquedaError",
                                message = ex.Message
                            });
                        }
                        break;

                    case "garantias:registrarSolicitud":
                    case "garantias:evaluar":
                    case "reemplazos-garantia:entregar":
                        ProcesarOperacionGarantia(tipo, raiz);
                        break;


                    case "productos:selectAll":
                    case "productos:guardarIntegrado":
                        ProcesarProductoIntegrado(tipo, raiz);
                        break;

                    case "categorias:selectAll":
                    case "obtenerEstados":
                    case "categorias:guardarIntegrado":
                    case "categorias:actualizarIntegrado":
                    case "categorias:desactivarIntegrado":
                        ProcesarCategoriaIntegrada(tipo, raiz);
                        break;

                    case "subcategorias:selectAll":
                    case "subcategorias:guardarIntegrado":
                        ProcesarSubcategoriaIntegrada(tipo, raiz);
                        break;

                    case "clientes:selectAll":
                    case "clientes:guardarIntegrado":
                        ProcesarClienteIntegrado(tipo, raiz);
                        break;

                    case "marcas:selectAll":
                    case "marcas:guardarIntegrado":
                        ProcesarMarcaIntegrada(tipo, raiz);
                        break;

                    case "proveedores:selectAll":
                    case "proveedores:guardarIntegrado":
                        ProcesarProveedorIntegrado(tipo, raiz);
                        break;

                    case "puestos:selectAll":
                    case "puestos:guardarIntegrado":
                        ProcesarPuestoIntegrado(tipo, raiz);
                        break;

                    case "empleados:selectAll":
                    case "empleados:guardarIntegrado":
                        ProcesarEmpleadoIntegrado(tipo, raiz);
                        break;

                    case "roles:selectAll":
                    case "roles:guardarIntegrado":
                        ProcesarRolIntegrado(tipo, raiz);
                        break;

                    case "estados:selectAll":
                        try
                        {
                            var listaEstados = _estadoBL.SelectAll();

                            EnviarAlDashboard(new
                            {
                                type = "estados:list",
                                data = listaEstados
                            });
                        }
                        catch (Exception ex)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = ex.Message
                            });
                        }
                        break;


                    default:
                        ProcesarModuloAdicional(tipo, raiz);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al procesar el mensaje:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        private bool AutorizarMensajePorRol(string tipo)
        {
            if (_sesionActual == null)
                return false;

            if (tipo == "ventas:generarPdf")
                return PermisosSistema.PuedeRealizar(_sesionActual, "ventas", "imprimirTicket");

            if (tipo == "garantias:buscarPorFactura")
                return _sesionActual.RolId == PermisosSistema.RolTecnico;

            if (tipo == "obtenerEstados" || tipo == "estados:selectAll" ||
                tipo == "categorias:selectAll" || tipo == "subcategorias:selectAll" ||
                tipo == "empleados:selectAll")
                return _sesionActual.RolId == PermisosSistema.RolVendedor ||
                       _sesionActual.RolId == PermisosSistema.RolTecnico;

            int separador = tipo.IndexOf(':');
            if (separador <= 0 || separador == tipo.Length - 1)
                return false;

            string modulo = tipo.Substring(0, separador);
            string accion = tipo.Substring(separador + 1);
            return PermisosSistema.PuedeRealizar(_sesionActual, modulo, accion);
        }

        private bool ProcesarCrudMaestros(string tipo, JsonElement raiz)
        {
            const string sufijoActualizar = ":actualizarIntegrado";
            const string sufijoDesactivar = ":desactivarIntegrado";
            bool actualizar = tipo.EndsWith(sufijoActualizar, StringComparison.Ordinal);
            bool desactivar = tipo.EndsWith(sufijoDesactivar, StringComparison.Ordinal);
            if (!actualizar && !desactivar) return false;

            string modulo = tipo.Substring(0, tipo.Length -
                (actualizar ? sufijoActualizar.Length : sufijoDesactivar.Length));


            if (modulo == "categorias") return false;


            string[] permitidos = { "subcategorias", "marcas", "modelos", "vehiculos", "compatibilidad",
                "productos", "proveedores", "clientes", "puestos", "empleados",
                "usuarios", "roles" };
            if (!permitidos.Contains(modulo)) return false;

            try
            {
                if (!raiz.TryGetProperty("data", out JsonElement datos) ||
                    datos.ValueKind != JsonValueKind.Object)
                    throw new ArgumentException("No se recibieron datos válidos para " + modulo + ".");

                if (desactivar && modulo == "marcas")
                {
                    if (!datos.TryGetProperty("MarcaId", out JsonElement marcaIdJson) ||
                        !marcaIdJson.TryGetInt32(out int marcaId) || marcaId <= 0)
                        throw new ArgumentException("Selecciona una marca válida.");

                    Marca marcaActual = _marcaBL.SelectAll()
                        .FirstOrDefault(m => m.MarcaId == marcaId)
                        ?? throw new InvalidOperationException("La marca no existe en SQL Server.");

                    if (marcaActual.EstadoId != 1 && marcaActual.EstadoId != 2)
                        throw new InvalidOperationException("La marca tiene un estado no válido.");

                    int estadoEsperado = marcaActual.EstadoId == 1 ? 2 : 1;
                    int estadoNuevo = _marcaBL.Delete(new Marca { MarcaId = marcaId });

                    if (estadoNuevo != estadoEsperado)
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado de la marca.");

                    EnviarAlDashboard(new
                    {
                        type = "marcas:desactivada",
                        message = estadoNuevo == 1 ? "Marca habilitada correctamente." : "Marca desactivada correctamente.",
                        data = new { MarcaId = marcaId, EstadoId = estadoNuevo }
                    });
                    return true;
                }

                if (desactivar && modulo == "modelos")
                {
                    if (!datos.TryGetProperty("ModeloId", out JsonElement modeloIdJson) ||
                        !modeloIdJson.TryGetInt32(out int modeloId) || modeloId <= 0)
                        throw new ArgumentException("Selecciona un modelo válido.");

                    Modelo modeloActual = _modeloBL.SelectAll()
                        .FirstOrDefault(m => m.ModeloId == modeloId)
                        ?? throw new InvalidOperationException("El modelo no existe en SQL Server.");

                    if (modeloActual.EstadoId != 1 && modeloActual.EstadoId != 2)
                        throw new InvalidOperationException("El modelo tiene un estado no válido.");

                    int estadoEsperado = modeloActual.EstadoId == 1 ? 2 : 1;
                    int estadoNuevo = _modeloBL.Delete(new Modelo { ModeloId = modeloId });

                    if (estadoNuevo != estadoEsperado)
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado del modelo.");

                    EnviarAlDashboard(new
                    {
                        type = "modelos:desactivada",
                        message = estadoNuevo == 1 ? "Modelo habilitado correctamente." : "Modelo desactivado correctamente.",
                        data = new { ModeloId = modeloId, EstadoId = estadoNuevo }
                    });
                    return true;
                }

                if (desactivar && modulo == "vehiculos")
                {
                    if (!datos.TryGetProperty("VehiculoId", out JsonElement vehiculoIdJson) ||
                        !vehiculoIdJson.TryGetInt32(out int vehiculoId) || vehiculoId <= 0)
                        throw new ArgumentException("Selecciona un vehículo válido.");

                    Vehiculo vehiculoActual = _vehiculoBL.SelectAll()
                        .FirstOrDefault(v => v.VehiculoId == vehiculoId)
                        ?? throw new InvalidOperationException("El vehículo no existe en SQL Server.");

                    if (vehiculoActual.EstadoId != 1 && vehiculoActual.EstadoId != 2)
                        throw new InvalidOperationException("El vehículo tiene un estado no válido.");

                    int estadoEsperado = vehiculoActual.EstadoId == 1 ? 2 : 1;
                    int estadoNuevo = _vehiculoBL.Delete(new Vehiculo { VehiculoId = vehiculoId });

                    if (estadoNuevo != estadoEsperado)
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado del vehículo.");

                    EnviarAlDashboard(new
                    {
                        type = "vehiculos:desactivada",
                        message = estadoNuevo == 1 ? "Vehículo habilitado correctamente." : "Vehículo desactivado correctamente.",
                        data = new { VehiculoId = vehiculoId, EstadoId = estadoNuevo }
                    });
                    return true;
                }

                if (desactivar && modulo == "compatibilidad")
                {
                    if (!datos.TryGetProperty("CompatibilidadId", out JsonElement compatibilidadIdJson) ||
                        !compatibilidadIdJson.TryGetInt32(out int compatibilidadId) || compatibilidadId <= 0)
                        throw new ArgumentException("Selecciona una compatibilidad válida.");

                    CompatibilidadRepuesto compatibilidadActual = _compatibilidadBL.SelectAll()
                        .FirstOrDefault(c => c.CompatibilidadId == compatibilidadId)
                        ?? throw new InvalidOperationException("La compatibilidad no existe en SQL Server.");

                    if (compatibilidadActual.EstadoId != 1 && compatibilidadActual.EstadoId != 2)
                        throw new InvalidOperationException("La compatibilidad tiene un estado no válido.");

                    int estadoEsperado = compatibilidadActual.EstadoId == 1 ? 2 : 1;
                    int estadoNuevo = _compatibilidadBL.Delete(new CompatibilidadRepuesto
                    {
                        CompatibilidadId = compatibilidadId
                    });

                    if (estadoNuevo != estadoEsperado)
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado de la compatibilidad.");

                    EnviarAlDashboard(new
                    {
                        type = "compatibilidad:desactivada",
                        message = estadoNuevo == 1 ? "Compatibilidad habilitada correctamente." : "Compatibilidad desactivada correctamente.",
                        data = new { CompatibilidadId = compatibilidadId, EstadoId = estadoNuevo }
                    });
                    return true;
                }

                if (desactivar && modulo == "puestos")
                {
                    if (!datos.TryGetProperty("PuestoId", out JsonElement puestoIdJson) ||
                        !puestoIdJson.TryGetInt32(out int puestoId) || puestoId <= 0)
                        throw new ArgumentException("Selecciona un puesto válido.");

                    Puesto puestoActual = _puestoBL.SelectAll().FirstOrDefault(p => p.PuestoId == puestoId)
                        ?? throw new InvalidOperationException("El puesto no existe en SQL Server.");

                    if (puestoActual.EstadoId != 1 && puestoActual.EstadoId != 2)
                        throw new InvalidOperationException("El puesto tiene un estado no válido.");

                    int estadoEsperado = puestoActual.EstadoId == 1 ? 2 : 1;
                    int estadoNuevo = _puestoBL.Delete(new Puesto { PuestoId = puestoId });

                    if (estadoNuevo != estadoEsperado)
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado del puesto.");

                    EnviarAlDashboard(new
                    {
                        type = "puestos:desactivada",
                        message = estadoNuevo == 1 ? "Puesto habilitado correctamente." : "Puesto desactivado correctamente.",
                        data = new { PuestoId = puestoId, EstadoId = estadoNuevo }
                    });
                    return true;
                }

                if (desactivar && modulo == "empleados")
                {
                    if (!datos.TryGetProperty("EmpleadoId", out JsonElement empleadoIdJson) ||
                        !empleadoIdJson.TryGetInt32(out int empleadoId) || empleadoId <= 0)
                        throw new ArgumentException("Selecciona un empleado válido.");

                    Empleado empleadoActual = _empleadoBL.SelectAll()
                        .FirstOrDefault(e => e.EmpleadoId == empleadoId)
                        ?? throw new InvalidOperationException("El empleado no existe en SQL Server.");

                    if (empleadoActual.EstadoId != 1 && empleadoActual.EstadoId != 2)
                        throw new InvalidOperationException("El empleado tiene un estado no válido.");

                    if (_sesionActual != null && empleadoId == _sesionActual.EmpleadoId &&
                        empleadoActual.EstadoId == 1)
                        throw new InvalidOperationException("No puedes desactivar tu propio empleado durante la sesión.");

                    int estadoEsperado = empleadoActual.EstadoId == 1 ? 2 : 1;
                    int estadoNuevo = _empleadoBL.Delete(new Empleado { EmpleadoId = empleadoId });

                    if (estadoNuevo != estadoEsperado)
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado del empleado.");

                    EnviarAlDashboard(new
                    {
                        type = "empleados:desactivada",
                        message = estadoNuevo == 1 ? "Empleado habilitado correctamente." : "Empleado desactivado correctamente.",
                        data = new { EmpleadoId = empleadoId, EstadoId = estadoNuevo }
                    });
                    return true;
                }

                if (desactivar && modulo == "usuarios")
                {
                    if (!datos.TryGetProperty("UsuarioId", out JsonElement usuarioIdJson) ||
                        !usuarioIdJson.TryGetInt32(out int usuarioId) || usuarioId <= 0)
                        throw new ArgumentException("Selecciona una cuenta de usuario válida.");

                    Usuario usuarioActual = _usuarioBL.SelectAll()
                        .FirstOrDefault(u => u.UsuarioId == usuarioId)
                        ?? throw new InvalidOperationException("La cuenta de usuario no existe.");

                    if (usuarioActual.EstadoId != 1 && usuarioActual.EstadoId != 2)
                        throw new InvalidOperationException("La cuenta tiene un estado no válido.");

                    if (_sesionActual != null && usuarioId == _sesionActual.UsuarioId &&
                        usuarioActual.EstadoId == 1)
                        throw new InvalidOperationException("No puedes desactivar tu propia cuenta mientras tienes la sesión iniciada.");

                    int estadoEsperado = usuarioActual.EstadoId == 1 ? 2 : 1;
                    int estadoNuevo = _usuarioBL.Delete(new Usuario { UsuarioId = usuarioId });

                    if (estadoNuevo != estadoEsperado)
                        throw new InvalidOperationException("SQL Server no confirmó el cambio de estado de la cuenta.");

                    EnviarAlDashboard(new
                    {
                        type = "usuarios:desactivada",
                        message = estadoNuevo == 1 ? "Usuario habilitado correctamente." : "Usuario desactivado correctamente.",
                        data = new { UsuarioId = usuarioId, EstadoId = estadoNuevo }
                    });
                    return true;
                }

                if (desactivar && modulo == "roles")
                {
                    if (!datos.TryGetProperty("RolId", out JsonElement rolIdJson) ||
                        !rolIdJson.TryGetInt32(out int rolId) || rolId <= 0)
                        throw new ArgumentException("Selecciona un rol válido.");

                    Rol rolActual = _rolBL.SelectAll().FirstOrDefault(r => r.RolId == rolId)
                        ?? throw new InvalidOperationException("El rol no existe en SQL Server.");

                    if (rolId == 1 && rolActual.EstadoId == 1)
                        throw new InvalidOperationException("No se puede desactivar el rol administrador principal.");

                    if (_sesionActual != null && rolId == _sesionActual.RolId && rolActual.EstadoId == 1)
                        throw new InvalidOperationException("No puedes desactivar tu rol actual.");

                    if (rolActual.EstadoId != 1 && rolActual.EstadoId != 2)
                        throw new InvalidOperationException("El rol tiene un estado no válido.");

                    int estadoEsperado = rolActual.EstadoId == 1 ? 2 : 1;
                    int estadoNuevo = _rolBL.Delete(new Rol { RolId = rolId });

                    if (estadoNuevo != estadoEsperado)
                        throw new InvalidOperationException("El estado devuelto por SQL Server no coincide con el cambio solicitado.");

                    EnviarAlDashboard(new
                    {
                        type = "roles:desactivada",
                        message = estadoNuevo == 1 ? "Rol habilitado correctamente." : "Rol desactivado correctamente.",
                        data = new { RolId = rolId, EstadoId = estadoNuevo }
                    });
                    return true;
                }


                switch (modulo)
                {
                    case "subcategorias":
                        EjecutarCrudMaestro<Subcategoria>(modulo, datos, actualizar,
                            "SubcategoriaId", x => x.SubcategoriaId,
                            _subcategoriaBL.SelectAll, _subcategoriaBL.Update, _subcategoriaBL.Delete,
                            x =>
                            {
                                RequerirId(x.CategoriaId, "categoría");
                                RequerirTexto(x.Nombre, 120, "nombre de subcategoría");
                            });
                        break;
                    case "marcas":
                        EjecutarCrudMaestro<Marca>(modulo, datos, actualizar,
                            "MarcaId", x => x.MarcaId,
                            _marcaBL.SelectAll, _marcaBL.Update, _marcaBL.Delete,
                            x =>
                            {
                                RequerirTexto(x.Nombre, 100, "nombre de marca");
                            },
                            (nuevo, anterior) =>
                            {
                                nuevo.EstadoId = anterior.EstadoId;
                            });
                        break;
                    case "modelos":
                        EjecutarCrudMaestro<Modelo>(modulo, datos, actualizar,
                            "ModeloId", x => x.ModeloId,
                            _modeloBL.SelectAll, _modeloBL.Update, _modeloBL.Delete,
                            x =>
                            {
                                ValidarModelo(x);
                                if (!_marcaBL.SelectAll().Any(m => m.MarcaId == x.MarcaId && m.EstadoId == 1))
                                    throw new ArgumentException("Selecciona una marca activa.");
                            },
                            (nuevo, anterior) =>
                            {
                                nuevo.EstadoId = anterior.EstadoId;
                            });
                        break;
                    case "vehiculos":
                        EjecutarCrudMaestro<Vehiculo>(modulo, datos, actualizar,
                            "VehiculoId", x => x.VehiculoId,
                            _vehiculoBL.SelectAll, _vehiculoBL.Update, _vehiculoBL.Delete,
                            x =>
                            {
                                ValidarVehiculo(x);
                                if (!_modeloBL.SelectAll().Any(m => m.ModeloId == x.ModeloId && m.EstadoId == 1))
                                    throw new ArgumentException("Selecciona un modelo activo.");
                            },
                            (nuevo, anterior) =>
                            {
                                nuevo.EstadoId = anterior.EstadoId;
                            });
                        break;
                    case "compatibilidad":
                        EjecutarCrudMaestro<CompatibilidadRepuesto>(modulo, datos, actualizar,
                            "CompatibilidadId", x => x.CompatibilidadId,
                            _compatibilidadBL.SelectAll, _compatibilidadBL.Update, _compatibilidadBL.Delete,
                            x =>
                            {
                                ValidarCompatibilidad(x);
                                if (!_productoBL.SelectAll().Any(p => p.ProductoId == x.ProductoId && p.EstadoId == 1))
                                    throw new ArgumentException("Selecciona un producto activo.");
                                if (!_vehiculoBL.SelectAll().Any(v => v.VehiculoId == x.VehiculoId && v.EstadoId == 1))
                                    throw new ArgumentException("Selecciona un vehículo activo.");
                            },
                            (nuevo, anterior) =>
                            {
                                nuevo.EstadoId = anterior.EstadoId;
                            });
                        break;
                    case "productos":
                        if (actualizar)
                        {
                            ActualizarProductoConFotografia(datos);
                            break;
                        }
                        EjecutarCrudMaestro<Producto>(modulo, datos, actualizar,
                            "ProductoId", x => x.ProductoId,
                            _productoBL.SelectAll, _productoBL.Update, _productoBL.Delete,
                            x => { ValidarProducto(x); ValidarEstadoMaestro(x.EstadoId); },
                            (nuevo, anterior) =>
                            {


                                nuevo.Imagen = anterior.Imagen;
                                if (datos.TryGetProperty("ImagenArchivo", out var imagen) &&
                                    imagen.ValueKind != JsonValueKind.Null &&
                                    imagen.ValueKind != JsonValueKind.Undefined)
                                    throw new ArgumentException(
                                        "La edición de fotografía requiere su flujo específico; no se modificó la actual.");
                            });
                        break;
                    case "proveedores":
                        EjecutarCrudMaestro<Proveedor>(modulo, datos, actualizar,
                            "ProveedorId", x => x.ProveedorId,
                            _proveedorBL.SelectAll, _proveedorBL.Update, _proveedorBL.Delete,
                            x =>
                            {
                                RequerirTexto(x.NombreEmpresa, 100, "nombre de empresa");
                                RequerirTexto(x.NIT, 20, "NIT");
                                RequerirTexto(x.Telefono, 25, "teléfono");
                                RequerirTexto(x.Correo, 150, "correo");
                                RequerirTexto(x.Contacto, 100, "contacto");
                            });
                        break;
                    case "clientes":
                        EjecutarCrudMaestro<Cliente>(modulo, datos, actualizar,
                            "ClienteId", x => x.ClienteId,
                            _clienteBL.SelectAll, _clienteBL.Update,
                            x => _clienteBL.Delete(x.ClienteId),
                            ValidarClienteMaestro);
                        break;
                    case "puestos":
                        EjecutarCrudMaestro<Puesto>(modulo, datos, actualizar,
                            "PuestoId", x => x.PuestoId,
                            _puestoBL.SelectAll, _puestoBL.Update, _puestoBL.Delete,
                            x =>
                            {
                                RequerirTexto(x.Nombre, 100, "nombre del puesto");
                                RequerirTexto(x.Descripcion, 500, "descripción del puesto");
                            },
                            (nuevo, anterior) =>
                            {
                                nuevo.EstadoId = anterior.EstadoId;
                            });
                        break;
                    case "empleados":
                        EjecutarCrudMaestro<Empleado>(modulo, datos, actualizar,
                            "EmpleadoId", x => x.EmpleadoId,
                            _empleadoBL.SelectAll, _empleadoBL.Update, _empleadoBL.Delete,
                            ValidarEmpleado,
                            (nuevo, anterior) =>
                            {
                                nuevo.EstadoId = anterior.EstadoId;
                            },
                            id =>
                            {
                                if (_sesionActual != null && id == _sesionActual.EmpleadoId)
                                    throw new InvalidOperationException("No puedes desactivar tu propio empleado.");
                            });
                        break;
                    case "roles":
                        EjecutarCrudMaestro<Rol>(modulo, datos, actualizar,
                            "RolId", x => x.RolId,
                            _rolBL.SelectAll, _rolBL.Update, _rolBL.Delete,
                            x =>
                            {
                                RequerirTexto(x.Nombre, 100, "nombre del rol");
                            },
                            (nuevo, anterior) =>
                            {
                                nuevo.EstadoId = anterior.EstadoId;
                            },
                            id =>
                            {
                                if (_sesionActual != null && id == _sesionActual.RolId)
                                    throw new InvalidOperationException("No puedes desactivar tu rol actual.");
                            });
                        break;
                    case "usuarios":
                        EjecutarCrudMaestro<Usuario>(modulo, datos, actualizar,
                            "UsuarioId", x => x.UsuarioId,
                            _usuarioBL.SelectAll, _usuarioBL.Update, _usuarioBL.Delete,
                            x =>
                            {
                                RequerirId(x.EmpleadoId, "empleado");
                                RequerirId(x.RolId, "rol");
                                RequerirTexto(x.NombreUsuario, 50, "nombre de usuario");
                                x.NombreUsuario = x.NombreUsuario.Trim();
                                if (x.NombreUsuario.Length < 3 ||
                                    !System.Text.RegularExpressions.Regex.IsMatch(x.NombreUsuario, @"\A[A-Za-z0-9._-]+\z"))
                                    throw new ArgumentException("El usuario debe tener de 3 a 50 caracteres y usar letras, números, punto, guion o guion bajo.");

                                if (!string.IsNullOrEmpty(x.Contrasena) &&
                                    (string.IsNullOrWhiteSpace(x.Contrasena) || x.Contrasena.Length < 8 || x.Contrasena.Length > 72))
                                    throw new ArgumentException("La contraseña nueva debe tener de 8 a 72 caracteres, o quedar vacía para conservar la actual.");

                                if (!_empleadoBL.SelectAll().Any(e => e.EmpleadoId == x.EmpleadoId && e.EstadoId == 1))
                                    throw new ArgumentException("Selecciona un empleado activo.");
                                if (!_rolBL.SelectAll().Any(r => r.RolId == x.RolId && r.EstadoId == 1))
                                    throw new ArgumentException("Selecciona un rol activo.");
                            },
                            (nuevo, anterior) =>
                            {
                                nuevo.EstadoId = anterior.EstadoId;
                                if (_sesionActual != null && nuevo.UsuarioId == _sesionActual.UsuarioId &&
                                    (nuevo.RolId != anterior.RolId || nuevo.EmpleadoId != anterior.EmpleadoId))
                                    throw new InvalidOperationException("No puedes cambiar tu propio rol o empleado mientras tienes la sesión iniciada.");

                                if (anterior.EstadoId == 1 && anterior.RolId == 1 && nuevo.RolId != 1 &&
                                    !_usuarioBL.SelectAll().Any(u => u.UsuarioId != anterior.UsuarioId &&
                                        u.RolId == 1 && u.EstadoId == 1))
                                    throw new InvalidOperationException("No puedes quitar el rol de la última cuenta administradora activa.");
                            },
                            id =>
                            {
                                if (_sesionActual != null && id == _sesionActual.UsuarioId)
                                    throw new InvalidOperationException("No puedes desactivar tu propia cuenta.");
                            });
                        break;
                }

                EnviarAlDashboard(new
                {
                    type = modulo + (actualizar ? ":actualizada" : ":desactivada"),
                    message = actualizar ? "Registro actualizado correctamente." :
                        "Estado del registro cambiado correctamente."
                });
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new { type = modulo + ":error", message = ex.Message });
            }
            return true;
        }

        private void EjecutarCrudMaestro<T>(
            string modulo, JsonElement datos, bool actualizar, string nombreId,
            Func<T, int> obtenerId, Func<List<T>> seleccionar,
            Func<T, int> modificar, Func<T, int>? desactivar,
            Action<T>? validar = null, Action<T, T>? validarCambio = null,
            Action<int>? validarBaja = null) where T : class, new()
        {
            if (!datos.TryGetProperty(nombreId, out JsonElement campoId) ||
                !campoId.TryGetInt32(out int id) || id <= 0)
                throw new ArgumentException("Selecciona un registro válido de " + modulo + ".");

            T anterior = seleccionar().FirstOrDefault(x => obtenerId(x) == id)
                ?? throw new InvalidOperationException("El registro no existe en SQL Server.");

            var propiedadEstado = typeof(T).GetProperty("EstadoId");

            if (!actualizar)
            {
                if (desactivar == null)
                    throw new InvalidOperationException("Este módulo no admite cambio de estado.");

                if (propiedadEstado == null || !propiedadEstado.CanRead)
                    throw new InvalidOperationException("Este módulo no tiene un EstadoId válido.");

                object? valor = propiedadEstado.GetValue(anterior);
                if (valor == null)
                    throw new InvalidOperationException("El registro no tiene un estado válido.");

                int estadoAnterior = Convert.ToInt32(valor, CultureInfo.InvariantCulture);
                ValidarEstadoMaestro(estadoAnterior);
                validarBaja?.Invoke(id);

                int estadoEsperado = estadoAnterior == 1 ? 2 : 1;
                int estadoNuevo = desactivar(new TConId<T>(id, nombreId).Crear());

                if (estadoNuevo != estadoEsperado)
                    throw new InvalidOperationException(
                        "SQL Server no confirmó el nuevo estado. Se esperaba EstadoId = " + estadoEsperado + ".");
                return;
            }

            T entidad = datos.Deserialize<T>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new ArgumentException("Los datos de edición no son válidos.");

            if (obtenerId(entidad) != id)
                throw new ArgumentException("El identificador de edición es incorrecto.");

            if (propiedadEstado != null && propiedadEstado.CanRead && propiedadEstado.CanWrite)
                propiedadEstado.SetValue(entidad, propiedadEstado.GetValue(anterior));

            validar?.Invoke(entidad);
            validarCambio?.Invoke(entidad, anterior);

            int resultado = modificar(entidad);
            if (resultado <= 0)
                throw new InvalidOperationException("SQL Server no confirmó la actualización.");
        }


        private sealed class TConId<T> where T : class, new()
        {
            private readonly int _id;
            private readonly string _propiedad;
            public TConId(int id, string propiedad) { _id = id; _propiedad = propiedad; }
            public T Crear()
            {
                T entidad = new T();
                var propiedad = typeof(T).GetProperty(_propiedad);
                if (propiedad == null || propiedad.PropertyType != typeof(int) || !propiedad.CanWrite)
                    throw new InvalidOperationException("El identificador de entidad no es válido.");
                propiedad.SetValue(entidad, _id);
                return entidad;
            }
        }

        private static void ValidarEstadoMaestro(int estadoId)
        {
            if (estadoId != 1 && estadoId != 2)
                throw new ArgumentException("El estado debe ser Activo (1) o Inactivo (2).");
        }

        private static void ValidarClienteMaestro(Cliente cliente)
        {
            RequerirTexto(cliente.Nombre, 100, "nombre");
            RequerirTexto(cliente.Apellido, 100, "apellido");
            RequerirTexto(cliente.DUI, 10, "DUI");
            RequerirTexto(cliente.Telefono, 25, "teléfono");
            RequerirTexto(cliente.Correo, 150, "correo");
            if ((cliente.NIT?.Length ?? 0) > 20 ||
                (cliente.Direccion?.Length ?? 0) > 50)
                throw new ArgumentException("NIT o dirección superan la longitud permitida.");
            if (!System.Net.Mail.MailAddress.TryCreate(cliente.Correo, out var correo) ||
                correo.Address != cliente.Correo)
                throw new ArgumentException("El correo electrónico no es válido.");
        }


        private void ProcesarOperacionGarantia(string tipo, JsonElement raiz)
        {
            try
            {
                int separadorPermiso = tipo.IndexOf(':');
                if (_sesionActual == null || separadorPermiso <= 0 ||
                    !PermisosSistema.PuedeRealizar(_sesionActual,
                        tipo.Substring(0, separadorPermiso),
                        tipo.Substring(separadorPermiso + 1)))
                    throw new UnauthorizedAccessException("No tienes permisos para gestionar garantías.");

                if (!raiz.TryGetProperty("data", out JsonElement datos) ||
                    datos.ValueKind != JsonValueKind.Object)
                    throw new ArgumentException("No se recibieron los datos de la garantía.");


                int empleadoId = _sesionActual.EmpleadoId;
                if (empleadoId <= 0)
                    throw new InvalidOperationException("La sesión no tiene un empleado válido.");

                if (tipo == "garantias:registrarSolicitud")
                {
                    int detalleVentaId = LeerEnteroGarantia(datos, "DetalleVentaId");
                    int cantidad = LeerEnteroGarantia(datos, "CantidadReclamada");
                    string descripcion = LeerTextoGarantia(datos, "DescripcionFalla") ?? "";
                    string? observaciones = LeerTextoGarantia(datos, "Observaciones");

                    GarantiaSolicitudResultado resultado = _garantiaBL.RegistrarSolicitud(
                        detalleVentaId, empleadoId, cantidad, descripcion, observaciones);

                    EnviarAlDashboard(new { type = "garantias:solicitudRegistrada", data = resultado });
                }
                else if (tipo == "garantias:evaluar")
                {
                    int garantiaId = LeerEnteroGarantia(datos, "GarantiaId");
                    string decision = LeerTextoGarantia(datos, "Decision") ?? "";
                    if (!datos.TryGetProperty("DefectoFabricacion", out JsonElement defecto) ||
                        (defecto.ValueKind != JsonValueKind.True && defecto.ValueKind != JsonValueKind.False))
                        throw new ArgumentException("Indica si se confirmó un defecto de fabricación.");
                    string? observaciones = LeerTextoGarantia(datos, "Observaciones");

                    GarantiaEvaluacionResultado resultado = _garantiaBL.Evaluar(
                        garantiaId, empleadoId, decision, defecto.GetBoolean(), observaciones);

                    EnviarAlDashboard(new { type = "garantias:evaluada", data = resultado });
                }
                else if (tipo == "reemplazos-garantia:entregar")
                {
                    int garantiaId = LeerEnteroGarantia(datos, "GarantiaId");
                    string? observaciones = LeerTextoGarantia(datos, "Observaciones");

                    ReemplazoEntregaResultado resultado = _reemplazoGarantiaBL.EntregarReemplazo(
                        garantiaId, empleadoId, observaciones);


                    EnviarAlDashboard(new { type = "reemplazos-garantia:resultadoEntrega", data = resultado });
                }
            }
            catch (Exception ex)
            {
                string respuesta = tipo switch
                {
                    "garantias:registrarSolicitud" => "garantias:solicitudError",
                    "garantias:evaluar" => "garantias:evaluacionError",
                    _ => "reemplazos-garantia:entregaError"
                };
                EnviarAlDashboard(new { type = respuesta, message = ex.Message });
            }
        }

        private static int LeerEnteroGarantia(JsonElement datos, string propiedad)
        {
            if (!datos.TryGetProperty(propiedad, out JsonElement campo) ||
                campo.ValueKind != JsonValueKind.Number || !campo.TryGetInt32(out int valor) || valor <= 0)
                throw new ArgumentException("Selecciona un valor válido para " + propiedad + ".");
            return valor;
        }

        private static string? LeerTextoGarantia(JsonElement datos, string propiedad)
        {
            if (!datos.TryGetProperty(propiedad, out JsonElement campo) ||
                campo.ValueKind == JsonValueKind.Null || campo.ValueKind == JsonValueKind.Undefined)
                return null;
            if (campo.ValueKind != JsonValueKind.String)
                throw new ArgumentException("El campo " + propiedad + " debe ser texto.");
            return campo.GetString();
        }


        private object ObtenerResumenDashboard()
        {
            var ventas = _ventaBL.SelectAll()
                .OrderByDescending(v => v.FechaVenta)
                .ThenByDescending(v => v.VentaId)
                .Take(10)
                .ToList();

            var idsClientes = ventas.Select(v => v.ClienteId).ToHashSet();
            var clientes = _clienteBL.SelectAll()
                .Where(c => idsClientes.Contains(c.ClienteId))
                .ToDictionary(c => c.ClienteId);


            var filasVentas = ventas.Select(v => new object[]
            {
                v.NumeroFactura ?? ("Venta #" + v.VentaId),
                clientes.TryGetValue(v.ClienteId, out var cliente)
                    ? (cliente.Nombre + " " + cliente.Apellido).Trim()
                    : "Cliente #" + v.ClienteId,
                v.FechaVenta.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
                v.Total,
                "Registrada"
            }).ToArray();

            var bajos = _inventarioBL.SelectAll()
                .Where(i => i.StockActual <= i.StockMinimo)
                .OrderBy(i => i.StockActual)
                .ThenBy(i => i.ProductoId)
                .ToList();

            var idsProductos = bajos.Select(i => i.ProductoId).ToHashSet();
            var productos = _productoBL.SelectAll()
                .Where(p => idsProductos.Contains(p.ProductoId))
                .ToDictionary(p => p.ProductoId);


            var filasStock = bajos.Select(i =>
            {
                productos.TryGetValue(i.ProductoId, out var producto);
                return new object[]
                {
                    "!",
                    producto?.Nombre ?? ("Producto #" + i.ProductoId),
                    i.StockActual == 0 ? "Agotado" : "Existencias bajas",
                    producto?.CodigoOEM ?? ("#" + i.ProductoId),
                    i.StockActual,
                    i.StockMinimo
                };
            }).ToArray();

            return new { Ventas = filasVentas, StockBajo = filasStock };
        }


        private async Task ProcesarCorreoFacturaNuevaAsync(int ventaId)
        {
            try
            {
                var venta = await Task.Run(() => _ventaBL.SelectAll()
                    .FirstOrDefault(v => v.VentaId == ventaId))
                    ?? throw new InvalidOperationException("No se encontró la venta registrada.");
                var cliente = await Task.Run(() => _clienteBL.SelectAll()
                    .FirstOrDefault(c => c.ClienteId == venta.ClienteId));

                if (cliente == null || string.IsNullOrWhiteSpace(cliente.Correo))
                {
                    EnviarAlDashboard(new
                    {
                        type = "ventas:correoEstado",
                        data = new { VentaId = ventaId, Enviado = false },
                        message = "Venta registrada. El cliente no tiene correo para enviar la factura."
                    });
                    MessageBox.Show(
                        "La venta se registró, pero el cliente no tiene correo configurado. No se envió ninguna factura.",
                        "AutoParts - Correo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var servicioPdf = new FacturaPdfService(
                    _ventaBL, _detalleVentaBL, _productoBL, _clienteBL, _empleadoBL);
                string rutaPdf = await Task.Run(() => servicioPdf.Generar(ventaId));
                string enlace = await ObtenerOPublicarFacturaDigitalAsync(
                    ventaId, rutaPdf, venta.NumeroFactura ?? "");

                var servicioCorreo = new FacturaCorreoService();
                bool enviado = await servicioCorreo.EnviarSiPendienteAsync(
                    ventaId,
                    venta.NumeroFactura ?? "",
                    cliente.Correo,
                    (cliente.Nombre + " " + cliente.Apellido).Trim(),
                    rutaPdf,
                    enlace);

                EnviarAlDashboard(new
                {
                    type = "ventas:correoEstado",
                    data = new { VentaId = ventaId, Enviado = enviado },
                    message = enviado
                        ? "Factura PDF enviada automáticamente al correo del cliente."
                        : "La factura ya había sido enviada por correo."
                });
                MessageBox.Show(
                    enviado
                        ? "Gmail aceptó el envío de la factura " + venta.NumeroFactura + " a " + cliente.Correo + ". Revisa también Spam si no aparece en Recibidos."
                        : "Esta venta ya figura como enviada en el registro local. No se volvió a enviar el correo.",
                    "AutoParts - Correo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new
                {
                    type = "ventas:correoEstado",
                    data = new { VentaId = ventaId, Enviado = false },
                    message = "Venta registrada. No se pudo enviar el correo automático: " + ex.Message
                });
                MessageBox.Show(
                    "La venta se registró, pero no se pudo enviar la factura por Gmail.\n\n" + ex.Message,
                    "AutoParts - Correo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private const string NombreImpresoraPt210 = "AutoParts PT-210";
        private const string PuertoBluetoothPt210 = "COM4";
        private const bool UsarBluetoothCom4Pt210 = true;
        private const int AnchoTicketPt210 = 32;

        private void ImprimirTicketVentaPt210(int ventaId)
        {
            var venta = _ventaBL.SelectAll().FirstOrDefault(v => v.VentaId == ventaId)
                ?? throw new InvalidOperationException("La venta no existe en SQL Server.");
            var lineas = _detalleVentaBL.SelectAll().Where(d => d.VentaId == ventaId).ToList();
            if (lineas.Count == 0)
                throw new InvalidOperationException("La venta no tiene detalles registrados.");

            var productos = _productoBL.SelectAll().ToDictionary(p => p.ProductoId);
            var cliente = _clienteBL.SelectAll().FirstOrDefault(c => c.ClienteId == venta.ClienteId);
            var empleado = _empleadoBL.SelectAll().FirstOrDefault(e => e.EmpleadoId == venta.EmpleadoId);
            var t = new StringBuilder();
            AgregarCentradoPt210(t, "REPUESTOS Y ACCESORIOS");
            AgregarCentradoPt210(t, "COMPROBANTE INTERNO");
            t.AppendLine(new string('-', AnchoTicketPt210));
            AgregarPartidoPt210(t, "Factura: " + venta.NumeroFactura);
            t.AppendLine("Fecha: " + venta.FechaVenta.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
            AgregarPartidoPt210(t, "Cliente: " + (cliente == null ? "No identificado" : cliente.Nombre + " " + cliente.Apellido));
            AgregarPartidoPt210(t, "Atendio: " + (empleado == null ? "No identificado" : empleado.Nombre + " " + empleado.Apellido));
            t.AppendLine(new string('-', AnchoTicketPt210));

            foreach (var detalle in lineas)
            {
                string nombre = productos.TryGetValue(detalle.ProductoId, out var producto)
                    ? producto.Nombre : "Producto #" + detalle.ProductoId;
                AgregarPartidoPt210(t, nombre);
                string izquierda = detalle.Cantidad.ToString(CultureInfo.InvariantCulture)
                    + " x $" + detalle.PrecioUnitario.ToString("0.00", CultureInfo.InvariantCulture);
                AgregarDosColumnasPt210(t, izquierda,
                    "$" + detalle.Subtotal.ToString("0.00", CultureInfo.InvariantCulture));
            }

            t.AppendLine(new string('-', AnchoTicketPt210));
            AgregarDosColumnasPt210(t, "TOTAL", "$" + venta.Total.ToString("0.00", CultureInfo.InvariantCulture));
            t.AppendLine(new string('-', AnchoTicketPt210));
            AgregarCentradoPt210(t, "COD");

            string codigoEscaneable = (venta.NumeroFactura ?? string.Empty).Trim().Replace("-", "");
            if (codigoEscaneable.Length == 0 || codigoEscaneable.Length > 40 ||
                codigoEscaneable.Any(c => c < 32 || c > 126 || c == '{'))
                throw new InvalidOperationException("El número de factura no se puede codificar en Code 128.");

            string? enlaceQr = ObtenerEnlaceFacturaDigitalGuardado(ventaId);
            if (string.IsNullOrWhiteSpace(enlaceQr))
            {
                try
                {
                    var servicioPdf = new FacturaPdfService(
                        _ventaBL, _detalleVentaBL, _productoBL, _clienteBL, _empleadoBL);
                    string rutaPdf = servicioPdf.Generar(ventaId);
                    enlaceQr = ObtenerOPublicarFacturaDigitalAsync(
                        ventaId, rutaPdf, venta.NumeroFactura ?? "")
                        .GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Factura digital no disponible: " + ex.Message);
                }
            }

            using (var bytes = new MemoryStream())
            {
                bytes.WriteByte(0x1B);
                bytes.WriteByte(0x40);
                if (!IntentarEscribirLogoPt210(bytes))
                    EscribirTextoCentradoPt210(bytes, "AUTOPARTS\n");
                EscribirTextoPt210(bytes, t.ToString());
                EscribirCodigoBarrasPt210(bytes, codigoEscaneable);
                EscribirTextoCentradoPt210(bytes, codigoEscaneable + "\n\n");

                if (!string.IsNullOrWhiteSpace(enlaceQr))
                {
                    EscribirTextoPt210(bytes, new string('-', AnchoTicketPt210) + "\n");
                    EscribirTextoCentradoPt210(bytes, "ESCANEA TU FACTURA DIGITAL\n");
                    EscribirQrPt210(bytes, enlaceQr);
                    EscribirTextoCentradoPt210(bytes, "Consulta tu factura PDF\n\n");
                }

                var pie = new StringBuilder();
                AgregarCentradoPt210(pie, "GRACIAS POR SU COMPRA!");
                AgregarCentradoPt210(pie, "Conserve su comprobante");
                pie.AppendLine();
                pie.AppendLine();
                pie.AppendLine();
                EscribirTextoPt210(bytes, pie.ToString());

                byte[] datos = bytes.ToArray();
                if (UsarBluetoothCom4Pt210)
                    EnviarPuertoBluetoothPt210(PuertoBluetoothPt210, datos);
                else
                    EnviarTextoCrudoPt210(NombreImpresoraPt210, datos);
            }
        }

        private static readonly object CandadoEnlacesFactura = new object();

        private static string RutaEnlacesFacturaDigital => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AutoParts", "Facturas", "enlaces-digitales.json");

        private static Dictionary<string, string> LeerEnlacesFacturaDigital()
        {
            if (!File.Exists(RutaEnlacesFacturaDigital))
                return new Dictionary<string, string>();
            string contenido = File.ReadAllText(RutaEnlacesFacturaDigital);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(contenido)
                ?? new Dictionary<string, string>();
        }

        private static string? ObtenerEnlaceFacturaDigitalGuardado(int ventaId)
        {
            lock (CandadoEnlacesFactura)
            {
                var enlaces = LeerEnlacesFacturaDigital();
                return enlaces.TryGetValue(ventaId.ToString(CultureInfo.InvariantCulture), out var url)
                    && Uri.TryCreate(url, UriKind.Absolute, out var uri)
                    && uri.Scheme == Uri.UriSchemeHttps
                    && uri.Host.Equals("guiaqhvpitswyusrjpzj.supabase.co", StringComparison.OrdinalIgnoreCase)
                    ? url : null;
            }
        }

        private static void GuardarEnlaceFacturaDigital(int ventaId, string enlace)
        {
            if (!Uri.TryCreate(enlace, UriKind.Absolute, out Uri? uri)
                || uri.Scheme != Uri.UriSchemeHttps
                || !uri.Host.Equals("guiaqhvpitswyusrjpzj.supabase.co", StringComparison.OrdinalIgnoreCase)
                || !uri.AbsolutePath.Equals("/functions/v1/consultar-factura", StringComparison.Ordinal))
                throw new InvalidOperationException("Supabase devolvió una dirección de factura no válida.");

            lock (CandadoEnlacesFactura)
            {
                var enlaces = LeerEnlacesFacturaDigital();
                enlaces[ventaId.ToString(CultureInfo.InvariantCulture)] = enlace;
                string ruta = RutaEnlacesFacturaDigital;
                Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
                string temporal = ruta + ".tmp";
                File.WriteAllText(temporal, JsonSerializer.Serialize(enlaces));
                File.Move(temporal, ruta, true);
            }
        }

        private async Task<string> ObtenerOPublicarFacturaDigitalAsync(
            int ventaId, string rutaPdf, string numeroFactura)
        {
            string? guardado = ObtenerEnlaceFacturaDigitalGuardado(ventaId);
            if (!string.IsNullOrWhiteSpace(guardado)) return guardado;

            var servicio = new FacturaDigitalService();
            string enlace = await servicio.PublicarAsync(rutaPdf, numeroFactura);
            GuardarEnlaceFacturaDigital(ventaId, enlace);
            return enlace;
        }

        private static void EscribirTextoCentradoPt210(Stream destino, string texto)
        {
            destino.WriteByte(0x1B);
            destino.WriteByte(0x61);
            destino.WriteByte(0x01);
            EscribirTextoPt210(destino, texto);
            destino.WriteByte(0x1B);
            destino.WriteByte(0x61);
            destino.WriteByte(0x00);
        }

        private static void EscribirQrPt210(Stream destino, string enlace)
        {
            byte[] datos = Encoding.ASCII.GetBytes(enlace);
            int longitud = datos.Length + 3;
            if (datos.Length == 0 || longitud > 65535)
                throw new InvalidOperationException("El enlace QR no es válido.");

            destino.WriteByte(0x1B);
            destino.WriteByte(0x61);
            destino.WriteByte(0x01);
            destino.Write(new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00 });
            destino.Write(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x04 });
            destino.Write(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x31 });
            destino.Write(new byte[] {
                0x1D, 0x28, 0x6B,
                (byte)(longitud & 0xFF), (byte)((longitud >> 8) & 0xFF),
                0x31, 0x50, 0x30
            });
            destino.Write(datos, 0, datos.Length);
            destino.Write(new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30, 0x0A });
            destino.WriteByte(0x1B);
            destino.WriteByte(0x61);
            destino.WriteByte(0x00);
        }

        private static bool IntentarEscribirLogoPt210(Stream destino)
        {
            string rutaLogo = Path.Combine(AppContext.BaseDirectory, "Web", "Logotipo.png");
            if (!File.Exists(rutaLogo)) return false;

            try
            {
                using var imagen = System.Drawing.Image.FromFile(rutaLogo);
                int ancho = 288;
                int alto = Math.Min(160, Math.Max(1, (int)Math.Round(
                    imagen.Height * (double)ancho / imagen.Width)));
                using var lienzo = new System.Drawing.Bitmap(ancho, alto);
                using (var grafico = System.Drawing.Graphics.FromImage(lienzo))
                {
                    grafico.Clear(System.Drawing.Color.White);
                    grafico.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    grafico.DrawImage(imagen, 0, 0, ancho, alto);
                }

                int anchoBytes = (ancho + 7) / 8;
                byte[] pixeles = new byte[anchoBytes * alto];
                for (int y = 0; y < alto; y++)
                {
                    for (int x = 0; x < ancho; x++)
                    {
                        var pixel = lienzo.GetPixel(x, y);
                        int intensidad = (pixel.R * 299 + pixel.G * 587 + pixel.B * 114) / 1000;
                        if (intensidad < 160)
                            pixeles[y * anchoBytes + x / 8] |= (byte)(0x80 >> (x % 8));
                    }
                }

                destino.Write(new byte[] { 0x1B, 0x61, 0x01 });
                destino.Write(new byte[] {
                    0x1D, 0x76, 0x30, 0x00,
                    (byte)(anchoBytes & 0xFF), (byte)(anchoBytes >> 8),
                    (byte)(alto & 0xFF), (byte)(alto >> 8)
                });
                destino.Write(pixeles, 0, pixeles.Length);
                destino.WriteByte(0x0A);
                destino.Write(new byte[] { 0x1B, 0x61, 0x00 });
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Logo PT-210 no disponible: " + ex.Message);
                return false;
            }
        }

        private static void EscribirTextoPt210(Stream destino, string texto)
        {
            byte[] datos = Encoding.ASCII.GetBytes(texto.Replace("\r\n", "\n").Replace("\n", "\r\n"));
            destino.Write(datos, 0, datos.Length);
        }

        private static void EscribirCodigoBarrasPt210(Stream destino, string codigo)
        {
            byte[] contenido = Encoding.ASCII.GetBytes("{B" + codigo);
            if (contenido.Length > 255)
                throw new ArgumentException("El código de barras supera el tamaño permitido.");

            destino.WriteByte(0x1B);
            destino.WriteByte(0x61);
            destino.WriteByte(0x01);
            destino.WriteByte(0x1D);
            destino.WriteByte(0x77);
            destino.WriteByte(0x02);
            destino.WriteByte(0x1D);
            destino.WriteByte(0x68);
            destino.WriteByte(0x58);
            destino.WriteByte(0x1D);
            destino.WriteByte(0x48);
            destino.WriteByte(0x00);
            destino.WriteByte(0x1D);
            destino.WriteByte(0x6B);
            destino.WriteByte(0x49);
            destino.WriteByte((byte)contenido.Length);
            destino.Write(contenido, 0, contenido.Length);
            destino.WriteByte(0x0A);
            destino.WriteByte(0x1B);
            destino.WriteByte(0x61);
            destino.WriteByte(0x00);
        }

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Microsoft.Win32.SafeHandles.SafeFileHandle AbrirPuertoBluetoothPt210(
            string nombre, uint acceso, uint compartir, IntPtr seguridad, uint creacion,
            uint atributos, IntPtr plantilla);

        [DllImport("kernel32.dll", EntryPoint = "WriteFile", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EscribirPuertoBluetoothPt210(
            Microsoft.Win32.SafeHandles.SafeFileHandle puerto,
            byte[] datos, uint longitud, out uint escritos, IntPtr superpuesto);

        private static void EnviarPuertoBluetoothPt210(string puerto, byte[] datos)
        {
            using (var manejador = AbrirPuertoBluetoothPt210(
                @"\\.\" + puerto, 0xC0000000u, 0, IntPtr.Zero, 3, 0, IntPtr.Zero))
            {
                if (manejador.IsInvalid)
                    throw new InvalidOperationException("No se pudo abrir " + puerto +
                        ". Comprueba que la PT-210 esté conectada y que el puerto no esté ocupado. Error " +
                        Marshal.GetLastWin32Error());

                const int lote = 256;
                for (int pos = 0; pos < datos.Length; pos += lote)
                {
                    int longitud = Math.Min(lote, datos.Length - pos);
                    byte[] fragmento = new byte[longitud];
                    Buffer.BlockCopy(datos, pos, fragmento, 0, longitud);
                    if (!EscribirPuertoBluetoothPt210(manejador, fragmento,
                            (uint)longitud, out uint escritos, IntPtr.Zero) || escritos != (uint)longitud)
                        throw new InvalidOperationException("No se pudo enviar el ticket completo por " + puerto + ".");
                    System.Threading.Thread.Sleep(45);
                }
            }
        }

        private static string LimpiarPt210(string? texto)
        {
            string descompuesto = (texto ?? string.Empty).Normalize(NormalizationForm.FormD);
            var limpio = new StringBuilder();
            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                limpio.Append(c >= 32 && c <= 126 ? c : ' ');
            }
            return limpio.ToString();
        }

        private static void AgregarPartidoPt210(StringBuilder t, string texto)
        {
            string[] palabras = LimpiarPt210(texto).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string linea = "";
            foreach (string palabra in palabras)
            {
                string pendiente = palabra;
                while (pendiente.Length > AnchoTicketPt210)
                {
                    if (linea.Length > 0) { t.AppendLine(linea); linea = ""; }
                    t.AppendLine(pendiente.Substring(0, AnchoTicketPt210));
                    pendiente = pendiente.Substring(AnchoTicketPt210);
                }
                if (pendiente.Length == 0) continue;
                if (linea.Length == 0) linea = pendiente;
                else if (linea.Length + 1 + pendiente.Length <= AnchoTicketPt210) linea += " " + pendiente;
                else { t.AppendLine(linea); linea = pendiente; }
            }
            if (linea.Length > 0) t.AppendLine(linea);
        }

        private static void AgregarCentradoPt210(StringBuilder t, string texto)
        {
            string x = LimpiarPt210(texto);
            if (x.Length > AnchoTicketPt210) { AgregarPartidoPt210(t, x); return; }
            t.AppendLine(x.PadLeft((AnchoTicketPt210 + x.Length) / 2));
        }

        private static void AgregarDosColumnasPt210(StringBuilder t, string izquierda, string derecha)
        {
            string izq = LimpiarPt210(izquierda);
            string der = LimpiarPt210(derecha);
            if (izq.Length + der.Length + 1 > AnchoTicketPt210)
            {
                AgregarPartidoPt210(t, izq);
                t.AppendLine(der.PadLeft(AnchoTicketPt210));
            }
            else t.AppendLine(izq + new string(' ', AnchoTicketPt210 - izq.Length - der.Length) + der);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private class DocumentoCrudoPt210
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string Nombre = "Ticket AutoParts";
            [MarshalAs(UnmanagedType.LPWStr)] public string? ArchivoSalida;
            [MarshalAs(UnmanagedType.LPWStr)] public string TipoDatos = "RAW";
        }

        [DllImport("winspool.drv", EntryPoint = "OpenPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AbrirImpresoraPt210(string nombre, out IntPtr manejador, IntPtr opciones);

        [DllImport("winspool.drv", EntryPoint = "StartDocPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int IniciarDocumentoPt210(IntPtr impresora, int nivel,
            [In, MarshalAs(UnmanagedType.LPStruct)] DocumentoCrudoPt210 documento);

        [DllImport("winspool.drv", EntryPoint = "StartPagePrinter", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IniciarPaginaPt210(IntPtr impresora);

        [DllImport("winspool.drv", EntryPoint = "WritePrinter", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EscribirPt210(IntPtr impresora, byte[] bytes, int longitud, out int escritos);

        [DllImport("winspool.drv", EntryPoint = "EndPagePrinter", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminarPaginaPt210(IntPtr impresora);

        [DllImport("winspool.drv", EntryPoint = "EndDocPrinter", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminarDocumentoPt210(IntPtr impresora);

        [DllImport("winspool.drv", EntryPoint = "ClosePrinter", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CerrarImpresoraPt210(IntPtr impresora);

        private static void EnviarTextoCrudoPt210(string impresora, byte[] bytes)
        {
            if (!AbrirImpresoraPt210(impresora, out IntPtr manejador, IntPtr.Zero))
                throw new InvalidOperationException("Windows no encuentra la impresora '" + impresora +
                    "'. Revisa su nombre en Impresoras y escáneres. Error " + Marshal.GetLastWin32Error());
            try
            {
                if (IniciarDocumentoPt210(manejador, 1, new DocumentoCrudoPt210()) == 0)
                    throw new InvalidOperationException("No se pudo iniciar el trabajo de impresión. Error " + Marshal.GetLastWin32Error());
                try
                {
                    if (!IniciarPaginaPt210(manejador))
                        throw new InvalidOperationException("No se pudo iniciar la página. Error " + Marshal.GetLastWin32Error());
                    try
                    {
                        if (!EscribirPt210(manejador, bytes, bytes.Length, out int escritos) || escritos != bytes.Length)
                            throw new InvalidOperationException("No se pudo enviar el ticket completo. Error " + Marshal.GetLastWin32Error());
                    }
                    finally { TerminarPaginaPt210(manejador); }
                }
                finally { TerminarDocumentoPt210(manejador); }
            }
            finally { CerrarImpresoraPt210(manejador); }
        }

        private void AbrirCategorias()
        {
            using (frmCategorias formulario =
                new frmCategorias(_categoriaBL, _estadoBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirEstados()
        {
            using (frmEstados formulario =
                new frmEstados(_estadoBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirRoles()
        {
            using (frmRoles formulario =
                new frmRoles(_rolBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirProveedores()
        {
            using (frmProveedores formulario =
                new frmProveedores(_proveedorBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirClientes()
        {
            using (frmClientes formulario =
                new frmClientes(_clienteBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirMarcas()
        {
            using (frmMarcas formulario =
                new frmMarcas(_marcaBL, _estadoBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirPuestos()
        {
            using (frmPuestos formulario =
                new frmPuestos(_puestoBL, _estadoBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void EnviarAlDashboard(object mensaje)
        {
            if (webViewMain.CoreWebView2 == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);

            webViewMain.CoreWebView2.PostWebMessageAsJson(json);
        }


        private void ActualizarProductoConFotografia(JsonElement datos)
        {
            if (!datos.TryGetProperty("ProductoId", out JsonElement campoId) ||
                !campoId.TryGetInt32(out int productoId) || productoId <= 0)
                throw new ArgumentException("Selecciona un producto válido.");

            Producto anterior = _productoBL.SelectAll()
                .FirstOrDefault(p => p.ProductoId == productoId)
                ?? throw new InvalidOperationException("El producto no existe en SQL Server.");

            Producto producto = datos.Deserialize<Producto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new ArgumentException("Los datos de edición del producto no son válidos.");

            if (producto.ProductoId != productoId)
                throw new ArgumentException("El identificador de edición es incorrecto.");

            producto.EstadoId = anterior.EstadoId;
            producto.Imagen = anterior.Imagen;
            producto.CodigoBarras = anterior.CodigoBarras;
            ValidarProducto(producto);
            ValidarEstadoMaestro(producto.EstadoId);

            string? rutaImagenCreada = null;
            try
            {
                if (datos.TryGetProperty("ImagenArchivo", out JsonElement imagenArchivo) &&
                    imagenArchivo.ValueKind != JsonValueKind.Null &&
                    imagenArchivo.ValueKind != JsonValueKind.Undefined)
                {
                    string nuevoNombre = GuardarFotografia(imagenArchivo);
                    producto.Imagen = nuevoNombre;
                    rutaImagenCreada = Path.Combine(CarpetaImagenes, nuevoNombre);
                }

                int resultado = _productoBL.Update(producto);
                if (resultado <= 0)
                    throw new InvalidOperationException("SQL Server no confirmó la actualización del producto.");
            }
            catch
            {
                if (rutaImagenCreada != null)
                {
                    try { File.Delete(rutaImagenCreada); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
                throw;
            }
        }

        private void ProcesarProductoIntegrado(string tipo, JsonElement raiz)
        {
            try
            {
                if (tipo == "productos:selectAll")
                {

                    var productos = _productoBL.SelectAll();
                    EnviarAlDashboard(new
                    {
                        type = "productos:list",
                        data = productos ?? new List<Producto>()
                    });
                }
                else if (tipo == "productos:guardarIntegrado")
                {
                    if (!raiz.TryGetProperty("data", out JsonElement datos) ||
                        datos.ValueKind != JsonValueKind.Object)
                        throw new ArgumentException("No se recibieron los datos del producto.");

                    Producto? producto = datos.Deserialize<Producto>(
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (producto == null)
                        throw new ArgumentException("El producto recibido no es válido.");

                    ValidarProducto(producto);


                    producto.Imagen = null;
                    string? rutaImagenCreada = null;
                    try
                    {
                        if (datos.TryGetProperty("ImagenArchivo", out JsonElement imagenArchivo)
                            && imagenArchivo.ValueKind != JsonValueKind.Null
                            && imagenArchivo.ValueKind != JsonValueKind.Undefined)
                        {
                            string nuevoNombre = GuardarFotografia(imagenArchivo);
                            producto.Imagen = nuevoNombre;
                            rutaImagenCreada = Path.Combine(CarpetaImagenes, nuevoNombre);
                        }

                        int resultado = _productoBL.Insert(producto);
                        if (resultado <= 0)
                            throw new InvalidOperationException("SQL Server no confirmó el registro del producto.");
                    }
                    catch
                    {

                        if (rutaImagenCreada != null)
                        {
                            try { File.Delete(rutaImagenCreada); }
                            catch (IOException) { }
                            catch (UnauthorizedAccessException) { }
                        }
                        throw;
                    }

                    EnviarAlDashboard(new { type = "productos:guardado" });
                }
            }
            catch (Exception ex)
            {

                EnviarAlDashboard(new
                {
                    type = "error",
                    message = "Productos: " + ex.Message
                });
            }
        }


        private static string GuardarFotografia(JsonElement archivo)
        {
            if (archivo.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("La fotografía recibida no es válida.");

            if (!archivo.TryGetProperty("TipoMime", out var tipoJson) ||
                tipoJson.ValueKind != JsonValueKind.String ||
                !archivo.TryGetProperty("Base64", out var base64Json) ||
                base64Json.ValueKind != JsonValueKind.String)
                throw new ArgumentException("Faltan los datos de la fotografía.");

            string tipoMime = tipoJson.GetString() ?? "";
            string base64 = base64Json.GetString() ?? "";
            string extension = tipoMime switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => throw new ArgumentException("Solo se permiten imágenes JPG, PNG o WebP.")
            };

            if (base64.Length == 0 || base64.Length > ((MaxImagenBytes + 2) / 3 * 4) + 8)
                throw new ArgumentException("La fotografía no puede superar los 5 MB.");

            byte[] bytes;
            try { bytes = Convert.FromBase64String(base64); }
            catch (FormatException)
            {
                throw new ArgumentException("El contenido de la fotografía no es válido.");
            }

            if (bytes.Length == 0 || bytes.Length > MaxImagenBytes)
                throw new ArgumentException("La fotografía no puede superar los 5 MB.");

            bool tipoCorrecto = tipoMime switch
            {
                "image/jpeg" => bytes.Length >= 3 && bytes[0] == 0xFF &&
                                bytes[1] == 0xD8 && bytes[2] == 0xFF,
                "image/png" => bytes.Length >= 8 &&
                               bytes[0] == 0x89 && bytes[1] == 0x50 &&
                               bytes[2] == 0x4E && bytes[3] == 0x47 &&
                               bytes[4] == 0x0D && bytes[5] == 0x0A &&
                               bytes[6] == 0x1A && bytes[7] == 0x0A,
                "image/webp" => bytes.Length >= 12 &&
                                bytes[0] == (byte)'R' && bytes[1] == (byte)'I' &&
                                bytes[2] == (byte)'F' && bytes[3] == (byte)'F' &&
                                bytes[8] == (byte)'W' && bytes[9] == (byte)'E' &&
                                bytes[10] == (byte)'B' && bytes[11] == (byte)'P',
                _ => false
            };
            if (!tipoCorrecto)
                throw new ArgumentException("El archivo no corresponde al formato de imagen seleccionado.");

            Directory.CreateDirectory(CarpetaImagenes);
            string nombre = Guid.NewGuid().ToString("N") + extension;
            string ruta = Path.Combine(CarpetaImagenes, nombre);
            using (FileStream destino = new FileStream(ruta, FileMode.CreateNew, FileAccess.Write))
                destino.Write(bytes, 0, bytes.Length);
            return nombre;
        }

        private void ProcesarCategoriaIntegrada(
            string tipo,
            JsonElement raiz)
        {
            try
            {
                switch (tipo)
                {
                    case "categorias:selectAll":

                        var categorias = _categoriaBL.SelectAll();

                        EnviarAlDashboard(new
                        {
                            type = "categorias:list",
                            data = categorias
                        });

                        break;

                    case "obtenerEstados":

                        var estados = _estadoBL.SelectAll();

                        EnviarAlDashboard(new
                        {
                            type = "estados",
                            data = estados
                        });

                        break;

                    case "categorias:guardarIntegrado":

                        JsonElement datos = raiz.GetProperty("data");

                        string nombre = (
                            datos.GetProperty("Nombre").GetString() ?? ""
                        ).Trim();

                        int estadoId = datos
                            .GetProperty("EstadoId")
                            .GetInt32();

                        if (string.IsNullOrWhiteSpace(nombre) ||
                            nombre.Length > 100)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Ingresa un nombre válido de hasta 100 caracteres."
                            });

                            return;
                        }

                        if (estadoId <= 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Selecciona un estado válido."
                            });

                            return;
                        }

                        Categoria categoria = new Categoria()
                        {
                            Nombre = nombre,
                            EstadoId = estadoId
                        };

                        int resultado = _categoriaBL.Insert(categoria);

                        if (resultado > 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "categorias:guardado"
                            });
                        }
                        else
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "No se pudo registrar la categoría."
                            });
                        }

                        break;


                    case "categorias:actualizarIntegrado":
                        {
                            if (!raiz.TryGetProperty("data", out JsonElement datosEdicion) ||
                                datosEdicion.ValueKind != JsonValueKind.Object)
                                throw new ArgumentException("No se recibieron los datos de la categoría.");

                            if (!datosEdicion.TryGetProperty("CategoriaId", out JsonElement idEdicion) ||
                                !idEdicion.TryGetInt32(out int categoriaIdEdicion) || categoriaIdEdicion <= 0)
                                throw new ArgumentException("Selecciona una categoría válida para editar.");

                            if (!datosEdicion.TryGetProperty("Nombre", out JsonElement nombreEdicionJson) ||
                                nombreEdicionJson.ValueKind != JsonValueKind.String)
                                throw new ArgumentException("Ingresa un nombre válido para la categoría.");

                            string nombreEdicion = (nombreEdicionJson.GetString() ?? "").Trim();

                            Categoria anteriorCategoria = _categoriaBL.SelectAll()
                                .FirstOrDefault(c => c.CategoriaId == categoriaIdEdicion)
                                ?? throw new InvalidOperationException("La categoría no existe en SQL Server.");
                            int estadoIdEdicion = anteriorCategoria.EstadoId;
                            ValidarEstadoMaestro(estadoIdEdicion);

                            Categoria categoriaEditada = new Categoria
                            {
                                CategoriaId = categoriaIdEdicion,
                                Nombre = nombreEdicion,
                                EstadoId = estadoIdEdicion
                            };


                            int filasEditadas = _categoriaBL.Update(categoriaEditada);
                            if (filasEditadas <= 0)
                                throw new InvalidOperationException("No se confirmó la actualización de la categoría.");

                            EnviarAlDashboard(new
                            {
                                type = "categorias:actualizada",
                                data = new { categoriaEditada.CategoriaId, categoriaEditada.Nombre, categoriaEditada.EstadoId }
                            });
                            break;
                        }


                    case "categorias:desactivarIntegrado":
                        {
                            if (!raiz.TryGetProperty("data", out JsonElement datosBaja) ||
                                datosBaja.ValueKind != JsonValueKind.Object ||
                                !datosBaja.TryGetProperty("CategoriaId", out JsonElement idBaja) ||
                                !idBaja.TryGetInt32(out int categoriaIdBaja) || categoriaIdBaja <= 0)
                                throw new ArgumentException("Selecciona una categoría válida para desactivar.");

                            Categoria anteriorCategoria = _categoriaBL.SelectAll()
                                .FirstOrDefault(c => c.CategoriaId == categoriaIdBaja)
                                ?? throw new InvalidOperationException("La categoría no existe en SQL Server.");
                            ValidarEstadoMaestro(anteriorCategoria.EstadoId);
                            int estadoEsperado = anteriorCategoria.EstadoId == 1 ? 2 : 1;

                            int estadoDevuelto = _categoriaBL.Delete(new Categoria
                            {
                                CategoriaId = categoriaIdBaja
                            });

                            if (estadoDevuelto != estadoEsperado)
                                throw new InvalidOperationException("SQL Server no confirmó el cambio de estado esperado.");

                            EnviarAlDashboard(new
                            {
                                type = "categorias:desactivada",
                                data = new { CategoriaId = categoriaIdBaja, EstadoId = estadoDevuelto }
                            });
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }


        private void ProcesarSubcategoriaIntegrada(
            string tipo,
            JsonElement raiz)
        {
            try
            {
                switch (tipo)
                {

                    case "subcategorias:selectAll":
                        {
                            var subcategorias = _subcategoriaBL.SelectAll();
                            var categorias = _categoriaBL.SelectAll();

                            var listado = subcategorias.Select(s => new
                            {
                                s.SubcategoriaId,
                                s.CategoriaId,
                                s.Nombre,
                                s.EstadoId,
                                CategoriaNombre = categorias
                                    .FirstOrDefault(c => c.CategoriaId == s.CategoriaId)
                                    ?.Nombre ?? ""
                            }).ToList();

                            EnviarAlDashboard(new
                            {
                                type = "subcategorias:list",
                                data = listado
                            });

                            break;
                        }


                    case "subcategorias:guardarIntegrado":
                        JsonElement datos = raiz.GetProperty("data");

                        string nombre = (ObtenerTextoCliente(datos, "Nombre") ?? "").Trim();
                        int categoriaId = datos.GetProperty("CategoriaId").GetInt32();
                        int estadoId = datos.GetProperty("EstadoId").GetInt32();

                        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 120)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Ingresa un nombre válido de hasta 120 caracteres."
                            });
                            return;
                        }

                        if (categoriaId <= 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Selecciona una categoría válida."
                            });
                            return;
                        }

                        if (estadoId <= 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Selecciona un estado válido."
                            });
                            return;
                        }

                        Subcategoria subcategoria = new Subcategoria()
                        {
                            CategoriaId = categoriaId,
                            Nombre = nombre,
                            EstadoId = estadoId
                        };

                        int resultado = _subcategoriaBL.Insert(subcategoria);

                        if (resultado > 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "subcategorias:guardado"
                            });
                        }
                        else
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "No se pudo registrar la subcategoría."
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }

        private void ProcesarClienteIntegrado(
            string tipo,
            JsonElement raiz)
        {
            try
            {
                switch (tipo)
                {
                    case "clientes:selectAll":

                        var clientes = _clienteBL.SelectAll();

                        EnviarAlDashboard(new
                        {
                            type = "clientes:list",
                            data = clientes
                        });

                        break;

                    case "clientes:guardarIntegrado":

                        JsonElement datos = raiz.GetProperty("data");

                        string nombre = ObtenerTextoCliente(datos, "Nombre") ?? "";
                        string apellido = ObtenerTextoCliente(datos, "Apellido") ?? "";
                        string dui = ObtenerTextoCliente(datos, "DUI") ?? "";
                        string? nit = ObtenerTextoCliente(datos, "NIT");
                        string? telefono = ObtenerTextoCliente(datos, "Telefono");
                        string? correo = ObtenerTextoCliente(datos, "Correo");
                        string? direccion = ObtenerTextoCliente(datos, "Direccion");

                        if (string.IsNullOrWhiteSpace(nombre) ||
                            string.IsNullOrWhiteSpace(apellido) ||
                            string.IsNullOrWhiteSpace(dui) ||
                            string.IsNullOrWhiteSpace(telefono) ||
                            string.IsNullOrWhiteSpace(correo))
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Nombre, apellido, DUI, teléfono y correo son obligatorios."
                            });
                            return;
                        }

                        if (nombre.Length > 100 ||
                            apellido.Length > 100 ||
                            dui.Length > 10 ||
                            (nit?.Length ?? 0) > 20 ||
                            (telefono?.Length ?? 0) > 25 ||
                            (correo?.Length ?? 0) > 150 ||
                            (direccion?.Length ?? 0) > 50)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Uno o más campos superan la longitud permitida."
                            });
                            return;
                        }

                        if (correo != null &&
                            (!System.Net.Mail.MailAddress.TryCreate(correo, out var email) ||
                             email.Address != correo))
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Ingresa un correo electrónico válido."
                            });
                            return;
                        }

                        Cliente cliente = new Cliente()
                        {
                            Nombre = nombre,
                            Apellido = apellido,
                            DUI = dui,
                            NIT = nit,
                            Telefono = telefono,
                            Correo = correo,
                            Direccion = direccion
                        };

                        int resultado = _clienteBL.Insert(cliente);

                        if (resultado > 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "clientes:guardado"
                            });
                        }
                        else
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "No se pudo registrar el cliente."
                            });
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }

        private void ProcesarMarcaIntegrada(string tipo, JsonElement raiz)
        {
            try
            {
                switch (tipo)
                {
                    case "marcas:selectAll":
                        EnviarAlDashboard(new
                        {
                            type = "marcas:list",
                            data = _marcaBL.SelectAll()
                        });
                        break;

                    case "marcas:guardarIntegrado":
                        if (!raiz.TryGetProperty("data", out JsonElement datos) ||
                            datos.ValueKind != JsonValueKind.Object ||
                            !datos.TryGetProperty("Nombre", out JsonElement nombreJson) ||
                            nombreJson.ValueKind != JsonValueKind.String)
                            throw new ArgumentException("Ingresa un nombre válido para la marca.");

                        string nombre = (nombreJson.GetString() ?? string.Empty).Trim();
                        if (nombre.Length == 0 || nombre.Length > 100)
                            throw new ArgumentException("El nombre de la marca debe tener de 1 a 100 caracteres.");

                        int resultado = _marcaBL.Insert(new Marca
                        {
                            Nombre = nombre,
                            EstadoId = 1
                        });

                        if (resultado <= 0)
                            throw new InvalidOperationException("SQL Server no confirmó el registro de la marca.");

                        EnviarAlDashboard(new { type = "marcas:guardado" });
                        break;
                }
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new
                {
                    type = "marcas:error",
                    message = ex.Message
                });
            }
        }

        private void ProcesarProveedorIntegrado(string tipo, JsonElement raiz)
        {
            try
            {
                switch (tipo)
                {
                    case "proveedores:selectAll":
                        var proveedores = _proveedorBL.SelectAll();
                        EnviarAlDashboard(new
                        {
                            type = "proveedores:list",
                            data = proveedores
                        });
                        break;

                    case "proveedores:guardarIntegrado":
                        JsonElement datos = raiz.GetProperty("data");

                        string nombreEmpresa = ObtenerTextoCliente(datos, "NombreEmpresa") ?? "";
                        string nit = ObtenerTextoCliente(datos, "NIT") ?? "";
                        string telefono = ObtenerTextoCliente(datos, "Telefono") ?? "";
                        string correo = ObtenerTextoCliente(datos, "Correo") ?? "";
                        string contacto = ObtenerTextoCliente(datos, "Contacto") ?? "";

                        if (string.IsNullOrWhiteSpace(nombreEmpresa) ||
                            string.IsNullOrWhiteSpace(nit) ||
                            string.IsNullOrWhiteSpace(telefono) ||
                            string.IsNullOrWhiteSpace(correo) ||
                            string.IsNullOrWhiteSpace(contacto))
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Completa todos los campos del proveedor."
                            });
                            return;
                        }

                        if (nombreEmpresa.Length > 100 ||
                            nit.Length > 20 ||
                            telefono.Length > 25 ||
                            correo.Length > 150 ||
                            contacto.Length > 100)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Uno o más campos superan la longitud permitida."
                            });
                            return;
                        }

                        if (!System.Net.Mail.MailAddress.TryCreate(correo, out var email) ||
                            email.Address != correo)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Ingresa un correo electrónico válido."
                            });
                            return;
                        }

                        Proveedor proveedor = new Proveedor()
                        {
                            NombreEmpresa = nombreEmpresa,
                            NIT = nit,
                            Telefono = telefono,
                            Correo = correo,
                            Contacto = contacto
                        };

                        int resultado = _proveedorBL.Insert(proveedor);

                        if (resultado > 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "proveedores:guardado"
                            });
                        }
                        else
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "No se pudo registrar el proveedor."
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }

        private void ProcesarPuestoIntegrado(string tipo, JsonElement raiz)
        {
            try
            {
                switch (tipo)
                {
                    case "puestos:selectAll":
                        var puestos = _puestoBL.SelectAll();
                        EnviarAlDashboard(new
                        {
                            type = "puestos:list",
                            data = puestos
                        });
                        break;

                    case "puestos:guardarIntegrado":
                        JsonElement datos = raiz.GetProperty("data");
                        string nombre = (ObtenerTextoCliente(datos, "Nombre") ?? "").Trim();
                        string descripcion = (ObtenerTextoCliente(datos, "Descripcion") ?? "").Trim();

                        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
                            throw new ArgumentException("Ingresa un nombre válido de hasta 100 caracteres.");

                        if (string.IsNullOrWhiteSpace(descripcion) || descripcion.Length > 500)
                            throw new ArgumentException("Ingresa una descripción válida de hasta 500 caracteres.");

                        Puesto puesto = new Puesto()
                        {
                            Nombre = nombre,
                            Descripcion = descripcion,
                            EstadoId = 1
                        };

                        int resultado = _puestoBL.Insert(puesto);

                        if (resultado > 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "puestos:guardado"
                            });
                        }
                        else
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "No se pudo registrar el puesto."
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }

        private void ProcesarEmpleadoIntegrado(string tipo, JsonElement raiz)
        {
            try
            {
                if (tipo == "empleados:selectAll")
                {
                    EnviarAlDashboard(new
                    {
                        type = "empleados:list",
                        data = _empleadoBL.SelectAll()
                    });
                    return;
                }

                if (tipo != "empleados:guardarIntegrado") return;

                if (!raiz.TryGetProperty("data", out JsonElement datos) ||
                    datos.ValueKind != JsonValueKind.Object)
                    throw new ArgumentException("No se recibieron los datos del empleado.");

                Empleado empleado = datos.Deserialize<Empleado>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new ArgumentException("Los datos del empleado no son válidos.");

                empleado.EstadoId = 1;
                ValidarEmpleado(empleado);

                int resultado = _empleadoBL.Insert(empleado);
                if (resultado <= 0)
                    throw new InvalidOperationException("SQL Server no confirmó el registro del empleado.");

                EnviarAlDashboard(new { type = "empleados:guardado" });
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new { type = "empleados:error", message = ex.Message });
            }
        }

        private void ProcesarRolIntegrado(string tipo, JsonElement raiz)
        {
            try
            {
                switch (tipo)
                {
                    case "roles:selectAll":
                        var roles = _rolBL.SelectAll();
                        EnviarAlDashboard(new
                        {
                            type = "roles:list",
                            data = roles
                        });
                        break;

                    case "roles:guardarIntegrado":
                        JsonElement datos = raiz.GetProperty("data");
                        string nombre = ObtenerTextoCliente(datos, "Nombre") ?? "";

                        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Ingresa un nombre válido de hasta 100 caracteres."
                            });
                            return;
                        }

                        Rol rol = new Rol()
                        {
                            Nombre = nombre
                        };

                        int resultado = _rolBL.Insert(rol);

                        if (resultado > 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "roles:guardado"
                            });
                        }
                        else
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "No se pudo registrar el rol."
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }


        private void ProcesarModeloIntegrado(string accion, JsonElement raiz)
        {
            if (accion == "selectAll")
            {
                EnviarAlDashboard(new
                {
                    type = "modelos:list",
                    data = _modeloBL.SelectAll()
                });
                return;
            }

            if (accion != "guardarIntegrado") return;

            if (!raiz.TryGetProperty("data", out JsonElement datos) ||
                datos.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("No se recibieron los datos del modelo.");

            Modelo modelo = datos.Deserialize<Modelo>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new ArgumentException("Los datos del modelo no son válidos.");

            ValidarModelo(modelo);
            if (!_marcaBL.SelectAll().Any(m => m.MarcaId == modelo.MarcaId && m.EstadoId == 1))
                throw new ArgumentException("Selecciona una marca activa.");

            modelo.EstadoId = 1;
            int resultado = _modeloBL.Insert(modelo);
            if (resultado <= 0)
                throw new InvalidOperationException("SQL Server no confirmó el registro del modelo.");

            EnviarAlDashboard(new { type = "modelos:guardado" });
        }

        private void ProcesarModuloAdicional(string tipo, JsonElement raiz)
        {
            const string consulta = ":selectAll";
            const string guardar = ":guardarIntegrado";

            string accion;
            string modulo;

            if (tipo.EndsWith(consulta, StringComparison.Ordinal))
            {
                accion = "selectAll";
                modulo = tipo.Substring(0, tipo.Length - consulta.Length);
            }
            else if (tipo.EndsWith(guardar, StringComparison.Ordinal))
            {
                accion = "guardarIntegrado";
                modulo = tipo.Substring(0, tipo.Length - guardar.Length);
            }
            else
            {
                return;
            }

            try
            {
                switch (modulo)
                {
                    case "productos":
                        ProcesarEntidad<Producto>(modulo, accion, raiz,
                            _productoBL.SelectAll, _productoBL.Insert, ValidarProducto);
                        break;

                    case "modelos":
                        ProcesarModeloIntegrado(accion, raiz);
                        break;

                    case "vehiculos":
                        ProcesarEntidad<Vehiculo>(modulo, accion, raiz,
                            _vehiculoBL.SelectAll, _vehiculoBL.Insert, ValidarVehiculo);
                        break;

                    case "compatibilidad":
                        ProcesarEntidad<CompatibilidadRepuesto>(modulo, accion, raiz,
                            _compatibilidadBL.SelectAll, _compatibilidadBL.Insert, ValidarCompatibilidad);
                        break;

                    case "inventario":


                        ProcesarEntidad<Inventario>(modulo, accion, raiz,
                            _inventarioBL.SelectAll, _inventarioBL.Insert, ValidarInventario);
                        break;


                    case "compras":
                        ProcesarEntidad<Compra>(modulo, accion, raiz, _compraBL.SelectAll);
                        break;

                    case "detalles-compra":
                        ProcesarEntidad<Detalle_Compra>(modulo, accion, raiz, _detalleCompraBL.SelectAll);
                        break;

                    case "ventas":
                        ProcesarEntidad<Venta>(modulo, accion, raiz, _ventaBL.SelectAll);
                        break;

                    case "detalles-venta":
                        ProcesarEntidad<Detalle_Venta>(modulo, accion, raiz, _detalleVentaBL.SelectAll);
                        break;

                    case "garantias":
                        ProcesarEntidad<Garantia>(modulo, accion, raiz, _garantiaBL.SelectAll);
                        break;

                    case "reemplazos-garantia":
                        ProcesarEntidad<ReemplazoGarantia>(modulo, accion, raiz, _reemplazoGarantiaBL.SelectAll);
                        break;

                    case "usuarios":
                        if (accion == "selectAll")
                        {

                            var usuarios = _usuarioBL.SelectAll().Select(u => new
                            {
                                u.UsuarioId,
                                u.NombreUsuario,
                                u.EmpleadoId,
                                u.RolId,
                                u.EstadoId
                            }).ToList();

                            EnviarAlDashboard(new
                            {
                                type = "usuarios:list",
                                data = usuarios
                            });
                        }
                        else if (accion == "guardarIntegrado")
                        {
                            RegistrarUsuarioIntegrado(raiz);
                        }
                        break;

                    default:
                        EnviarAlDashboard(new
                        {
                            type = "error",
                            message = "El módulo solicitado no está registrado en MainForm."
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                EnviarAlDashboard(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }


        private void RegistrarUsuarioIntegrado(JsonElement raiz)
        {
            if (_sesionActual == null || !PermisosSistema.EsAdministrador(_sesionActual))
                throw new UnauthorizedAccessException("Solo un administrador puede crear usuarios.");

            if (!raiz.TryGetProperty("data", out JsonElement datos) ||
                datos.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("No se recibieron los datos del usuario.");

            int LeerId(string nombre)
            {
                if (!datos.TryGetProperty(nombre, out JsonElement elemento) ||
                    elemento.ValueKind != JsonValueKind.Number ||
                    !elemento.TryGetInt32(out int valor) || valor <= 0)
                    throw new ArgumentException("Selecciona un valor válido para " + nombre + ".");
                return valor;
            }

            int empleadoId = LeerId("EmpleadoId");
            int rolId = LeerId("RolId");
            const int estadoId = 1;
            string nombreUsuario = (ObtenerTextoCliente(datos, "NombreUsuario") ?? "").Trim();


            if (!datos.TryGetProperty("Contrasena", out JsonElement campoClave) ||
                campoClave.ValueKind != JsonValueKind.String)
                throw new ArgumentException("Ingresa una contraseña válida.");
            string contrasena = campoClave.GetString() ?? "";

            if (nombreUsuario.Length < 3 || nombreUsuario.Length > 50 ||
                !System.Text.RegularExpressions.Regex.IsMatch(
                    nombreUsuario, @"\A[A-Za-z0-9._-]+\z"))
                throw new ArgumentException("El usuario debe tener de 3 a 50 caracteres: letras, números, punto, guion o guion bajo.");

            if (contrasena.Length < 8 || contrasena.Length > 72 ||
                string.IsNullOrWhiteSpace(contrasena))
                throw new ArgumentException("La contraseña debe tener de 8 a 72 caracteres.");

            if (!_empleadoBL.SelectAll().Any(e => e.EmpleadoId == empleadoId && e.EstadoId == 1))
                throw new ArgumentException("Selecciona un empleado activo.");
            if (!_rolBL.SelectAll().Any(r => r.RolId == rolId && r.EstadoId == 1))
                throw new ArgumentException("Selecciona un rol activo.");


            var existentes = _usuarioBL.SelectAll();
            if (existentes.Any(u => u.EmpleadoId == empleadoId))
                throw new ArgumentException("Este empleado ya tiene una cuenta.");
            if (existentes.Any(u => string.Equals(u.NombreUsuario, nombreUsuario,
                    StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Ese nombre de usuario ya está registrado.");


            var nuevo = new Usuario
            {
                EmpleadoId = empleadoId,
                RolId = rolId,
                EstadoId = estadoId,
                NombreUsuario = nombreUsuario,
                Contrasena = contrasena
            };

            try
            {
                int filas = _usuarioBL.Insert(nuevo);
                if (filas <= 0)
                    throw new InvalidOperationException("SQL Server no confirmó el registro del usuario.");
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                throw new ArgumentException("El usuario o empleado ya tiene una cuenta registrada.");
            }

            EnviarAlDashboard(new { type = "usuarios:guardado" });
        }


        private void ProcesarEntidad<T>(
            string modulo,
            string accion,
            JsonElement raiz,
            Func<List<T>> seleccionar,
            Func<T, int>? insertar = null,
            Action<T>? validar = null) where T : class
        {
            if (accion == "selectAll")
            {
                var registros = seleccionar();
                EnviarAlDashboard(new
                {
                    type = modulo + ":list",
                    data = registros
                });
                return;
            }

            if (accion != "guardarIntegrado") return;

            if (insertar == null)
            {
                EnviarAlDashboard(new
                {
                    type = "error",
                    message = "Este módulo requiere un proceso completo de registro con sus detalles y actualización de inventario. La consulta sí está disponible."
                });
                return;
            }

            if (!raiz.TryGetProperty("data", out JsonElement datos) ||
                datos.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("No se recibieron datos válidos del formulario.");
            }

            var opciones = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            T entidad = datos.Deserialize<T>(opciones)
                ?? throw new ArgumentException("Los datos del registro están incompletos.");


            validar?.Invoke(entidad);

            int resultado = insertar(entidad);
            if (resultado <= 0)
            {
                throw new InvalidOperationException("No se pudo guardar el registro en SQL Server.");
            }

            EnviarAlDashboard(new { type = modulo + ":guardado" });
        }


        private static void ValidarProducto(Producto p)
        {
            RequerirTexto(p.CodigoOEM, 50, "código OEM");
            RequerirTexto(p.Nombre, 100, "nombre del producto");
            RequerirTexto(p.Descripcion, 500, "descripción");
            RequerirId(p.SubcategoriaId, "subcategoría");
            RequerirId(p.EstadoId, "estado");

            if (p.PrecioVenta <= 0 || p.PrecioVenta != decimal.Round(p.PrecioVenta, 2))
                throw new ArgumentException("El precio debe ser mayor que cero y tener hasta dos decimales.");

            if (p.DiasGarantia < 0 || p.DiasGarantia > 36500)
                throw new ArgumentException("Los días de garantía deben estar entre 0 y 36500.");
        }

        private static void ValidarModelo(Modelo modelo)
        {
            RequerirTexto(modelo.Nombre, 100, "nombre del modelo");
            RequerirId(modelo.MarcaId, "marca");
        }

        private static void ValidarVehiculo(Vehiculo vehiculo)
        {
            RequerirId(vehiculo.ModeloId, "modelo");
            RequerirTexto(vehiculo.TipoMotor, 100, "tipo de motor");

            if (vehiculo.Anio < 1886 || vehiculo.Anio > DateTime.Now.Year + 2)
                throw new ArgumentException("Ingresa un año de vehículo válido.");

            if (vehiculo.Cilindrada <= 0 || vehiculo.Cilindrada > 999.99m ||
                vehiculo.Cilindrada != decimal.Round(vehiculo.Cilindrada, 2))
                throw new ArgumentException("La cilindrada debe ser positiva y tener hasta dos decimales.");
        }

        private static void ValidarCompatibilidad(CompatibilidadRepuesto relacion)
        {
            RequerirId(relacion.ProductoId, "producto");
            if (!relacion.VehiculoId.HasValue)
                throw new ArgumentException("Selecciona un vehículo compatible.");
            RequerirId(relacion.VehiculoId.Value, "vehículo");
        }

        private static void ValidarInventario(Inventario inventario)
        {
            RequerirId(inventario.ProductoId, "producto");
            RequerirId(inventario.EstadoId, "estado");
            if (inventario.StockActual < 0 || inventario.StockMinimo < 0 ||
                inventario.StockMaximo < inventario.StockMinimo)
                throw new ArgumentException("Verifica el stock actual, mínimo y máximo.");
        }

        private static void ValidarEmpleado(Empleado empleado)
        {
            RequerirId(empleado.PuestoId, "puesto");
            RequerirTexto(empleado.Nombre, 100, "nombre");
            RequerirTexto(empleado.Apellido, 100, "apellido");
            RequerirTexto(empleado.DUI, 10, "DUI");
            RequerirTexto(empleado.Telefono, 25, "teléfono");
            RequerirTexto(empleado.Correo, 150, "correo electrónico");

            if (!System.Net.Mail.MailAddress.TryCreate(empleado.Correo, out var correo) ||
                correo.Address != empleado.Correo)
                throw new ArgumentException("Ingresa un correo electrónico válido.");
        }

        private static void RequerirId(int id, string campo)
        {
            if (id <= 0)
                throw new ArgumentException("Selecciona un valor válido para " + campo + ".");
        }

        private static void RequerirTexto(string? texto, int maximo, string campo)
        {
            if (string.IsNullOrWhiteSpace(texto) || texto.Trim().Length > maximo)
                throw new ArgumentException("Ingresa un " + campo + " válido (máximo " + maximo + " caracteres).");
        }

        private static string? ObtenerTextoCliente(JsonElement datos, string propiedad)
        {
            if (!datos.TryGetProperty(propiedad, out JsonElement valor) ||
                valor.ValueKind == JsonValueKind.Null ||
                valor.ValueKind == JsonValueKind.Undefined)
                return null;

            if (valor.ValueKind != JsonValueKind.String)
                throw new ArgumentException("El campo " + propiedad + " debe ser texto.");

            string texto = (valor.GetString() ?? "").Trim();
            return texto.Length == 0 ? null : texto;
        }

        private void webView21_Click(
            object sender,
            EventArgs e)
        {

        }

        private void webViewMain_Click(
            object sender,
            EventArgs e)
        {

        }

    }
}