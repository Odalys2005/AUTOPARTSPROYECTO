
using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmMarcas : Form
    {
        private readonly MarcaBL _marcaBL;
        private readonly EstadoBL _estadoBL;

        public frmMarcas(MarcaBL marcaBL, EstadoBL estadoBL)
        {
            InitializeComponent();

            _marcaBL = marcaBL;
            _estadoBL = estadoBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Marcas";

            Width = 1100;
            Height = 720;
            MinimumSize = new System.Drawing.Size(800, 550);

            webViewMarcas.Dock = DockStyle.Fill;

            Load += frmMarcas_Load;
        }

        private async void frmMarcas_Load(
            object? sender,
            EventArgs e)
        {
            try
            {
                await webViewMarcas.EnsureCoreWebView2Async();

                webViewMarcas.CoreWebView2.WebMessageReceived +=
                    WebViewMarcas_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "modulos",
                    "marcas.html"
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

                webViewMarcas.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar Marcas:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void EnviarAlHTML(object mensaje)
        {
            if (webViewMarcas.CoreWebView2 == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);

            webViewMarcas.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void CargarMarcas()
        {
            var lista = _marcaBL.SelectAll();

            EnviarAlHTML(new
            {
                type = "marcas:list",
                data = lista
            });
        }

        private void WebViewMarcas_WebMessageReceived(
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
                    case "marcas:selectAll":
                        CargarMarcas();
                        break;

                    case "marcas:abrirNuevo":
                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirNuevaMarca();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Nueva marca:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
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

        private void AbrirNuevaMarca()
        {
            using (frmMarcasNuevo formulario =
                new frmMarcasNuevo(_marcaBL, _estadoBL))
            {
                DialogResult resultado = formulario.ShowDialog(this);

                if (resultado == DialogResult.OK)
                {
                    CargarMarcas();
                }
            }
        }

        private void webViewMarcas_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
