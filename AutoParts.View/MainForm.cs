
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
        private readonly RolBL _rolBL;
        private readonly ProveedorBL _proveedorBL;
        private readonly ClienteBL _clienteBL;
        private readonly MarcaBL _marcaBL;
        private readonly PuestoBL _puestoBL;

        public MainForm(
            CategoriaBL categoriaBL,
            EstadoBL estadoBL,
            RolBL rolBL,
            ProveedorBL proveedorBL,
            ClienteBL clienteBL,
            MarcaBL marcaBL,
            PuestoBL puestoBL)
        {
            InitializeComponent();

            _categoriaBL = categoriaBL;
            _estadoBL = estadoBL;
            _rolBL = rolBL;
            _proveedorBL = proveedorBL;
            _clienteBL = clienteBL;
            _marcaBL = marcaBL;
            _puestoBL = puestoBL;

            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            webViewMain.Dock = DockStyle.Fill;

            Load += MainForm_Load;
        }

        private async void MainForm_Load(
            object? sender,
            EventArgs e)
        {
            try
            {
                await webViewMain.EnsureCoreWebView2Async();

                webViewMain.CoreWebView2.WebMessageReceived +=
                    WebViewMain_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "index.html"
                );

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
                    case "categorias:abrirLista":

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

                    case "estados:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirEstados();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Estados:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "roles:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirRoles();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Roles:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "proveedores:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirProveedores();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Proveedores:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "clientes:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirClientes();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Clientes:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "marcas:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirMarcas();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Marcas:\n" +
                                    ex.Message,
                                    "AutoParts",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                            }
                        }));

                        break;

                    case "puestos:abrirLista":

                        BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                AbrirPuestos();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(
                                    "Error al abrir Puestos:\n" +
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

        private void AbrirCategorias()
        {
            using (frmCategorias formulario =
                new frmCategorias(_categoriaBL, _estadoBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirEstados()
        {
            using (frmEstados formulario =
                new frmEstados(_estadoBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirRoles()
        {
            using (frmRoles formulario =
                new frmRoles(_rolBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirProveedores()
        {
            using (frmProveedores formulario =
                new frmProveedores(_proveedorBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirClientes()
        {
            using (frmClientes formulario =
                new frmClientes(_clienteBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirMarcas()
        {
            using (frmMarcas formulario =
                new frmMarcas(_marcaBL, _estadoBL))
            {
                formulario.ShowDialog(this);
            }
        }

        private void AbrirPuestos()
        {
            using (frmPuestos formulario =
                new frmPuestos(_puestoBL, _estadoBL))
            {
                formulario.ShowDialog(this);
            }
        }

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
