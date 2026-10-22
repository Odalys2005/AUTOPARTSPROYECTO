
using AutoParts.BusinessLogic;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class Form1 : Form
    {
        private readonly CategoriaBL _categoriaBL;

        public Form1(CategoriaBL categoriaBL)
        {
            InitializeComponent();
            _categoriaBL = categoriaBL;
            CargarLogin();
        }

        private void webView21_Click(object sender, EventArgs e)
        {

        }

        private async void CargarLogin()
        {
            try
            {
                await webView21.EnsureCoreWebView2Async();

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "login.html"
                );

                if (File.Exists(ruta))
                {
                    webView21.NavigationCompleted += WebView21_NavigationCompleted;
                    webView21.Source = new Uri(ruta);
                }
                else
                {
                    MessageBox.Show(
                        "No se encontró el archivo: " + ruta
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void WebView21_NavigationCompleted(
            object? sender,
            Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
                return;

            try
            {
                var categorias = _categoriaBL.SelectAll();

                string json = JsonSerializer.Serialize(categorias);

                await webView21.CoreWebView2.ExecuteScriptAsync(
                    $"mostrarCategorias({json});"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error al cargar categorías",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
