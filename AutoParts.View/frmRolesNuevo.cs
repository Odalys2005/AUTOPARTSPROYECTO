
using AutoParts.BusinessLogic;
using AutoParts.Entity;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmRolesNuevo : Form
    {
        private readonly RolBL _rolBL;

        public frmRolesNuevo(RolBL rolBL)
        {
            InitializeComponent();

            _rolBL = rolBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Nuevo rol";

            Width = 650;
            Height = 550;

            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            webViewRoles.Dock = DockStyle.Fill;

            Load += frmRolesNuevo_Load;
        }


        private async void frmRolesNuevo_Load(
            object? sender,
            EventArgs e)
        {
            try
            {
                await webViewRoles.EnsureCoreWebView2Async();

                webViewRoles.CoreWebView2.WebMessageReceived +=
                    WebViewRoles_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "modulos",
                    "roles-nuevo.html"
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

                webViewRoles.Source = new Uri(ruta);
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
            if (webViewRoles.CoreWebView2 == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);

            webViewRoles.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void WebViewRoles_WebMessageReceived(
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
                        GuardarRol(raiz.GetProperty("data"));
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

        private void GuardarRol(JsonElement datos)
        {
            string nombre = (
                datos.GetProperty("Nombre").GetString() ?? ""
            ).Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "Ingresa el nombre del rol."
                });

                return;
            }

            if (nombre.Length > 100)
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "El nombre no puede superar los 100 caracteres."
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
                MessageBox.Show(
                    "Rol registrado correctamente.",
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
                    message = "No se pudo guardar el rol."
                });
            }
        }

        private void webViewRoles_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
