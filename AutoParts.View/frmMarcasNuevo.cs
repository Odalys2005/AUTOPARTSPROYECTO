
using AutoParts.BusinessLogic;
using AutoParts.Entity;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmMarcasNuevo : Form
    {
        private readonly MarcaBL _marcaBL;
        private readonly EstadoBL _estadoBL;

        public frmMarcasNuevo(
            MarcaBL marcaBL,
            EstadoBL estadoBL)
        {
            InitializeComponent();

            _marcaBL = marcaBL;
            _estadoBL = estadoBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Nueva marca";

            Width = 650;
            Height = 550;
            MinimumSize = new System.Drawing.Size(550, 450);

            webViewMarcas.Dock = DockStyle.Fill;

            Load += frmMarcasNuevo_Load;
        }

        private async void frmMarcasNuevo_Load(
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
                    "marcas-nuevo.html"
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

                webViewMarcas.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar Nueva marca:\n" + ex.Message,
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

        private void WebViewMarcas_WebMessageReceived(
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
                    case "obtenerEstados":
                        CargarEstados();
                        break;

                    case "guardar":
                        GuardarMarca(raiz.GetProperty("data"));
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

        private void GuardarMarca(JsonElement datos)
        {
            string nombre = (
                datos.GetProperty("Nombre").GetString() ?? ""
            ).Trim();

            int estadoId = datos
                .GetProperty("EstadoId")
                .GetInt32();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "Ingresa el nombre de la marca."
                });

                return;
            }

            if (nombre.Length > 100)
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "El nombre no puede superar 100 caracteres."
                });

                return;
            }

            if (estadoId <= 0)
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "Selecciona un estado válido."
                });

                return;
            }

            Marca marca = new Marca()
            {
                Nombre = nombre,
                EstadoId = estadoId
            };

            int resultado = _marcaBL.Insert(marca);

            if (resultado > 0)
            {
                MessageBox.Show(
                    "Marca registrada correctamente.",
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "No se pudo guardar la marca."
                });
            }
        }

        private void webViewMarcas_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
