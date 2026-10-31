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
                // Solo las fotos del catálogo se exponen a WebView2.
                Directory.CreateDirectory(CarpetaImagenes);
                webViewMain.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    HostImagenes,
                    CarpetaImagenes,
                    CoreWebView2HostResourceAccessKind.DenyCors);
                webViewMain.CoreWebView2.WebMessageReceived += WebViewMain_WebMessageReceived;
                webViewMain.CoreWebView2.NavigationStarting += (s, args) =>
                {
                    // No permitir que la interfaz navegue a sitios externos.
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

                // Nunca atender operaciones del Dashboard antes de iniciar sesión.
                if (_sesionActual == null)
                {
                    if (tipo == "auth:login" && PaginaActualEs("login.html"))
                        await ProcesarLoginAsync(raiz);
                    return;
                }

                // Después de autenticarse, solo el Dashboard puede mandar operaciones.
                if (!PaginaActualEs("index.html")) return;
                if (tipo == "auth:logout")
                {
                    _sesionActual = null;
                    NavegarPagina("login.html");
                    return;
                }
                if (tipo == "auth:sesionSolicitar")
                {
                    EnviarSesion();
                    return;
                }
                if (tipo == "auth:login") return;

                if (!PermisosSistema.EsAdministrador(_sesionActual))
                {
                    EnviarAlDashboard(new
                    {
                        type = "error",
                        message = "No tienes permisos para realizar esta operación."
                    });

                    return;
                }

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

                    // DASHBOARD: resumen de ventas e inventario en modo solo lectura.
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

                    // COMPRAS: registra cabecera, detalles y aumento de stock una sola vez.
                    // La transaccion atomica se ejecuta en dbo.spCompraRegistrarCompleta.
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

                            // No confiar en totales enviados por el HTML. CompraBL y SQL
                            // validan y recalculan todos los importes desde las lineas.
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
                            // Un error SQL se revierte en el procedimiento; no
                            // efectuar aqui otra insercion ni actualizar stock.
                            EnviarAlDashboard(new
                            {
                                type = "compras:error",
                                message = "Compra no registrada: " + ex.Message
                            });
                        }
                        break;

                    // PASO 4: Confirmación de ventas completas (factura, detalles y stock).
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

                    // PT-210: imprimir una venta existente. NO registra ni descuenta inventario.
                    case "ventas:imprimirTicket":
                        try
                        {
                            if (!raiz.TryGetProperty("data", out JsonElement datosTicket) ||
                                datosTicket.ValueKind != JsonValueKind.Object ||
                                !datosTicket.TryGetProperty("VentaId", out JsonElement idTicketJson) ||
                                !idTicketJson.TryGetInt32(out int idVentaTicket) || idVentaTicket <= 0)
                                throw new ArgumentException("Selecciona una venta registrada válida.");

                            // Recuperar la factura y sus líneas desde SQL, nunca desde HTML.
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

                    // PRODUCTOS: consulta y registro desde WebView2.
                    case "productos:selectAll":
                    case "productos:guardarIntegrado":
                        ProcesarProductoIntegrado(tipo, raiz);
                        break;

                    case "categorias:selectAll":
                    case "obtenerEstados":
                    case "categorias:guardarIntegrado":
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

                    // Todas las demás entidades se atienden desde un solo
                    // procesador. No hay que agregar más cases aquí.
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

        // Datos reales para las dos tablas del Dashboard.
        // Esta consulta no registra movimientos ni modifica existencias.
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

            // app.js espera [factura, cliente, fecha, total, estado].
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

            // app.js espera [simbolo, nombre, detalle, sku, actual, minimo].
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

        // ==========================================================
        // IMPRESIÓN PT-210 / Windows Generic-Text Only / papel de 57 mm
        // 32 columnas ASCII: compatible con impresión RAW de texto.
        // Los datos se recuperan de las tablas existentes mediante BL.
        // ==========================================================
        private const string NombreImpresoraPt210 = "AutoParts PT-210";
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
            AgregarCentradoPt210(t, "AUTOPARTS");
            AgregarCentradoPt210(t, "REPUESTOS Y ACCESORIOS");
            AgregarCentradoPt210(t, "COMPROBANTE INTERNO");
            t.AppendLine(new string('-', AnchoTicketPt210));
            AgregarPartidoPt210(t, "Factura: " + venta.NumeroFactura);
            t.AppendLine("Venta: " + venta.VentaId);
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
            AgregarCentradoPt210(t, "GRACIAS POR SU COMPRA!");
            AgregarCentradoPt210(t, "Conserve su comprobante");
            t.AppendLine();
            t.AppendLine();
            t.AppendLine();

            // El controlador Generic / Text Only ya fue probado en Windows.
            // RAW envía solamente texto ASCII; no necesita comandos ESC/POS.
            byte[] datos = Encoding.ASCII.GetBytes(t.ToString().Replace("\n", "\r\n"));
            EnviarTextoCrudoPt210(NombreImpresoraPt210, datos);
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

        // ==========================================================
        // PRODUCTOS - LISTADO / NUEVO REGISTRO
        // ==========================================================
        private void ProcesarProductoIntegrado(string tipo, JsonElement raiz)
        {
            try
            {
                if (tipo == "productos:selectAll")
                {
                    // Devuelve la lista real, incluida una lista vacía si no hay productos.
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

                    // Nunca aceptar rutas de archivos suministradas por el HTML.
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
                        // Si SQL Server falla, no dejamos la fotografía huérfana.
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
                // El HTML muestra la causa, sin quedarse en «Cargando...».
                EnviarAlDashboard(new
                {
                    type = "error",
                    message = "Productos: " + ex.Message
                });
            }
        }


        // Guardar una sola fotografía principal en la carpeta privada de AutoParts.
        // ImagenArchivo = { TipoMime: "image/jpeg|png|webp", Base64: "..." }.
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


        // ==========================================
        // SUBCATEGORÍAS - MÓDULO INTEGRADO
        // ==========================================
        private void ProcesarSubcategoriaIntegrada(
            string tipo,
            JsonElement raiz)
        {
            try
            {
                switch (tipo)
                {
                    case "subcategorias:selectAll":
                        var subcategorias = _subcategoriaBL.SelectAll();

                        EnviarAlDashboard(new
                        {
                            type = "subcategorias:list",
                            data = subcategorias
                        });
                        break;

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
                        var marcas = _marcaBL.SelectAll();
                        EnviarAlDashboard(new
                        {
                            type = "marcas:list",
                            data = marcas
                        });
                        break;

                    case "marcas:guardarIntegrado":
                        JsonElement datos = raiz.GetProperty("data");

                        string nombre = (datos.GetProperty("Nombre").GetString() ?? "").Trim();
                        int estadoId = datos.GetProperty("EstadoId").GetInt32();

                        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
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

                        Marca marca = new Marca()
                        {
                            Nombre = nombre,
                            EstadoId = estadoId
                        };

                        int resultado = _marcaBL.Insert(marca);

                        if (resultado > 0)
                        {
                            EnviarAlDashboard(new
                            {
                                type = "marcas:guardado"
                            });
                        }
                        else
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "No se pudo registrar la marca."
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
                        string nombre = ObtenerTextoCliente(datos, "Nombre") ?? "";
                        string? descripcion = ObtenerTextoCliente(datos, "Descripcion");
                        int estadoId = datos.GetProperty("EstadoId").GetInt32();

                        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
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

                        Puesto puesto = new Puesto()
                        {
                            Nombre = nombre,
                            Descripcion = descripcion,
                            EstadoId = estadoId
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

        // ==========================================================
        // MÓDULOS ADICIONALES DEL DASHBOARD
        // Estas claves coinciden con categorias-integradas_21_modulos.js.
        // Las consultas quedan listas. Los módulos transaccionales
        // solo permiten consultar hasta que existan operaciones
        // atómicas en BusinessLogic / DataAccess.
        // ==========================================================
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
                return; // No intercepta otras funciones del Dashboard.
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
                        ProcesarEntidad<Modelo>(modulo, accion, raiz,
                            _modeloBL.SelectAll, _modeloBL.Insert, ValidarModelo);
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
                        // Alta inicial de existencias; movimientos de stock
                        // por compras, ventas o garantías necesitan transacción.
                        ProcesarEntidad<Inventario>(modulo, accion, raiz,
                            _inventarioBL.SelectAll, _inventarioBL.Insert, ValidarInventario);
                        break;

                    case "empleados":
                        ProcesarEntidad<Empleado>(modulo, accion, raiz,
                            _empleadoBL.SelectAll, _empleadoBL.Insert, ValidarEmpleado);
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
                            // Nunca enviar contraseñas o hashes al HTML.
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

        // Registro de usuarios desde el Dashboard. Las contraseñas solo se usan
        // en memoria para crear el hash dentro de UsuarioBL.Insert().
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
            int estadoId = LeerId("EstadoId");
            string nombreUsuario = (ObtenerTextoCliente(datos, "NombreUsuario") ?? "").Trim();

            // No registrar ni mostrar jamás la contraseña recibida en errores o logs.
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

            if (!_empleadoBL.SelectAll().Any(e => e.EmpleadoId == empleadoId))
                throw new ArgumentException("El empleado seleccionado no existe.");
            if (!_rolBL.SelectAll().Any(r => r.RolId == rolId))
                throw new ArgumentException("El rol seleccionado no existe.");
            if (!_estadoBL.SelectAll().Any(e => e.EstadoId == estadoId))
                throw new ArgumentException("El estado seleccionado no existe.");

            // Comprobación amigable; las restricciones UNIQUE de SQL Server
            // siguen siendo la protección definitiva ante registros simultáneos.
            var existentes = _usuarioBL.SelectAll();
            if (existentes.Any(u => u.EmpleadoId == empleadoId))
                throw new ArgumentException("Este empleado ya tiene una cuenta.");
            if (existentes.Any(u => string.Equals(u.NombreUsuario, nombreUsuario,
                    StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException("Ese nombre de usuario ya está registrado.");

            // UsuarioBL.Insert genera PasswordHelper.CrearHash; no hashear aquí.
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

        // Comportamiento compartido para listados y registros básicos.
        // Para compras, ventas y garantías Insert no se expone aquí:
        // sus cabeceras, detalles y stock deben procesarse juntos.
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

            // Las validaciones obligatorias se ejecutan en C#.
            validar?.Invoke(entidad);

            int resultado = insertar(entidad);
            if (resultado <= 0)
            {
                throw new InvalidOperationException("No se pudo guardar el registro en SQL Server.");
            }

            EnviarAlDashboard(new { type = modulo + ":guardado" });
        }

        // ==========================================================
        // VALIDACIONES DE ENTIDADES QUE SE PUEDEN GUARDAR DE FORMA
        // INDIVIDUAL DESDE LOS HTML DEL DASHBOARD
        // ==========================================================
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
            RequerirId(modelo.EstadoId, "estado");
        }

        private static void ValidarVehiculo(Vehiculo vehiculo)
        {
            RequerirId(vehiculo.ModeloId, "modelo");
            RequerirId(vehiculo.EstadoId, "estado");
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
            RequerirId(empleado.EstadoId, "estado");
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