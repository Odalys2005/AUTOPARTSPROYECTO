namespace AutoParts.View
{
    partial class frmEstados
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
            webViewEstados = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewEstados).BeginInit();
            SuspendLayout();
            // 
            // webViewEstados
            // 
            webViewEstados.AllowExternalDrop = true;
            webViewEstados.CreationProperties = null;
            webViewEstados.DefaultBackgroundColor = Color.White;
            webViewEstados.Dock = DockStyle.Fill;
            webViewEstados.Location = new Point(0, 0);
            webViewEstados.Name = "webViewEstados";
            webViewEstados.Size = new Size(800, 450);
            webViewEstados.TabIndex = 0;
            webViewEstados.ZoomFactor = 1D;
            webViewEstados.Click += webViewEstados_Click;
            // 
            // frmEstados
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(webViewEstados);
            Name = "frmEstados";
            Text = "frmEstados";
            ((System.ComponentModel.ISupportInitialize)webViewEstados).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewEstados;
    }
}