
using AutoParts.BusinessLogic;
using AutoParts.Entity;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmProveedoresNuevo : Form
    {
        private readonly ProveedorBL _proveedorBL;

        public frmProveedoresNuevo(ProveedorBL proveedorBL)
        {
            InitializeComponent();

            _proveedorBL = proveedorBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Nuevo proveedor";

            Width = 750;
            Height = 800;
            MinimumSize = new System.Drawing.Size(650, 600);

            webViewProveedores.Dock = DockStyle.Fill;

            Load += frmProveedoresNuevo_Load;
        }

        private async void frmProveedoresNuevo_Load(
            object? sender,
            EventArgs e)
        {
            try
            {
                await webViewProveedores.EnsureCoreWebView2Async();

                webViewProveedores.CoreWebView2.WebMessageReceived +=
                    WebViewProveedores_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "modulos",
                    "proveedores-nuevo.html"
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

                webViewProveedores.Source = new Uri(ruta);
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
            if (webViewProveedores.CoreWebView2 == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);

            webViewProveedores.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void WebViewProveedores_WebMessageReceived(
            object? sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using JsonDocument documento =
                    JsonDocument.Parse(e.WebMessageAsJson);

                JsonElement raiz = documento.RootElement;

                string tipo =
                    raiz.GetProperty("type").GetString() ?? "";

                switch (tipo)
                {
                    case "guardar":
                        GuardarProveedor(raiz.GetProperty("data"));
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

        private void GuardarProveedor(JsonElement datos)
        {
            string nombreEmpresa = (
                datos.GetProperty("NombreEmpresa").GetString() ?? ""
            ).Trim();

            string nit = (
                datos.GetProperty("NIT").GetString() ?? ""
            ).Trim();

            string telefono = (
                datos.GetProperty("Telefono").GetString() ?? ""
            ).Trim();

            string correo = (
                datos.GetProperty("Correo").GetString() ?? ""
            ).Trim();

            string contacto = (
                datos.GetProperty("Contacto").GetString() ?? ""
            ).Trim();

            if (string.IsNullOrWhiteSpace(nombreEmpresa) ||
                string.IsNullOrWhiteSpace(nit) ||
                string.IsNullOrWhiteSpace(telefono) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contacto))
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "Todos los campos son obligatorios."
                });

                return;
            }

            if (nombreEmpresa.Length > 100 ||
                nit.Length > 20 ||
                telefono.Length > 25 ||
                correo.Length > 150 ||
                contacto.Length > 100)
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "Uno o más campos superan la longitud permitida."
                });

                return;
            }

            if (!System.Net.Mail.MailAddress.TryCreate(correo, out var direccion) ||
                direccion.Address != correo)
            {
                EnviarAlHTML(new
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
                MessageBox.Show(
                    "Proveedor registrado correctamente.",
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
                    message = "No se pudo guardar el proveedor."
                });
            }
        }

        private void webViewProveedores_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
