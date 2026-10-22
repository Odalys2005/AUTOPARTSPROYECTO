
using AutoParts.BusinessLogic;
using AutoParts.Entity;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmCategoriasNuevo : Form
    {
        private readonly CategoriaBL _categoriaBL;
        private readonly EstadoBL _estadoBL;

        public frmCategoriasNuevo(
            CategoriaBL categoriaBL,
            EstadoBL estadoBL)
        {
            InitializeComponent();

            _categoriaBL = categoriaBL;
            _estadoBL = estadoBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Nueva categoría";

            Width = 650;
            Height = 670;

            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            webViewCategoria.Dock = DockStyle.Fill;

            Load += frmCategoriasNuevo_Load;
        }

        private async void frmCategoriasNuevo_Load(
            object? sender,
            EventArgs e)
        {
            try
            {
                await webViewCategoria.EnsureCoreWebView2Async();

                webViewCategoria.CoreWebView2.WebMessageReceived +=
                    WebViewCategoria_WebMessageReceived;

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "modulos",
                    "categorias-nuevo.html"
                );

                if (!File.Exists(ruta))
                {
                    MessageBox.Show(
                        "No se encontró el archivo:\n" + ruta,
                        "AutoParts",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                webViewCategoria.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar el formulario:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void EnviarAlHTML(object mensaje)
        {
            if (webViewCategoria.CoreWebView2 == null)
                return;

            string json = JsonSerializer.Serialize(mensaje);

            webViewCategoria.CoreWebView2.PostWebMessageAsJson(json);
        }

        private void WebViewCategoria_WebMessageReceived(
            object? sender,
            CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using JsonDocument documento =
                    JsonDocument.Parse(e.WebMessageAsJson);

                JsonElement raiz = documento.RootElement;

                string tipo =
                    raiz.GetProperty("type").GetString() ?? "";

                switch (tipo)
                {
                    case "obtenerEstados":
                        CargarEstados();
                        break;

                    case "guardar":
                        GuardarCategoria(raiz.GetProperty("data"));
                        break;

                    case "cancelar":
                        DialogResult = DialogResult.Cancel;
                        Close();
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

        private void CargarEstados()
        {
            var lista = _estadoBL.SelectAll();

            EnviarAlHTML(new
            {
                type = "estados",
                data = lista
            });
        }


        private void GuardarCategoria(JsonElement datos)
        {
            // Obtener el nombre de la categoría
            string nombre = (
                datos.GetProperty("Nombre").GetString() ?? ""
            ).Trim();

            // Obtener el estado seleccionado
            int estadoId = datos.GetProperty("EstadoId").GetInt32();

            // Validar los campos
            if (string.IsNullOrWhiteSpace(nombre) || estadoId <= 0)
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "Ingresa el nombre y selecciona un estado."
                });

                return;
            }

            // Crear la entidad
            Categoria categoria = new Categoria()
            {
                Nombre = nombre,
                EstadoId = estadoId
            };

            // Guardar en SQL Server mediante BusinessLogic
            int resultado = _categoriaBL.Insert(categoria);

            if (resultado > 0)
            {
                MessageBox.Show(
                    "Categoría registrada correctamente.",
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                // Cerrar el formulario y actualizar el listado
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "No se pudo guardar la categoría."
                });
            }
        }


        private void webViewCategoria_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
