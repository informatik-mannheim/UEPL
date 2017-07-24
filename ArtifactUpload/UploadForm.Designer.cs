namespace ArtifactUpload
{
    partial class UploadForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.txt_file = new System.Windows.Forms.TextBox();
            this.lbl_file = new System.Windows.Forms.Label();
            this.btn_upload = new System.Windows.Forms.Button();
            this.txt_response = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btn_refresh = new System.Windows.Forms.Button();
            this.lbl_server = new System.Windows.Forms.Label();
            this.txt_Server = new System.Windows.Forms.TextBox();
            this.btn_server = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // txt_file
            // 
            this.txt_file.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_file.Location = new System.Drawing.Point(61, 90);
            this.txt_file.Name = "txt_file";
            this.txt_file.Size = new System.Drawing.Size(382, 20);
            this.txt_file.TabIndex = 0;
            this.txt_file.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.FilePathMouseDoubleClick);
            // 
            // lbl_file
            // 
            this.lbl_file.AutoSize = true;
            this.lbl_file.Location = new System.Drawing.Point(12, 93);
            this.lbl_file.Name = "lbl_file";
            this.lbl_file.Size = new System.Drawing.Size(26, 13);
            this.lbl_file.TabIndex = 1;
            this.lbl_file.Text = "File:";
            // 
            // btn_upload
            // 
            this.btn_upload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_upload.Location = new System.Drawing.Point(448, 258);
            this.btn_upload.Name = "btn_upload";
            this.btn_upload.Size = new System.Drawing.Size(75, 47);
            this.btn_upload.TabIndex = 2;
            this.btn_upload.Text = "Upload";
            this.btn_upload.UseVisualStyleBackColor = true;
            this.btn_upload.Click += new System.EventHandler(this.UploadButtonClick);
            // 
            // txt_response
            // 
            this.txt_response.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_response.Location = new System.Drawing.Point(3, 16);
            this.txt_response.Margin = new System.Windows.Forms.Padding(10);
            this.txt_response.Multiline = true;
            this.txt_response.Name = "txt_response";
            this.txt_response.Size = new System.Drawing.Size(424, 160);
            this.txt_response.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.txt_response);
            this.groupBox1.Location = new System.Drawing.Point(12, 127);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(430, 179);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Response";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 50);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Artifact:";
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Location = new System.Drawing.Point(61, 47);
            this.comboBox1.MaxDropDownItems = 12;
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(382, 21);
            this.comboBox1.Sorted = true;
            this.comboBox1.TabIndex = 7;
            // 
            // btn_refresh
            // 
            this.btn_refresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_refresh.Location = new System.Drawing.Point(449, 46);
            this.btn_refresh.Name = "btn_refresh";
            this.btn_refresh.Size = new System.Drawing.Size(75, 23);
            this.btn_refresh.TabIndex = 8;
            this.btn_refresh.Text = "Refresh";
            this.btn_refresh.UseVisualStyleBackColor = true;
            this.btn_refresh.Click += new System.EventHandler(this.RefreshButtonClick);
            // 
            // lbl_server
            // 
            this.lbl_server.AutoSize = true;
            this.lbl_server.Location = new System.Drawing.Point(12, 15);
            this.lbl_server.Name = "lbl_server";
            this.lbl_server.Size = new System.Drawing.Size(41, 13);
            this.lbl_server.TabIndex = 10;
            this.lbl_server.Text = "Server:";
            // 
            // txt_Server
            // 
            this.txt_Server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_Server.Location = new System.Drawing.Point(61, 12);
            this.txt_Server.Name = "txt_Server";
            this.txt_Server.Size = new System.Drawing.Size(382, 20);
            this.txt_Server.TabIndex = 9;
            // 
            // btn_server
            // 
            this.btn_server.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_server.Location = new System.Drawing.Point(449, 10);
            this.btn_server.Name = "btn_server";
            this.btn_server.Size = new System.Drawing.Size(75, 23);
            this.btn_server.TabIndex = 11;
            this.btn_server.Text = "Set Server";
            this.btn_server.UseVisualStyleBackColor = true;
            this.btn_server.Click += new System.EventHandler(this.ServerButtonClick);
            // 
            // UploadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 319);
            this.Controls.Add(this.btn_server);
            this.Controls.Add(this.lbl_server);
            this.Controls.Add(this.txt_Server);
            this.Controls.Add(this.btn_refresh);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btn_upload);
            this.Controls.Add(this.lbl_file);
            this.Controls.Add(this.txt_file);
            this.Name = "UploadForm";
            this.Text = "ArtifactUploader";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox txt_file;
        private System.Windows.Forms.Label lbl_file;
        private System.Windows.Forms.Button btn_upload;
        private System.Windows.Forms.TextBox txt_response;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btn_refresh;
        private System.Windows.Forms.Label lbl_server;
        private System.Windows.Forms.TextBox txt_Server;
        private System.Windows.Forms.Button btn_server;
    }
}

