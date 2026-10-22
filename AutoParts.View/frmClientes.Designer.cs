namespace AutoParts.View
{
    partial class frmClientes
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
            webViewClientes = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewClientes).BeginInit();
            SuspendLayout();
            // 
            // webViewClientes
            // 
            webViewClientes.AllowExternalDrop = true;
            webViewClientes.CreationProperties = null;
            webViewClientes.DefaultBackgroundColor = Color.White;
            webViewClientes.Dock = DockStyle.Fill;
            webViewClientes.Location = new Point(0, 0);
            webViewClientes.Name = "webViewClientes";
            webViewClientes.Size = new Size(800, 450);
            webViewClientes.TabIndex = 0;
            webViewClientes.ZoomFactor = 1D;
            webViewClientes.Click += webViewClientes_Click;
            // 
            // frmClientes
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(webViewClientes);
            Name = "frmClientes";
            Text = "frmClientes";
            ((System.ComponentModel.ISupportInitialize)webViewClientes).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewClientes;
    }
}