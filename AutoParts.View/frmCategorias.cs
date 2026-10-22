
using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmCategorias : Form
    {
        private readonly CategoriaBL _categoriaBL;
        private readonly EstadoBL _estadoBL;

        public frmCategorias(CategoriaBL categoriaBL, EstadoBL estadoBL)
        {
            InitializeComponent();
            _categoriaBL = categoriaBL;
            _estadoBL = estadoBL;
            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Categorías";
            Width = 1100;
            Height = 720;
            MinimumSize = new System.Drawing.Size(800, 550);
            webViewCategorias.Dock = DockStyle.Fill;
            Load += frmCategorias_Load;
        }

        private async void frmCategorias_Load(object? sender, EventArgs e)
        {
            try
            {
                await webViewCategorias.EnsureCoreWebView2Async();
                webViewCategorias.CoreWebView2.WebMessageReceived += WebViewCategorias_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "modulos",
                    "categorias.html"
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

                webViewCategorias.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar Categorías:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void EnviarAlHTML(object mensaje)
        {
            if (webViewCategorias.CoreWebView2 == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);
            webViewCategorias.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void CargarCategorias()
        {
            var lista = _categoriaBL.SelectAll();
            EnviarAlHTML(new
            {
                type = "categorias:list",
                data = lista
            });
        }

        private void WebViewCategorias_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using JsonDocument documento = JsonDocument.Parse(e.WebMessageAsJson);
                string tipo = documento.RootElement.GetProperty("type").GetString() ?? "";

                switch (tipo)
                {
                    case "categorias:selectAll":
                        CargarCategorias();
                        break;

                    case "categorias:abrirNuevo":
                        BeginInvoke(new Action(() =>
                        {
                            AbrirNuevaCategoria();
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

        private void AbrirNuevaCategoria()
        {
            using (frmCategoriasNuevo formulario = new frmCategoriasNuevo(_categoriaBL, _estadoBL))
            {
                DialogResult resultado = formulario.ShowDialog(this);
                if (resultado == DialogResult.OK)
                {
                    CargarCategorias();
                }
            }
        }

        private void webViewCategorias_Click(object sender, EventArgs e)
        {
        }
    }
}
