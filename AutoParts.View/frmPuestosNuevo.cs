
using AutoParts.BusinessLogic;
using AutoParts.Entity;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class frmPuestosNuevo : Form
    {
        private readonly PuestoBL _puestoBL;
        private readonly EstadoBL _estadoBL;

        public frmPuestosNuevo(
            PuestoBL puestoBL,
            EstadoBL estadoBL)
        {
            InitializeComponent();

            _puestoBL = puestoBL;
            _estadoBL = estadoBL;

            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoParts | Nuevo puesto";

            Width = 700;
            Height = 650;
            MinimumSize = new System.Drawing.Size(600, 550);

            webViewPuestos.Dock = DockStyle.Fill;

            Load += frmPuestosNuevo_Load;
        }

        private async void frmPuestosNuevo_Load(
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
                    "puestos-nuevo.html"
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

                webViewPuestos.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar Nuevo puesto:\n" + ex.Message,
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

        private void WebViewPuestos_WebMessageReceived(
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
                        GuardarPuesto(raiz.GetProperty("data"));
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

        private void GuardarPuesto(JsonElement datos)
        {
            string nombre = (
                datos.GetProperty("Nombre").GetString() ?? ""
            ).Trim();

            string? descripcion = null;

            if (datos.TryGetProperty("Descripcion", out JsonElement valor) &&
                valor.ValueKind == JsonValueKind.String)
            {
                descripcion = valor.GetString()?.Trim();
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                descripcion = null;
            }

            int estadoId = datos
                .GetProperty("EstadoId")
                .GetInt32();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                EnviarAlHTML(new
                {
                    type = "error",
                    message = "Ingresa el nombre del puesto."
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

            Puesto puesto = new Puesto()
            {
                Nombre = nombre,
                Descripcion = descripcion,
                EstadoId = estadoId
            };

            int resultado = _puestoBL.Insert(puesto);

            if (resultado > 0)
            {
                MessageBox.Show(
                    "Puesto registrado correctamente.",
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
                    message = "No se pudo guardar el puesto."
                });
            }
        }

        private void webViewPuestos_Click(
            object sender,
            EventArgs e)
        {

        }
    }
}
