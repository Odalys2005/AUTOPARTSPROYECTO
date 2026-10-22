namespace AutoParts.View
{
    partial class frmMarcasNuevo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            webViewMarcas = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewMarcas).BeginInit();
            SuspendLayout();
            // 
            // webViewMarcas
            // 
            webViewMarcas.AllowExternalDrop = true;
            webViewMarcas.CreationProperties = null;
            webViewMarcas.DefaultBackgroundColor = Color.White;
            webViewMarcas.Dock = DockStyle.Fill;
            webViewMarcas.Location = new Point(0, 0);
            webViewMarcas.Name = "webViewMarcas";
            webViewMarcas.Size = new Size(800, 450);
            webViewMarcas.TabIndex = 0;
            webViewMarcas.ZoomFactor = 1D;
            webViewMarcas.Click += webViewMarcas_Click;
            // 
            // frmMarcasNuevo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(webViewMarcas);
            Name = "frmMarcasNuevo";
            Text = "frmMarcasNuevo";
            ((System.ComponentModel.ISupportInitialize)webViewMarcas).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewMarcas;
    }
}