
using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmPuestos : Form
    {
        private readonly PuestoBL _puestoBL;
        private readonly EstadoBL _estadoBL;

        public frmPuestos(PuestoBL puestoBL, EstadoBL estadoBL)
        {
            InitializeComponent();

            _puestoBL = puestoBL;
            _estadoBL = estadoBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Puestos";

            Width = 1100;
            Height = 720;
            MinimumSize = new System.Drawing.Size(800, 550);

            webViewPuestos.Dock = DockStyle.Fill;

            Load += frmPuestos_Load;
        }

        private async void frmPuestos_Load(
            object? sender,
            EventArgs e)
        {
            try
            {
                await webViewPuestos.EnsureCoreWebView2Async();

                webViewPuestos.CoreWebView2.WebMessageReceived +=
                    WebViewPuestos_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "modulos",
                    "puestos.html"
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

                webViewPuestos.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar Puestos:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void EnviarAlHTML(object mensaje)
        {
            if (webViewPuestos.CoreWebView2 == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);

            webViewPuestos.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void CargarPuestos()
        {
            var lista = _puestoBL.SelectAll();

            EnviarAlHTML(new
            {
                type = "puestos:list",
                data = lista
            });
        }

        private void WebViewPuestos_WebMessageReceived(
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
                    case "puestos:selectAll":
                        CargarPuestos();
                        break;

                    case "puestos:abrirNuevo":
                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirNuevoPuesto();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Nuevo puesto:\n" +
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

        private void AbrirNuevoPuesto()
        {
            using (frmPuestosNuevo formulario =
                new frmPuestosNuevo(_puestoBL, _estadoBL))
            {
                DialogResult resultado = formulario.ShowDialog(this);

                if (resultado == DialogResult.OK)
                {
                    CargarPuestos();
                }
            }
        }

        private void webViewPuestos_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
