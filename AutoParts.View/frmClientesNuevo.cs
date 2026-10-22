
using AutoParts.BusinessLogic;
using AutoParts.Entity;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmClientesNuevo : Form
    {
        private readonly ClienteBL _clienteBL;

        public frmClientesNuevo(ClienteBL clienteBL)
        {
            InitializeComponent();

            _clienteBL = clienteBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Nuevo cliente";

            Width = 750;
            Height = 850;
            MinimumSize = new System.Drawing.Size(650, 600);

            webViewClientes.Dock = DockStyle.Fill;

            Load += frmClientesNuevo_Load;
        }

        private async void frmClientesNuevo_Load(
            object? sender,
            EventArgs e)
        {
            try
            {
                await webViewClientes.EnsureCoreWebView2Async();

                webViewClientes.CoreWebView2.WebMessageReceived +=
                    WebViewClientes_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "modulos",
                    "clientes-nuevo.html"
                );

                if (!File.Exists(ruta))
                {
                    MessageBox.Show(
                        "No se encontró el archivo:\n" + ruta,
                        "AutoParts",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                webViewClientes.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar el formulario:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void EnviarAlHTML(object mensaje)
        {
            if (webViewClientes.CoreWebView2 == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);

            webViewClientes.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void WebViewClientes_WebMessageReceived(
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
                    case "guardar":
                        GuardarCliente(raiz.GetProperty("data"));
                        break;

                    case "cancelar":
                        DialogResult = DialogResult.Cancel;
                        Close();
                        break;
                }
            }
            catch (Exception ex)
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = ex.Message
                });
            }
        }

        private void GuardarCliente(JsonElement datos)
        {
            string nombre = (
                datos.GetProperty("Nombre").GetString() ?? ""
            ).Trim();

            string apellido = (
                datos.GetProperty("Apellido").GetString() ?? ""
            ).Trim();

            string dui = (
                datos.GetProperty("DUI").GetString() ?? ""
            ).Trim();

            string? nit = ObtenerOpcional(datos, "NIT");
            string? telefono = ObtenerOpcional(datos, "Telefono");
            string? correo = ObtenerOpcional(datos, "Correo");
            string? direccion = ObtenerOpcional(datos, "Direccion");

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellido) ||
                string.IsNullOrWhiteSpace(dui))
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "Nombre, apellido y DUI son obligatorios."
                });

                return;
            }

            if (!string.IsNullOrWhiteSpace(correo) &&
                (!System.Net.Mail.MailAddress.TryCreate(correo, out var email) ||
                 email.Address != correo))
            {
                EnviarAlHTML(new
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
                MessageBox.Show(
                    "Cliente registrado correctamente.",
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "No se pudo guardar el cliente."
                });
            }
        }

        private string? ObtenerOpcional(
            JsonElement datos,
            string propiedad)
        {
            if (!datos.TryGetProperty(propiedad, out JsonElement valor))
                return null;

            if (valor.ValueKind == JsonValueKind.Null ||
                valor.ValueKind == JsonValueKind.Undefined)
                return null;

            string texto = valor.GetString()?.Trim() ?? "";

            return string.IsNullOrWhiteSpace(texto)
                ? null
                : texto;
        }

        private void webViewClientes_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
