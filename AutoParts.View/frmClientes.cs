
using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmClientes : Form
    {
        private readonly ClienteBL _clienteBL;

        public frmClientes(ClienteBL clienteBL)
        {
            InitializeComponent();

            _clienteBL = clienteBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Clientes";

            Width = 1200;
            Height = 720;
            MinimumSize = new System.Drawing.Size(900, 550);

            webViewClientes.Dock = DockStyle.Fill;

            Load += frmClientes_Load;
        }

        private async void frmClientes_Load(
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
                    "clientes.html"
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

                webViewClientes.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar Clientes:\n" + ex.Message,
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

        private void CargarClientes()
        {
            var lista = _clienteBL.SelectAll();

            EnviarAlHTML(new
            {
                type = "clientes:list",
                data = lista
            });
        }

        private void WebViewClientes_WebMessageReceived(
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
                    case "clientes:selectAll":
                        CargarClientes();
                        break;

                    case "clientes:abrirNuevo":
                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirNuevoCliente();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Nuevo cliente:\n" +
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

        private void AbrirNuevoCliente()
        {
            using (frmClientesNuevo formulario =
                new frmClientesNuevo(_clienteBL))
            {
                DialogResult resultado = formulario.ShowDialog(this);

                if (resultado == DialogResult.OK)
                {
                    CargarClientes();
                }
            }
        }

        private void webViewClientes_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
