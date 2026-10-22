namespace AutoParts.View
{
    partial class frmCategorias
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
            webViewCategorias = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewCategorias).BeginInit();
            SuspendLayout();
            // 
            // webViewCategorias
            // 
            webViewCategorias.AllowExternalDrop = true;
            webViewCategorias.CreationProperties = null;
            webViewCategorias.DefaultBackgroundColor = Color.White;
            webViewCategorias.Dock = DockStyle.Fill;
            webViewCategorias.Location = new Point(0, 0);
            webViewCategorias.Name = "webViewCategorias";
            webViewCategorias.Size = new Size(800, 450);
            webViewCategorias.TabIndex = 0;
            webViewCategorias.ZoomFactor = 1D;
            webViewCategorias.Click += webViewCategorias_Click;
            // 
            // frmCategorias
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(webViewCategorias);
            Name = "frmCategorias";
            Text = "frmCategorias";
            ((System.ComponentModel.ISupportInitialize)webViewCategorias).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewCategorias;
    }
}