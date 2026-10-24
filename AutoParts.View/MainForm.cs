using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoParts.Entity;
using AutoParts.Entity.Productos;

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
                    int resultado = _productoBL.Insert(producto);

                    if (resultado <= 0)
                        throw new InvalidOperationException("SQL Server no confirmó el registro del producto.");

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
                        else
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "El registro de usuarios requiere validación de permisos y hash seguro de contraseña."
                            });
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