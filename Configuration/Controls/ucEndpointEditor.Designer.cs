
namespace SMTPRelay.Configuration.Controls
{
    partial class ucEndpointEditor
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
            this.grpMian = new System.Windows.Forms.GroupBox();
            this.chkMaildrop = new System.Windows.Forms.CheckBox();
            this.txtHostname = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbCertFriendlyName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbTLSMode = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbProtocol = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbIPAddress = new System.Windows.Forms.ComboBox();
            this.nudPort = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.lblIPAddress = new System.Windows.Forms.Label();
            this.cmdSaveChanges = new System.Windows.Forms.Button();
            this.cmdNew = new System.Windows.Forms.Button();
            this.cmdDelete = new System.Windows.Forms.Button();
            this.lstEndpoints = new System.Windows.Forms.ListView();
            this.colIPEndpointID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colProtocol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTLSMode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHostname = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colCertFriendlyName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colMaildrop = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.grpMian.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).BeginInit();
            this.SuspendLayout();
            // 
            // grpMian
            // 
            this.grpMian.Controls.Add(this.chkMaildrop);
            this.grpMian.Controls.Add(this.txtHostname);
            this.grpMian.Controls.Add(this.label5);
            this.grpMian.Controls.Add(this.cmbCertFriendlyName);
            this.grpMian.Controls.Add(this.label4);
            this.grpMian.Controls.Add(this.cmbTLSMode);
            this.grpMian.Controls.Add(this.label3);
            this.grpMian.Controls.Add(this.cmbProtocol);
            this.grpMian.Controls.Add(this.label1);
            this.grpMian.Controls.Add(this.cmbIPAddress);
            this.grpMian.Controls.Add(this.nudPort);
            this.grpMian.Controls.Add(this.label2);
            this.grpMian.Controls.Add(this.lblIPAddress);
            this.grpMian.Controls.Add(this.cmdSaveChanges);
            this.grpMian.Controls.Add(this.cmdNew);
            this.grpMian.Controls.Add(this.cmdDelete);
            this.grpMian.Controls.Add(this.lstEndpoints);
            this.grpMian.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMian.Location = new System.Drawing.Point(0, 0);
            this.grpMian.Name = "grpMian";
            this.grpMian.Size = new System.Drawing.Size(662, 342);
            this.grpMian.TabIndex = 0;
            this.grpMian.TabStop = false;
            this.grpMian.Text = "Endpoints";
            // 
            // chkMaildrop
            // 
            this.chkMaildrop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkMaildrop.AutoSize = true;
            this.chkMaildrop.Location = new System.Drawing.Point(9, 286);
            this.chkMaildrop.Name = "chkMaildrop";
            this.chkMaildrop.Size = new System.Drawing.Size(261, 17);
            this.chkMaildrop.TabIndex = 31;
            this.chkMaildrop.Text = "Allow Unauthenticated Maildrop to Maildrop Users";
            this.chkMaildrop.UseVisualStyleBackColor = true;
            this.chkMaildrop.CheckedChanged += new System.EventHandler(this.chkMaildrop_CheckedChanged);
            // 
            // txtHostname
            // 
            this.txtHostname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtHostname.Location = new System.Drawing.Point(6, 254);
            this.txtHostname.Name = "txtHostname";
            this.txtHostname.Size = new System.Drawing.Size(293, 20);
            this.txtHostname.TabIndex = 7;
            this.txtHostname.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 238);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(187, 13);
            this.label5.TabIndex = 30;
            this.label5.Text = "Hostname (FQDN) Reported to Clients";
            // 
            // cmbCertFriendlyName
            // 
            this.cmbCertFriendlyName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbCertFriendlyName.FormattingEnabled = true;
            this.cmbCertFriendlyName.Location = new System.Drawing.Point(305, 254);
            this.cmbCertFriendlyName.Name = "cmbCertFriendlyName";
            this.cmbCertFriendlyName.Size = new System.Drawing.Size(304, 21);
            this.cmbCertFriendlyName.TabIndex = 8;
            this.cmbCertFriendlyName.DropDown += new System.EventHandler(this.cmbCertFriendlyName_DropDown);
            this.cmbCertFriendlyName.SelectedValueChanged += new System.EventHandler(this.cmbCertFriendlyName_SelectedValueChanged);
            this.cmbCertFriendlyName.TextChanged += new System.EventHandler(this.cmbCertFriendlyName_TextChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(302, 238);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(130, 13);
            this.label4.TabIndex = 28;
            this.label4.Text = "SSL Certificate (TLS Only)";
            // 
            // cmbTLSMode
            // 
            this.cmbTLSMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbTLSMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTLSMode.FormattingEnabled = true;
            this.cmbTLSMode.Items.AddRange(new object[] {
            "Disabled",
            "Enabled",
            "Enforced"});
            this.cmbTLSMode.Location = new System.Drawing.Point(470, 207);
            this.cmbTLSMode.Name = "cmbTLSMode";
            this.cmbTLSMode.Size = new System.Drawing.Size(121, 21);
            this.cmbTLSMode.TabIndex = 6;
            this.cmbTLSMode.SelectedIndexChanged += new System.EventHandler(this.cmbTLSMode_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(470, 191);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "TLS Mode";
            // 
            // cmbProtocol
            // 
            this.cmbProtocol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProtocol.FormattingEnabled = true;
            this.cmbProtocol.Items.AddRange(new object[] {
            "None (Disable)",
            "SMTP",
            "ESMTP"});
            this.cmbProtocol.Location = new System.Drawing.Point(343, 207);
            this.cmbProtocol.Name = "cmbProtocol";
            this.cmbProtocol.Size = new System.Drawing.Size(121, 21);
            this.cmbProtocol.TabIndex = 5;
            this.cmbProtocol.SelectedIndexChanged += new System.EventHandler(this.cmbProtocol_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(343, 191);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 24;
            this.label1.Text = "Protocol";
            // 
            // cmbIPAddress
            // 
            this.cmbIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbIPAddress.FormattingEnabled = true;
            this.cmbIPAddress.Location = new System.Drawing.Point(6, 208);
            this.cmbIPAddress.Name = "cmbIPAddress";
            this.cmbIPAddress.Size = new System.Drawing.Size(259, 21);
            this.cmbIPAddress.TabIndex = 3;
            this.cmbIPAddress.DropDown += new System.EventHandler(this.cmbIPAddress_DropDown);
            this.cmbIPAddress.SelectedValueChanged += new System.EventHandler(this.cmbIPAddress_SelectedValueChanged);
            this.cmbIPAddress.TextChanged += new System.EventHandler(this.cmbIPAddress_TextChanged);
            // 
            // nudPort
            // 
            this.nudPort.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudPort.Location = new System.Drawing.Point(271, 208);
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
            this.nudPort.TabIndex = 4;
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
            this.label2.Location = new System.Drawing.Point(273, 192);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Port";
            // 
            // lblIPAddress
            // 
            this.lblIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblIPAddress.AutoSize = true;
            this.lblIPAddress.Location = new System.Drawing.Point(6, 192);
            this.lblIPAddress.Name = "lblIPAddress";
            this.lblIPAddress.Size = new System.Drawing.Size(89, 13);
            this.lblIPAddress.TabIndex = 18;
            this.lblIPAddress.Text = "Listen IP Address";
            // 
            // cmdSaveChanges
            // 
            this.cmdSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSaveChanges.Location = new System.Drawing.Point(559, 294);
            this.cmdSaveChanges.Name = "cmdSaveChanges";
            this.cmdSaveChanges.Size = new System.Drawing.Size(100, 42);
            this.cmdSaveChanges.TabIndex = 9;
            this.cmdSaveChanges.Text = "Save Changes";
            this.cmdSaveChanges.UseVisualStyleBackColor = true;
            this.cmdSaveChanges.Click += new System.EventHandler(this.cmdSaveChanges_Click);
            // 
            // cmdNew
            // 
            this.cmdNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdNew.Location = new System.Drawing.Point(87, 160);
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
            this.cmdDelete.Location = new System.Drawing.Point(6, 160);
            this.cmdDelete.Name = "cmdDelete";
            this.cmdDelete.Size = new System.Drawing.Size(75, 23);
            this.cmdDelete.TabIndex = 1;
            this.cmdDelete.Text = "Delete";
            this.cmdDelete.UseVisualStyleBackColor = true;
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
            // 
            // lstEndpoints
            // 
            this.lstEndpoints.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstEndpoints.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colIPEndpointID,
            this.colAddress,
            this.colPort,
            this.colProtocol,
            this.colTLSMode,
            this.colHostname,
            this.colCertFriendlyName,
            this.colMaildrop});
            this.lstEndpoints.FullRowSelect = true;
            this.lstEndpoints.HideSelection = false;
            this.lstEndpoints.Location = new System.Drawing.Point(6, 19);
            this.lstEndpoints.Name = "lstEndpoints";
            this.lstEndpoints.Size = new System.Drawing.Size(650, 135);
            this.lstEndpoints.TabIndex = 0;
            this.lstEndpoints.UseCompatibleStateImageBehavior = false;
            this.lstEndpoints.View = System.Windows.Forms.View.Details;
            this.lstEndpoints.SelectedIndexChanged += new System.EventHandler(this.lstEndpoints_SelectedIndexChanged);
            // 
            // colIPEndpointID
            // 
            this.colIPEndpointID.Text = "ID";
            this.colIPEndpointID.Width = 34;
            // 
            // colAddress
            // 
            this.colAddress.Text = "Address";
            this.colAddress.Width = 124;
            // 
            // colPort
            // 
            this.colPort.Text = "Port";
            // 
            // colProtocol
            // 
            this.colProtocol.Text = "Protocol";
            this.colProtocol.Width = 64;
            // 
            // colTLSMode
            // 
            this.colTLSMode.Text = "TLS Mode";
            this.colTLSMode.Width = 71;
            // 
            // colHostname
            // 
            this.colHostname.Text = "Hostname (FQDN)";
            this.colHostname.Width = 140;
            // 
            // colCertFriendlyName
            // 
            this.colCertFriendlyName.Text = "Certificate";
            this.colCertFriendlyName.Width = 254;
            // 
            // colMaildrop
            // 
            this.colMaildrop.Text = "Maildrop";
            // 
            // ucEndpointEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpMian);
            this.Name = "ucEndpointEditor";
            this.Size = new System.Drawing.Size(662, 342);
            this.grpMian.ResumeLayout(false);
            this.grpMian.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPort)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpMian;
        private System.Windows.Forms.ListView lstEndpoints;
        private System.Windows.Forms.Button cmdNew;
        private System.Windows.Forms.Button cmdDelete;
        private System.Windows.Forms.Button cmdSaveChanges;
        private System.Windows.Forms.ColumnHeader colIPEndpointID;
        private System.Windows.Forms.ColumnHeader colAddress;
        private System.Windows.Forms.ColumnHeader colPort;
        private System.Windows.Forms.ColumnHeader colProtocol;
        private System.Windows.Forms.ColumnHeader colTLSMode;
        private System.Windows.Forms.ColumnHeader colCertFriendlyName;
        private System.Windows.Forms.NumericUpDown nudPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblIPAddress;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbIPAddress;
        private System.Windows.Forms.ComboBox cmbProtocol;
        private System.Windows.Forms.ComboBox cmbTLSMode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbCertFriendlyName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtHostname;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ColumnHeader colHostname;
        private System.Windows.Forms.CheckBox chkMaildrop;
        private System.Windows.Forms.ColumnHeader colMaildrop;
    }
}
