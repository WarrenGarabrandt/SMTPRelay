
namespace SMTPRelay.Configuration.Controls
{
    partial class ucMailGatewayEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdSaveChanges = new System.Windows.Forms.Button();
            this.txtSenderOverride = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkAuthenticate = new System.Windows.Forms.CheckBox();
            this.chkEnableSSL = new System.Windows.Forms.CheckBox();
            this.nudPort = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSMTPServer = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdNew = new System.Windows.Forms.Button();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.lstMailGateways = new System.Windows.Forms.ListView();
            this.colMailGatewayID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSMTPServer = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colEnableSSL = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAuthenticate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colUsername = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSenderOverride = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chkConnectionLimit = new System.Windows.Forms.CheckBox();
            this.nudConnectionLimit = new System.Windows.Forms.NumericUpDown();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudConnectionLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.nudConnectionLimit);
            this.groupBox1.Controls.Add(this.chkConnectionLimit);
            this.groupBox1.Controls.Add(this.cmdSaveChanges);
            this.groupBox1.Controls.Add(this.txtSenderOverride);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtPassword);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtUsername);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.chkAuthenticate);
            this.groupBox1.Controls.Add(this.chkEnableSSL);
            this.groupBox1.Controls.Add(this.nudPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtSMTPServer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cmdNew);
            this.groupBox1.Controls.Add(this.cmdDelete);
            this.groupBox1.Controls.Add(this.lstMailGateways);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(733, 341);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Mail Gateways";
            // 
            // cmdSaveChanges
            // 
            this.cmdSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSaveChanges.Location = new System.Drawing.Point(630, 293);
            this.cmdSaveChanges.Name = "cmdSaveChanges";
            this.cmdSaveChanges.Size = new System.Drawing.Size(100, 42);
            this.cmdSaveChanges.TabIndex = 16;
            this.cmdSaveChanges.Text = "Save Changes";
            this.cmdSaveChanges.UseVisualStyleBackColor = true;
            this.cmdSaveChanges.Click += new System.EventHandler(this.cmdSaveChanges_Click);
            // 
            // txtSenderOverride
            // 
            this.txtSenderOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtSenderOverride.Location = new System.Drawing.Point(6, 238);
            this.txtSenderOverride.Name = "txtSenderOverride";
            this.txtSenderOverride.Size = new System.Drawing.Size(259, 20);
            this.txtSenderOverride.TabIndex = 14;
            this.txtSenderOverride.TextChanged += new System.EventHandler(this.txtSenderOverride_TextChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 222);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Sender Override";
            // 
            // txtPassword
            // 
            this.txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtPassword.Location = new System.Drawing.Point(168, 306);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Size = new System.Drawing.Size(147, 20);
            this.txtPassword.TabIndex = 12;
            this.txtPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(165, 290);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Password";
            // 
            // txtUsername
            // 
            this.txtUsername.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtUsername.Location = new System.Drawing.Point(9, 306);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(147, 20);
            this.txtUsername.TabIndex = 10;
            this.txtUsername.TextChanged += new System.EventHandler(this.txtUsername_TextChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 290);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Username";
            // 
            // chkAuthenticate
            // 
            this.chkAuthenticate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAuthenticate.AutoSize = true;
            this.chkAuthenticate.Location = new System.Drawing.Point(6, 270);
            this.chkAuthenticate.Name = "chkAuthenticate";
            this.chkAuthenticate.Size = new System.Drawing.Size(86, 17);
            this.chkAuthenticate.TabIndex = 8;
            this.chkAuthenticate.Text = "Authenticate";
            this.chkAuthenticate.UseVisualStyleBackColor = true;
            this.chkAuthenticate.CheckedChanged += new System.EventHandler(this.chkAuthenticate_CheckedChanged);
            // 
            // chkEnableSSL
            // 
            this.chkEnableSSL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkEnableSSL.AutoSize = true;
            this.chkEnableSSL.Location = new System.Drawing.Point(352, 195);
            this.chkEnableSSL.Name = "chkEnableSSL";
            this.chkEnableSSL.Size = new System.Drawing.Size(82, 17);
            this.chkEnableSSL.TabIndex = 7;
            this.chkEnableSSL.Text = "Enable SSL";
            this.chkEnableSSL.UseVisualStyleBackColor = true;
            this.chkEnableSSL.CheckedChanged += new System.EventHandler(this.chkEnableSSL_CheckedChanged);
            // 
            // nudPort
            // 
            this.nudPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudPort.Location = new System.Drawing.Point(276, 193);
            this.nudPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudPort.Name = "nudPort";
            this.nudPort.Size = new System.Drawing.Size(66, 20);
            this.nudPort.TabIndex = 6;
            this.nudPort.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.nudPort.ValueChanged += new System.EventHandler(this.nudPort_ValueChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(273, 177);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Port";
            // 
            // txtSMTPServer
            // 
            this.txtSMTPServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtSMTPServer.Location = new System.Drawing.Point(6, 193);
            this.txtSMTPServer.Name = "txtSMTPServer";
            this.txtSMTPServer.Size = new System.Drawing.Size(259, 20);
            this.txtSMTPServer.TabIndex = 4;
            this.txtSMTPServer.TextChanged += new System.EventHandler(this.txtSMTPServer_TextChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 177);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "SMTP Server";
            // 
            // cmdNew
            // 
            this.cmdNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdNew.Location = new System.Drawing.Point(87, 147);
            this.cmdNew.Name = "cmdNew";
            this.cmdNew.Size = new System.Drawing.Size(75, 23);
            this.cmdNew.TabIndex = 2;
            this.cmdNew.Text = "New";
            this.cmdNew.UseVisualStyleBackColor = true;
            this.cmdNew.Click += new System.EventHandler(this.cmdNew_Click);
            // 
            // cmdDelete
            // 
            this.cmdDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdDelete.Location = new System.Drawing.Point(6, 147);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(75, 23);
            this.cmdDelete.TabIndex = 1;
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // lstMailGateways
            // 
            this.lstMailGateways.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstMailGateways.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colMailGatewayID,
            this.colSMTPServer,
            this.colPort,
            this.colEnableSSL,
            this.colAuthenticate,
            this.colUsername,
            this.colSenderOverride});
            this.lstMailGateways.FullRowSelect = true;
            this.lstMailGateways.HideSelection = false;
            this.lstMailGateways.Location = new System.Drawing.Point(6, 19);
            this.lstMailGateways.MultiSelect = false;
            this.lstMailGateways.Name = "lstMailGateways";
            this.lstMailGateways.Size = new System.Drawing.Size(721, 122);
            this.lstMailGateways.TabIndex = 0;
            this.lstMailGateways.UseCompatibleStateImageBehavior = false;
            this.lstMailGateways.View = System.Windows.Forms.View.Details;
            this.lstMailGateways.SelectedIndexChanged += new System.EventHandler(this.lstMailGateways_SelectedIndexChanged);
            // 
            // colMailGatewayID
            // 
            this.colMailGatewayID.Text = "ID";
            this.colMailGatewayID.Width = 34;
            // 
            // colSMTPServer
            // 
            this.colSMTPServer.Text = "SMTP Server";
            this.colSMTPServer.Width = 173;
            // 
            // colPort
            // 
            this.colPort.Text = "Port";
            this.colPort.Width = 51;
            // 
            // colEnableSSL
            // 
            this.colEnableSSL.Text = "Enable SSL";
            this.colEnableSSL.Width = 71;
            // 
            // colAuthenticate
            // 
            this.colAuthenticate.Text = "Authenticate";
            this.colAuthenticate.Width = 75;
            // 
            // colUsername
            // 
            this.colUsername.Text = "Username";
            this.colUsername.Width = 151;
            // 
            // colSenderOverride
            // 
            this.colSenderOverride.Text = "Sender Override";
            this.colSenderOverride.Width = 162;
            // 
            // chkConnectionLimit
            // 
            this.chkConnectionLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkConnectionLimit.AutoSize = true;
            this.chkConnectionLimit.Location = new System.Drawing.Point(276, 240);
            this.chkConnectionLimit.Name = "chkConnectionLimit";
            this.chkConnectionLimit.Size = new System.Drawing.Size(104, 17);
            this.chkConnectionLimit.TabIndex = 17;
            this.chkConnectionLimit.Text = "Connection Limit";
            this.chkConnectionLimit.UseVisualStyleBackColor = true;
            this.chkConnectionLimit.CheckedChanged += new System.EventHandler(this.chkConnectionLimit_CheckedChanged);
            // 
            // nudConnectionLimit
            // 
            this.nudConnectionLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudConnectionLimit.Location = new System.Drawing.Point(386, 238);
            this.nudConnectionLimit.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.nudConnectionLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudConnectionLimit.Name = "nudConnectionLimit";
            this.nudConnectionLimit.Size = new System.Drawing.Size(66, 20);
            this.nudConnectionLimit.TabIndex = 18;
            this.nudConnectionLimit.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.nudConnectionLimit.ValueChanged += new System.EventHandler(this.nudConnectionLimit_ValueChanged);
            // 
            // ucMailGatewayEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "ucMailGatewayEditor";
            this.Size = new System.Drawing.Size(733, 341);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudConnectionLimit)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView lstMailGateways;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Button cmdNew;
        private System.Windows.Forms.ColumnHeader colMailGatewayID;
        private System.Windows.Forms.ColumnHeader colSMTPServer;
        private System.Windows.Forms.ColumnHeader colPort;
        private System.Windows.Forms.ColumnHeader colEnableSSL;
        private System.Windows.Forms.ColumnHeader colAuthenticate;
        private System.Windows.Forms.ColumnHeader colUsername;
        private System.Windows.Forms.ColumnHeader colSenderOverride;
        private System.Windows.Forms.CheckBox chkEnableSSL;
        private System.Windows.Forms.NumericUpDown nudPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSMTPServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkAuthenticate;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSenderOverride;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button cmdSaveChanges;
        private System.Windows.Forms.NumericUpDown nudConnectionLimit;
        private System.Windows.Forms.CheckBox chkConnectionLimit;
    }
}
