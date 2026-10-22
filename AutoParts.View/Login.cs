
using System;
using System.IO;
using System.Windows.Forms;

namespace AutoParts.View
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();

            this.Load += Login_Load;
        }

        private async void Login_Load(object? sender, EventArgs e)
        {
            try
            {
                await webViewLogin.EnsureCoreWebView2Async();

                string ruta = Path.Combine(
                    AppContext.BaseDirectory,
                    "Web",
                    "AutoParts_Login.html"
                );

                if (!File.Exists(ruta))
                {
                    MessageBox.Show(
                        "No se encontró el archivo HTML:\n" + ruta,
                        "AutoParts",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return;
                }

                webViewLogin.Source = new Uri(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar el login:\n" + ex.Message,
                    "AutoParts",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void webViewLogin_Click(object sender, EventArgs e)
        {

        }
    }
}
