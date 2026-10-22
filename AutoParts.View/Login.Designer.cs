namespace AutoParts.View
{
    partial class Login
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
            webViewLogin = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webViewLogin).BeginInit();
            SuspendLayout();
            // 
            // webViewLogin
            // 
            webViewLogin.AllowExternalDrop = true;
            webViewLogin.CreationProperties = null;
            webViewLogin.DefaultBackgroundColor = Color.White;
            webViewLogin.Dock = DockStyle.Fill;
            webViewLogin.Location = new Point(0, 0);
            webViewLogin.Name = "webViewLogin";
            webViewLogin.Size = new Size(967, 529);
            webViewLogin.TabIndex = 0;
            webViewLogin.ZoomFactor = 1D;
            webViewLogin.Click += webViewLogin_Click;
            // 
            // Login
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(967, 529);
            Controls.Add(webViewLogin);
            Name = "Login";
            Text = "Login";
            ((System.ComponentModel.ISupportInitialize)webViewLogin).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webViewLogin;
    }
}