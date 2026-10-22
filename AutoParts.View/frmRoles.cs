
using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmRoles : Form
    {
        private readonly RolBL _rolBL;

        public frmRoles(RolBL rolBL)
        {
            InitializeComponent();

            _rolBL = rolBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Roles";
            Width = 1100;
            Height = 720;
            MinimumSize = new System.Drawing.Size(800, 550);

            webViewRoles.Dock = DockStyle.Fill;

            Load += frmRoles_Load;
        }

        private async void frmRoles_Load(object? sender, EventArgs e)
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
                    "roles.html"
                );

                if (!File.Exists(ruta))
                {
                    MessageBox.Show(
                        "No se encontró el archivo:\n" + ruta,
                        "AutoParts",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                webViewRoles.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar Roles:\n" + ex.Message,
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


        private void CargarRoles()
        {
            var lista = _rolBL.SelectAll();

            EnviarAlHTML(new
            {
                type = "roles:list",
                data = lista
            });
        }

        
        private void WebViewRoles_WebMessageReceived(
            object? sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using JsonDocument documento =
                    JsonDocument.Parse(e.WebMessageAsJson);

                string tipo = documento.RootElement
                    .GetProperty("type")
                    .GetString() ?? "";

                switch (tipo)
                {
                    case "roles:selectAll":
                        CargarRoles();
                        break;

                    case "roles:abrirNuevo":
                        BeginInvoke(new Action(() =>
                        {
                            AbrirNuevoRol();
                        }));
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


        private void AbrirNuevoRol()
        {
            using (frmRolesNuevo formulario =
                new frmRolesNuevo(_rolBL))
            {
                DialogResult resultado = formulario.ShowDialog(this);

                if (resultado == DialogResult.OK)
                {
                    CargarRoles();
                }
            }
        }

        private void webViewRoles_Click(object sender, EventArgs e)
        {

        }
    }
}
