
using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmEstados : Form
    {
        private readonly EstadoBL _estadoBL;

        public frmEstados(EstadoBL estadoBL)
        {
            InitializeComponent();

            _estadoBL = estadoBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Estados";
            Width = 1100;
            Height = 720;
            MinimumSize = new System.Drawing.Size(800, 550);

            webViewEstados.Dock = DockStyle.Fill;

            Load += frmEstados_Load;
        }

        private async void frmEstados_Load(object? sender, EventArgs e)
        {
            try
            {
                await webViewEstados.EnsureCoreWebView2Async();

                webViewEstados.CoreWebView2.WebMessageReceived +=
                    WebViewEstados_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "modulos",
                    "estados.html"
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

                webViewEstados.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar Estados:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void EnviarAlHTML(object mensaje)
        {
            if (webViewEstados.CoreWebView2 == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);

            webViewEstados.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void CargarEstados()
        {
            var lista = _estadoBL.SelectAll();

            EnviarAlHTML(new
            {
                type = "estados:list",
                data = lista
            });
        }

        private void WebViewEstados_WebMessageReceived(
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
                    case "estados:selectAll":
                        CargarEstados();
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

        private void webViewEstados_Click(object sender, EventArgs e)
        {
        }
    }
}
