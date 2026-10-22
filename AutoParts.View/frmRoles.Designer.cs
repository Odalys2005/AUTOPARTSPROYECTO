namespace AutoParts.View
{
    partial class frmRoles
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
            webViewRoles = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewRoles).BeginInit();
            SuspendLayout();
            // 
            // webViewRoles
            // 
            webViewRoles.AllowExternalDrop = true;
            webViewRoles.CreationProperties = null;
            webViewRoles.DefaultBackgroundColor = Color.White;
            webViewRoles.Dock = DockStyle.Fill;
            webViewRoles.Location = new Point(0, 0);
            webViewRoles.Name = "webViewRoles";
            webViewRoles.Size = new Size(800, 450);
            webViewRoles.TabIndex = 0;
            webViewRoles.ZoomFactor = 1D;
            webViewRoles.Click += webViewRoles_Click;
            // 
            // frmRoles
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(webViewRoles);
            Name = "frmRoles";
            Text = "frmRoles";
            ((System.ComponentModel.ISupportInitialize)webViewRoles).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewRoles;
    }
}