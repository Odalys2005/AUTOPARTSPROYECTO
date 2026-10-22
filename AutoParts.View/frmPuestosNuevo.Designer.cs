namespace AutoParts.View
{
    partial class frmPuestosNuevo
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
            webViewPuestos = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewPuestos).BeginInit();
            SuspendLayout();
            // 
            // webViewPuestos
            // 
            webViewPuestos.AllowExternalDrop = true;
            webViewPuestos.CreationProperties = null;
            webViewPuestos.DefaultBackgroundColor = Color.White;
            webViewPuestos.Dock = DockStyle.Fill;
            webViewPuestos.Location = new Point(0, 0);
            webViewPuestos.Name = "webViewPuestos";
            webViewPuestos.Size = new Size(800, 450);
            webViewPuestos.TabIndex = 0;
            webViewPuestos.ZoomFactor = 1D;
            webViewPuestos.Click += webViewPuestos_Click;
            // 
            // frmPuestosNuevo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(webViewPuestos);
            Name = "frmPuestosNuevo";
            Text = "frmPuestosNuevo";
            ((System.ComponentModel.ISupportInitialize)webViewPuestos).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewPuestos;
    }
}