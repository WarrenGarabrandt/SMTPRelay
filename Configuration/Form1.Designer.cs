
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
            this.tpUsers = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtServerURL = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtUserEmail = new System.Windows.Forms.TextBox();
            this.txtUserPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmdLogin = new System.Windows.Forms.Button();
            this.cmdStartConsole = new System.Windows.Forms.Button();
            this.tcMain.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcMain
            // 
            this.tcMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcMain.Controls.Add(this.tpSendQueue);
            this.tcMain.Controls.Add(this.tpUsers);
            this.tcMain.Location = new System.Drawing.Point(12, 12);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(574, 426);
            this.tcMain.TabIndex = 0;
            this.tcMain.Visible = false;
            // 
            // tpSendQueue
            // 
            this.tpSendQueue.Location = new System.Drawing.Point(4, 22);
            this.tpSendQueue.Name = "tpSendQueue";
            this.tpSendQueue.Padding = new System.Windows.Forms.Padding(3);
            this.tpSendQueue.Size = new System.Drawing.Size(566, 400);
            this.tpSendQueue.TabIndex = 0;
            this.tpSendQueue.Text = "Send Queue";
            this.tpSendQueue.UseVisualStyleBackColor = true;
            // 
            // tpUsers
            // 
            this.tpUsers.Location = new System.Drawing.Point(4, 22);
            this.tpUsers.Name = "tpUsers";
            this.tpUsers.Padding = new System.Windows.Forms.Padding(3);
            this.tpUsers.Size = new System.Drawing.Size(566, 400);
            this.tpUsers.TabIndex = 1;
            this.tpUsers.Text = "Users";
            this.tpUsers.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
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
            this.groupBox1.Size = new System.Drawing.Size(574, 426);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(146, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server URL";
            // 
            // txtServerURL
            // 
            this.txtServerURL.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtServerURL.Location = new System.Drawing.Point(149, 129);
            this.txtServerURL.Name = "txtServerURL";
            this.txtServerURL.Size = new System.Drawing.Size(280, 20);
            this.txtServerURL.TabIndex = 1;
            this.txtServerURL.Text = "127.0.0.1";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(146, 164);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "User Email";
            // 
            // txtUserEmail
            // 
            this.txtUserEmail.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtUserEmail.Location = new System.Drawing.Point(149, 180);
            this.txtUserEmail.Name = "txtUserEmail";
            this.txtUserEmail.Size = new System.Drawing.Size(280, 20);
            this.txtUserEmail.TabIndex = 3;
            this.txtUserEmail.Text = "admin@local";
            // 
            // txtUserPassword
            // 
            this.txtUserPassword.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtUserPassword.Location = new System.Drawing.Point(149, 231);
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
            this.label3.Location = new System.Drawing.Point(146, 215);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "User Email";
            // 
            // cmdLogin
            // 
            this.cmdLogin.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmdLogin.Location = new System.Drawing.Point(354, 274);
            this.cmdLogin.Name = "cmdLogin";
            this.cmdLogin.Size = new System.Drawing.Size(75, 23);
            this.cmdLogin.TabIndex = 8;
            this.cmdLogin.Text = "Login";
            this.cmdLogin.UseVisualStyleBackColor = true;
            // 
            // cmdStartConsole
            // 
            this.cmdStartConsole.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cmdStartConsole.Location = new System.Drawing.Point(149, 274);
            this.cmdStartConsole.Name = "cmdStartConsole";
            this.cmdStartConsole.Size = new System.Drawing.Size(98, 23);
            this.cmdStartConsole.TabIndex = 9;
            this.cmdStartConsole.Text = "Start Console";
            this.cmdStartConsole.UseVisualStyleBackColor = true;
            this.cmdStartConsole.Click += new System.EventHandler(this.cmdStartConsole_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 450);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tcMain);
            this.Name = "Form1";
            this.Text = "SMTP Relay Configuration";
            this.tcMain.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
    }
}

