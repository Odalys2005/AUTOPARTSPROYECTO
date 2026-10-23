using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using AutoParts.Entity;

namespace AutoParts.View
{
    public partial class MainForm : Form
    {
        private readonly CategoriaBL _categoriaBL;
        private readonly EstadoBL _estadoBL;
        private readonly RolBL _rolBL;
        private readonly ProveedorBL _proveedorBL;
        private readonly ClienteBL _clienteBL;
        private readonly MarcaBL _marcaBL;
        private readonly PuestoBL _puestoBL;

        public MainForm(
            CategoriaBL categoriaBL,
            EstadoBL estadoBL,
            RolBL rolBL,
            ProveedorBL proveedorBL,
            ClienteBL clienteBL,
            MarcaBL marcaBL,
            PuestoBL puestoBL)
        {
            InitializeComponent();

            _categoriaBL = categoriaBL;
            _estadoBL = estadoBL;
            _rolBL = rolBL;
            _proveedorBL = proveedorBL;
            _clienteBL = clienteBL;
            _marcaBL = marcaBL;
            _puestoBL = puestoBL;

            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            webViewMain.Dock = DockStyle.Fill;

            Load += MainForm_Load;
        }

        private async void MainForm_Load(
            object? sender,
            EventArgs e)
        {
            try
            {
                await webViewMain.EnsureCoreWebView2Async();

                webViewMain.CoreWebView2.WebMessageReceived +=
                    WebViewMain_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "index.html"
                );

                if (!File.Exists(ruta))
                {
                    MessageBox.Show(
                        "No se encontró el Dashboard:\n" + ruta,
                        "AutoParts",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return;
                }

                webViewMain.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar el Dashboard:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void WebViewMain_WebMessageReceived(
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

                    case "categorias:selectAll":
                    case "obtenerEstados":
                    case "categorias:guardarIntegrado":
                        ProcesarCategoriaIntegrada(tipo, raiz);
                        break;

                    case "clientes:selectAll":
                    case "clientes:guardarIntegrado":
                        ProcesarClienteIntegrado(tipo, raiz);
                        break;

                    case "marcas:selectAll":
                    case "marcas:guardarIntegrado":
                        ProcesarMarcaIntegrada(tipo, raiz);
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
                            string.IsNullOrWhiteSpace(dui))
                        {
                            EnviarAlDashboard(new
                            {
                                type = "error",
                                message = "Nombre, apellido y DUI son obligatorios."
                            });
                            return;
                        }

                        if (nombre.Length > 100 ||
                            apellido.Length > 100 ||
                            dui.Length > 20 ||
                            (nit?.Length ?? 0) > 20 ||
                            (telefono?.Length ?? 0) > 25 ||
                            (correo?.Length ?? 0) > 150 ||
                            (direccion?.Length ?? 0) > 250)
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