namespace AutoParts.View
{
    partial class frmProveedoresNuevo
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
            webViewProveedores = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewProveedores).BeginInit();
            SuspendLayout();
            // 
            // webViewProveedores
            // 
            webViewProveedores.AllowExternalDrop = true;
            webViewProveedores.CreationProperties = null;
            webViewProveedores.DefaultBackgroundColor = Color.White;
            webViewProveedores.Dock = DockStyle.Fill;
            webViewProveedores.Location = new Point(0, 0);
            webViewProveedores.Name = "webViewProveedores";
            webViewProveedores.Size = new Size(800, 450);
            webViewProveedores.TabIndex = 0;
            webViewProveedores.ZoomFactor = 1D;
            webViewProveedores.Click += webViewProveedores_Click;
            // 
            // frmProveedoresNuevo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(webViewProveedores);
            Name = "frmProveedoresNuevo";
            Text = "frmProveedoresNuevo";
            ((System.ComponentModel.ISupportInitialize)webViewProveedores).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewProveedores;
    }
}