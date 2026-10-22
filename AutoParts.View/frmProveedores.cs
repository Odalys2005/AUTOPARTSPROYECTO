
using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmProveedores : Form
    {
        private readonly ProveedorBL _proveedorBL;

        

        public frmProveedores(ProveedorBL proveedorBL)
        {
            InitializeComponent();

            _proveedorBL = proveedorBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Proveedores";

            Width = 1200;
            Height = 720;
            MinimumSize = new System.Drawing.Size(900, 550);

            webViewProveedores.Dock = DockStyle.Fill;

            Load += frmProveedores_Load;
        }

       

        private async void frmProveedores_Load(
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
                    "proveedores.html"
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

                webViewProveedores.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar Proveedores:\n" + ex.Message,
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


        private void CargarProveedores()
        {
            var lista = _proveedorBL.SelectAll();

            EnviarAlHTML(new
            {
                type = "proveedores:list",
                data = lista
            });
        }

       

        private void WebViewProveedores_WebMessageReceived(
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
                    case "proveedores:selectAll":
                        CargarProveedores();
                        break;

                    case "proveedores:abrirNuevo":
                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirNuevoProveedor();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Nuevo proveedor:\n" +
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

        

        private void AbrirNuevoProveedor()
        {
            using (frmProveedoresNuevo formulario =
                new frmProveedoresNuevo(_proveedorBL))
            {
                DialogResult resultado = formulario.ShowDialog(this);

                if (resultado == DialogResult.OK)
                {
                    CargarProveedores();
                }
            }
        }

       

        private void webViewProveedores_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
