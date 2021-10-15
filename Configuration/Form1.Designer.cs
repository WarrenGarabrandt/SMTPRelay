
namespace Configuration
{
    partial class Form1
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
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpSendQueue = new System.Windows.Forms.TabPage();
            this.ucSendQueueEditor = new SMTPRelay.Configuration.Controls.ucSendQueueEditor();
            this.tpEndpoints = new System.Windows.Forms.TabPage();
            this.ucEndpointEditor = new SMTPRelay.Configuration.Controls.ucEndpointEditor();
            this.tpUsers = new System.Windows.Forms.TabPage();
            this.ucUserEditor = new SMTPRelay.Configuration.Controls.ucUserEditor();
            this.tpMailGateway = new System.Windows.Forms.TabPage();
            this.ucMailGatewayEditor = new SMTPRelay.Configuration.Controls.ucMailGatewayEditor();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdStartConsole = new System.Windows.Forms.Button();
            this.cmdLogin = new System.Windows.Forms.Button();
            this.txtUserPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtUserEmail = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtServerURL = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tpDevices = new System.Windows.Forms.TabPage();
            this.ucDeviceEditor1 = new SMTPRelay.Configuration.Controls.ucDeviceEditor();
            this.tcMain.SuspendLayout();
            this.tpSendQueue.SuspendLayout();
            this.tpEndpoints.SuspendLayout();
            this.tpUsers.SuspendLayout();
            this.tpMailGateway.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tpDevices.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcMain
            // 
            this.tcMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcMain.Controls.Add(this.tpSendQueue);
            this.tcMain.Controls.Add(this.tpEndpoints);
            this.tcMain.Controls.Add(this.tpUsers);
            this.tcMain.Controls.Add(this.tpDevices);
            this.tcMain.Controls.Add(this.tpMailGateway);
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(810, 552);
            this.tcMain.TabIndex = 0;
            this.tcMain.SelectedIndexChanged += new System.EventHandler(this.tcMain_SelectedIndexChanged);
            // 
            // tpSendQueue
            // 
            this.tpSendQueue.Controls.Add(this.ucSendQueueEditor);
            this.tpSendQueue.Location = new System.Drawing.Point(4, 22);
            this.tpSendQueue.Name = "tpSendQueue";
            this.tpSendQueue.Padding = new System.Windows.Forms.Padding(3);
            this.tpSendQueue.Size = new System.Drawing.Size(802, 526);
            this.tpSendQueue.TabIndex = 0;
            this.tpSendQueue.Text = "Send Queue";
            this.tpSendQueue.UseVisualStyleBackColor = true;
            // 
            // ucSendQueueEditor
            // 
            this.ucSendQueueEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucSendQueueEditor.Location = new System.Drawing.Point(3, 3);
            this.ucSendQueueEditor.Name = "ucSendQueueEditor";
            this.ucSendQueueEditor.Size = new System.Drawing.Size(796, 520);
            this.ucSendQueueEditor.TabIndex = 0;
            // 
            // tpEndpoints
            // 
            this.tpEndpoints.Controls.Add(this.ucEndpointEditor);
            this.tpEndpoints.Location = new System.Drawing.Point(4, 22);
            this.tpEndpoints.Name = "tpEndpoints";
            this.tpEndpoints.Padding = new System.Windows.Forms.Padding(3);
            this.tpEndpoints.Size = new System.Drawing.Size(802, 526);
            this.tpEndpoints.TabIndex = 3;
            this.tpEndpoints.Text = "Endpoints";
            this.tpEndpoints.UseVisualStyleBackColor = true;
            // 
            // ucEndpointEditor
            // 
            this.ucEndpointEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucEndpointEditor.Location = new System.Drawing.Point(3, 3);
            this.ucEndpointEditor.Name = "ucEndpointEditor";
            this.ucEndpointEditor.Size = new System.Drawing.Size(796, 520);
            this.ucEndpointEditor.TabIndex = 0;
            // 
            // tpUsers
            // 
            this.tpUsers.Controls.Add(this.ucUserEditor);
            this.tpUsers.Location = new System.Drawing.Point(4, 22);
            this.tpUsers.Name = "tpUsers";
            this.tpUsers.Padding = new System.Windows.Forms.Padding(3);
            this.tpUsers.Size = new System.Drawing.Size(802, 526);
            this.tpUsers.TabIndex = 1;
            this.tpUsers.Text = "Users";
            this.tpUsers.UseVisualStyleBackColor = true;
            // 
            // ucUserEditor
            // 
            this.ucUserEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucUserEditor.Location = new System.Drawing.Point(3, 3);
            this.ucUserEditor.Name = "ucUserEditor";
            this.ucUserEditor.Size = new System.Drawing.Size(796, 520);
            this.ucUserEditor.TabIndex = 0;
            // 
            // tpMailGateway
            // 
            this.tpMailGateway.Controls.Add(this.ucMailGatewayEditor);
            this.tpMailGateway.Location = new System.Drawing.Point(4, 22);
            this.tpMailGateway.Name = "tpMailGateway";
            this.tpMailGateway.Padding = new System.Windows.Forms.Padding(3);
            this.tpMailGateway.Size = new System.Drawing.Size(802, 526);
            this.tpMailGateway.TabIndex = 2;
            this.tpMailGateway.Text = "Mail Gateways";
            this.tpMailGateway.UseVisualStyleBackColor = true;
            // 
            // ucMailGatewayEditor
            // 
            this.ucMailGatewayEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucMailGatewayEditor.Location = new System.Drawing.Point(3, 3);
            this.ucMailGatewayEditor.Name = "ucMailGatewayEditor";
            this.ucMailGatewayEditor.Size = new System.Drawing.Size(796, 520);
            this.ucMailGatewayEditor.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tcMain);
            this.groupBox1.Controls.Add(this.cmdStartConsole);
            this.groupBox1.Controls.Add(this.cmdLogin);
            this.groupBox1.Controls.Add(this.txtUserPassword);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtUserEmail);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtServerURL);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(810, 552);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Visible = false;
            // 
            // cmdStartConsole
            // 
            this.cmdStartConsole.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmdStartConsole.Location = new System.Drawing.Point(267, 337);
            this.cmdStartConsole.Name = "cmdStartConsole";
            this.cmdStartConsole.Size = new System.Drawing.Size(98, 23);
            this.cmdStartConsole.TabIndex = 9;
            this.cmdStartConsole.Text = "Start Console";
            this.cmdStartConsole.UseVisualStyleBackColor = true;
            this.cmdStartConsole.Click += new System.EventHandler(this.cmdStartConsole_Click);
            // 
            // cmdLogin
            // 
            this.cmdLogin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmdLogin.Location = new System.Drawing.Point(472, 337);
            this.cmdLogin.Name = "cmdLogin";
            this.cmdLogin.Size = new System.Drawing.Size(75, 23);
            this.cmdLogin.TabIndex = 8;
            this.cmdLogin.Text = "Login";
            this.cmdLogin.UseVisualStyleBackColor = true;
            // 
            // txtUserPassword
            // 
            this.txtUserPassword.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtUserPassword.Location = new System.Drawing.Point(267, 294);
            this.txtUserPassword.Name = "txtUserPassword";
            this.txtUserPassword.PasswordChar = '●';
            this.txtUserPassword.Size = new System.Drawing.Size(280, 20);
            this.txtUserPassword.TabIndex = 7;
            this.txtUserPassword.Text = "admin@local";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(264, 278);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "User Email";
            // 
            // txtUserEmail
            // 
            this.txtUserEmail.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtUserEmail.Location = new System.Drawing.Point(267, 243);
            this.txtUserEmail.Name = "txtUserEmail";
            this.txtUserEmail.Size = new System.Drawing.Size(280, 20);
            this.txtUserEmail.TabIndex = 3;
            this.txtUserEmail.Text = "admin@local";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(264, 227);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "User Email";
            // 
            // txtServerURL
            // 
            this.txtServerURL.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtServerURL.Location = new System.Drawing.Point(267, 192);
            this.txtServerURL.Name = "txtServerURL";
            this.txtServerURL.Size = new System.Drawing.Size(280, 20);
            this.txtServerURL.TabIndex = 1;
            this.txtServerURL.Text = "127.0.0.1";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(264, 176);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server URL";
            // 
            // tpDevices
            // 
            this.tpDevices.Controls.Add(this.ucDeviceEditor1);
            this.tpDevices.Location = new System.Drawing.Point(4, 22);
            this.tpDevices.Name = "tpDevices";
            this.tpDevices.Padding = new System.Windows.Forms.Padding(3);
            this.tpDevices.Size = new System.Drawing.Size(802, 526);
            this.tpDevices.TabIndex = 4;
            this.tpDevices.Text = "Devices";
            this.tpDevices.UseVisualStyleBackColor = true;
            // 
            // ucDeviceEditor1
            // 
            this.ucDeviceEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucDeviceEditor1.Location = new System.Drawing.Point(3, 3);
            this.ucDeviceEditor1.Name = "ucDeviceEditor1";
            this.ucDeviceEditor1.Size = new System.Drawing.Size(796, 520);
            this.ucDeviceEditor1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(834, 576);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "SMTP Relay Configuration";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.tcMain.ResumeLayout(false);
            this.tpSendQueue.ResumeLayout(false);
            this.tpEndpoints.ResumeLayout(false);
            this.tpUsers.ResumeLayout(false);
            this.tpMailGateway.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tpDevices.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpSendQueue;
        private System.Windows.Forms.TabPage tpUsers;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtServerURL;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUserPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtUserEmail;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button cmdStartConsole;
        private System.Windows.Forms.Button cmdLogin;
        private SMTPRelay.Configuration.Controls.ucUserEditor ucUserEditor;
        private System.Windows.Forms.TabPage tpMailGateway;
        private SMTPRelay.Configuration.Controls.ucMailGatewayEditor ucMailGatewayEditor;
        private SMTPRelay.Configuration.Controls.ucSendQueueEditor ucSendQueueEditor;
        private System.Windows.Forms.TabPage tpEndpoints;
        private SMTPRelay.Configuration.Controls.ucEndpointEditor ucEndpointEditor;
        private System.Windows.Forms.TabPage tpDevices;
        private SMTPRelay.Configuration.Controls.ucDeviceEditor ucDeviceEditor1;
    }
}

