
using AutoParts.BusinessLogic;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class MainForm : Form
    {
        private readonly CategoriaBL _categoriaBL;
        private readonly EstadoBL _estadoBL;

        // Constructor con inyección de dependencias
        public MainForm(
            CategoriaBL categoriaBL,
            EstadoBL estadoBL)
        {
            InitializeComponent();

            _categoriaBL = categoriaBL;
            _estadoBL = estadoBL;

            // Configuración de la ventana principal
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            // WebView2 ocupa todo el formulario
            webViewMain.Dock = DockStyle.Fill;

            // Evento para cargar el Dashboard
            Load += MainForm_Load;
        }

        // ==========================================
        // CARGAR EL DASHBOARD
        // ==========================================

        private async void MainForm_Load(
            object? sender,
            EventArgs e)
        {
            try
            {
                // Inicializar WebView2
                await webViewMain.EnsureCoreWebView2Async();

                // Escuchar mensajes enviados desde JavaScript
                webViewMain.CoreWebView2.WebMessageReceived +=
                    WebViewMain_WebMessageReceived;

                // Ruta del Dashboard
                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "index.html"
                );

                // Verificar que exista el HTML
                if (!File.Exists(ruta))
                {
                    MessageBox.Show(
                        "No se encontró el Dashboard:\n" + ruta,
                        "AutoParts",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return;
                }

                // Mostrar Dashboard en WebView2
                webViewMain.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar el Dashboard:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ==========================================
        // RECIBIR MENSAJES DESDE APP.JS
        // ==========================================

        private void WebViewMain_WebMessageReceived(
            object? sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using JsonDocument documento =
                    JsonDocument.Parse(e.WebMessageAsJson);

                JsonElement raiz = documento.RootElement;

                string tipo = raiz
                    .GetProperty("type")
                    .GetString() ?? "";

                switch (tipo)
                {
                    // Abrir el listado de categorías
                    case "categorias:abrirLista":

                        // Ejecutar la apertura después
                        // del evento de WebView2
                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirCategorias();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Categorías:\n" +
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
                MessageBox.Show(
                    "Error al procesar el mensaje:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // ==========================================
        // ABRIR frmCategorias
        // ==========================================

        private void AbrirCategorias()
        {
            using (frmCategorias formulario =
                new frmCategorias(
                    _categoriaBL,
                    _estadoBL))
            {
                formulario.ShowDialog(this);
            }
        }

        // ==========================================
        // EVENTOS EXISTENTES DEL DISEÑADOR
        // ==========================================

        private void webView21_Click(
            object sender,
            EventArgs e)
        {

        }

        private void webViewMain_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
