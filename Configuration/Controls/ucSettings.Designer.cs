
namespace SMTPRelay.Configuration.Controls
{
    partial class ucSettings
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
            this.grpReceiver = new System.Windows.Forms.GroupBox();
            this.grpSender = new System.Windows.Forms.GroupBox();
            this.grpMessage = new System.Windows.Forms.GroupBox();
            this.grpPurge = new System.Windows.Forms.GroupBox();
            this.chkReceiverPage = new System.Windows.Forms.CheckBox();
            this.chkSenderPage = new System.Windows.Forms.CheckBox();
            this.chkMessagePage = new System.Windows.Forms.CheckBox();
            this.chkPurgePage = new System.Windows.Forms.CheckBox();
            this.nudRecConnTimeoutMS = new System.Windows.Forms.NumericUpDown();
            this.nudRecCmdTimeoutMS = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmdSaveRecConnTimeoutMS = new System.Windows.Forms.Button();
            this.cmdSaveRecCmdTimeoutMS = new System.Windows.Forms.Button();
            this.cmdSaveRecBadCmdLimit = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.nudRecBadCmdLimit = new System.Windows.Forms.NumericUpDown();
            this.cmbRecVerboseDebugging = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtRecLogPath = new System.Windows.Forms.TextBox();
            this.cmdSaveRecVerboseDebugging = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.cmdSaveRecLogPath = new System.Windows.Forms.Button();
            this.grpReceiver.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecConnTimeoutMS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecCmdTimeoutMS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecBadCmdLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // grpReceiver
            // 
            this.grpReceiver.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpReceiver.Controls.Add(this.cmdSaveRecLogPath);
            this.grpReceiver.Controls.Add(this.label5);
            this.grpReceiver.Controls.Add(this.cmdSaveRecVerboseDebugging);
            this.grpReceiver.Controls.Add(this.txtRecLogPath);
            this.grpReceiver.Controls.Add(this.label4);
            this.grpReceiver.Controls.Add(this.cmbRecVerboseDebugging);
            this.grpReceiver.Controls.Add(this.cmdSaveRecBadCmdLimit);
            this.grpReceiver.Controls.Add(this.label3);
            this.grpReceiver.Controls.Add(this.nudRecBadCmdLimit);
            this.grpReceiver.Controls.Add(this.cmdSaveRecCmdTimeoutMS);
            this.grpReceiver.Controls.Add(this.cmdSaveRecConnTimeoutMS);
            this.grpReceiver.Controls.Add(this.label2);
            this.grpReceiver.Controls.Add(this.label1);
            this.grpReceiver.Controls.Add(this.nudRecCmdTimeoutMS);
            this.grpReceiver.Controls.Add(this.nudRecConnTimeoutMS);
            this.grpReceiver.Location = new System.Drawing.Point(137, 3);
            this.grpReceiver.Name = "grpReceiver";
            this.grpReceiver.Size = new System.Drawing.Size(509, 488);
            this.grpReceiver.TabIndex = 5;
            this.grpReceiver.TabStop = false;
            this.grpReceiver.Text = "SMTP Receiver Settings";
            this.grpReceiver.Visible = false;
            // 
            // grpSender
            // 
            this.grpSender.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSender.Location = new System.Drawing.Point(137, 3);
            this.grpSender.Name = "grpSender";
            this.grpSender.Size = new System.Drawing.Size(509, 488);
            this.grpSender.TabIndex = 6;
            this.grpSender.TabStop = false;
            this.grpSender.Text = "SMTP Sender Settings";
            this.grpSender.Visible = false;
            // 
            // grpMessage
            // 
            this.grpMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMessage.Location = new System.Drawing.Point(137, 3);
            this.grpMessage.Name = "grpMessage";
            this.grpMessage.Size = new System.Drawing.Size(509, 488);
            this.grpMessage.TabIndex = 7;
            this.grpMessage.TabStop = false;
            this.grpMessage.Text = "Message Settings";
            this.grpMessage.Visible = false;
            // 
            // grpPurge
            // 
            this.grpPurge.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPurge.Location = new System.Drawing.Point(137, 3);
            this.grpPurge.Name = "grpPurge";
            this.grpPurge.Size = new System.Drawing.Size(509, 488);
            this.grpPurge.TabIndex = 8;
            this.grpPurge.TabStop = false;
            this.grpPurge.Text = "Purge Settings";
            this.grpPurge.Visible = false;
            // 
            // chkReceiverPage
            // 
            this.chkReceiverPage.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkReceiverPage.Location = new System.Drawing.Point(3, 8);
            this.chkReceiverPage.Name = "chkReceiverPage";
            this.chkReceiverPage.Size = new System.Drawing.Size(128, 46);
            this.chkReceiverPage.TabIndex = 9;
            this.chkReceiverPage.Tag = "R";
            this.chkReceiverPage.Text = "SMTP Receiver";
            this.chkReceiverPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkReceiverPage.UseVisualStyleBackColor = true;
            this.chkReceiverPage.CheckedChanged += new System.EventHandler(this.PageChange);
            // 
            // chkSenderPage
            // 
            this.chkSenderPage.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkSenderPage.Location = new System.Drawing.Point(3, 60);
            this.chkSenderPage.Name = "chkSenderPage";
            this.chkSenderPage.Size = new System.Drawing.Size(128, 46);
            this.chkSenderPage.TabIndex = 10;
            this.chkSenderPage.Tag = "S";
            this.chkSenderPage.Text = "SMTP Sender";
            this.chkSenderPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkSenderPage.UseVisualStyleBackColor = true;
            this.chkSenderPage.CheckedChanged += new System.EventHandler(this.PageChange);
            // 
            // chkMessagePage
            // 
            this.chkMessagePage.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkMessagePage.Location = new System.Drawing.Point(3, 112);
            this.chkMessagePage.Name = "chkMessagePage";
            this.chkMessagePage.Size = new System.Drawing.Size(128, 46);
            this.chkMessagePage.TabIndex = 11;
            this.chkMessagePage.Tag = "M";
            this.chkMessagePage.Text = "Message";
            this.chkMessagePage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkMessagePage.UseVisualStyleBackColor = true;
            this.chkMessagePage.CheckedChanged += new System.EventHandler(this.PageChange);
            // 
            // chkPurgePage
            // 
            this.chkPurgePage.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPurgePage.Location = new System.Drawing.Point(3, 164);
            this.chkPurgePage.Name = "chkPurgePage";
            this.chkPurgePage.Size = new System.Drawing.Size(128, 46);
            this.chkPurgePage.TabIndex = 12;
            this.chkPurgePage.Tag = "P";
            this.chkPurgePage.Text = "Purge";
            this.chkPurgePage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkPurgePage.UseVisualStyleBackColor = true;
            this.chkPurgePage.CheckedChanged += new System.EventHandler(this.PageChange);
            // 
            // nudRecConnTimeoutMS
            // 
            this.nudRecConnTimeoutMS.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudRecConnTimeoutMS.Location = new System.Drawing.Point(155, 36);
            this.nudRecConnTimeoutMS.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudRecConnTimeoutMS.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudRecConnTimeoutMS.Name = "nudRecConnTimeoutMS";
            this.nudRecConnTimeoutMS.Size = new System.Drawing.Size(78, 20);
            this.nudRecConnTimeoutMS.TabIndex = 0;
            this.nudRecConnTimeoutMS.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudRecConnTimeoutMS.ValueChanged += new System.EventHandler(this.nudRecConnTimeoutMS_ValueChanged);
            // 
            // nudRecCmdTimeoutMS
            // 
            this.nudRecCmdTimeoutMS.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudRecCmdTimeoutMS.Location = new System.Drawing.Point(155, 75);
            this.nudRecCmdTimeoutMS.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudRecCmdTimeoutMS.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudRecCmdTimeoutMS.Name = "nudRecCmdTimeoutMS";
            this.nudRecCmdTimeoutMS.Size = new System.Drawing.Size(78, 20);
            this.nudRecCmdTimeoutMS.TabIndex = 1;
            this.nudRecCmdTimeoutMS.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudRecCmdTimeoutMS.ValueChanged += new System.EventHandler(this.nudRecCmdTimeoutMS_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Connection Timeout (ms)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Command Timeout (ms)";
            // 
            // cmdSaveRecConnTimeoutMS
            // 
            this.cmdSaveRecConnTimeoutMS.Location = new System.Drawing.Point(247, 34);
            this.cmdSaveRecConnTimeoutMS.Name = "cmdSaveRecConnTimeoutMS";
            this.cmdSaveRecConnTimeoutMS.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveRecConnTimeoutMS.TabIndex = 4;
            this.cmdSaveRecConnTimeoutMS.Text = "Save";
            this.cmdSaveRecConnTimeoutMS.UseVisualStyleBackColor = true;
            this.cmdSaveRecConnTimeoutMS.Visible = false;
            this.cmdSaveRecConnTimeoutMS.Click += new System.EventHandler(this.cmdSaveRecConnTimeoutMS_Click);
            // 
            // cmdSaveRecCmdTimeoutMS
            // 
            this.cmdSaveRecCmdTimeoutMS.Location = new System.Drawing.Point(247, 73);
            this.cmdSaveRecCmdTimeoutMS.Name = "cmdSaveRecCmdTimeoutMS";
            this.cmdSaveRecCmdTimeoutMS.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveRecCmdTimeoutMS.TabIndex = 5;
            this.cmdSaveRecCmdTimeoutMS.Text = "Save";
            this.cmdSaveRecCmdTimeoutMS.UseVisualStyleBackColor = true;
            this.cmdSaveRecCmdTimeoutMS.Visible = false;
            this.cmdSaveRecCmdTimeoutMS.Click += new System.EventHandler(this.cmdSaveRecCmdTimeoutMS_Click);
            // 
            // cmdSaveRecBadCmdLimit
            // 
            this.cmdSaveRecBadCmdLimit.Location = new System.Drawing.Point(247, 112);
            this.cmdSaveRecBadCmdLimit.Name = "cmdSaveRecBadCmdLimit";
            this.cmdSaveRecBadCmdLimit.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveRecBadCmdLimit.TabIndex = 8;
            this.cmdSaveRecBadCmdLimit.Text = "Save";
            this.cmdSaveRecBadCmdLimit.UseVisualStyleBackColor = true;
            this.cmdSaveRecBadCmdLimit.Visible = false;
            this.cmdSaveRecBadCmdLimit.Click += new System.EventHandler(this.cmdSaveRecBadCmdLimit_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(49, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Bad Command Limit";
            // 
            // nudRecBadCmdLimit
            // 
            this.nudRecBadCmdLimit.Location = new System.Drawing.Point(155, 114);
            this.nudRecBadCmdLimit.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.nudRecBadCmdLimit.Name = "nudRecBadCmdLimit";
            this.nudRecBadCmdLimit.Size = new System.Drawing.Size(78, 20);
            this.nudRecBadCmdLimit.TabIndex = 6;
            this.nudRecBadCmdLimit.ValueChanged += new System.EventHandler(this.nudRecBadCmdLimit_ValueChanged);
            // 
            // cmbRecVerboseDebugging
            // 
            this.cmbRecVerboseDebugging.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRecVerboseDebugging.FormattingEnabled = true;
            this.cmbRecVerboseDebugging.Items.AddRange(new object[] {
            "Disabled",
            "Envelope",
            "Full"});
            this.cmbRecVerboseDebugging.Location = new System.Drawing.Point(155, 150);
            this.cmbRecVerboseDebugging.Name = "cmbRecVerboseDebugging";
            this.cmbRecVerboseDebugging.Size = new System.Drawing.Size(78, 21);
            this.cmbRecVerboseDebugging.TabIndex = 9;
            this.cmbRecVerboseDebugging.SelectedIndexChanged += new System.EventHandler(this.cmbRecVerboseDebugging_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(48, 153);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Verbose Debugging";
            // 
            // txtRecLogPath
            // 
            this.txtRecLogPath.Location = new System.Drawing.Point(155, 187);
            this.txtRecLogPath.Name = "txtRecLogPath";
            this.txtRecLogPath.Size = new System.Drawing.Size(252, 20);
            this.txtRecLogPath.TabIndex = 11;
            this.txtRecLogPath.TextChanged += new System.EventHandler(this.txtRecLogPath_TextChanged);
            // 
            // cmdSaveRecVerboseDebugging
            // 
            this.cmdSaveRecVerboseDebugging.Location = new System.Drawing.Point(247, 149);
            this.cmdSaveRecVerboseDebugging.Name = "cmdSaveRecVerboseDebugging";
            this.cmdSaveRecVerboseDebugging.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveRecVerboseDebugging.TabIndex = 12;
            this.cmdSaveRecVerboseDebugging.Text = "Save";
            this.cmdSaveRecVerboseDebugging.UseVisualStyleBackColor = true;
            this.cmdSaveRecVerboseDebugging.Visible = false;
            this.cmdSaveRecVerboseDebugging.Click += new System.EventHandler(this.cmdSaveRecVerboseDebugging_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(99, 190);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Log Path";
            // 
            // cmdSaveRecLogPath
            // 
            this.cmdSaveRecLogPath.Location = new System.Drawing.Point(413, 185);
            this.cmdSaveRecLogPath.Name = "cmdSaveRecLogPath";
            this.cmdSaveRecLogPath.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveRecLogPath.TabIndex = 14;
            this.cmdSaveRecLogPath.Text = "Save";
            this.cmdSaveRecLogPath.UseVisualStyleBackColor = true;
            this.cmdSaveRecLogPath.Visible = false;
            this.cmdSaveRecLogPath.Click += new System.EventHandler(this.cmdSaveRecLogPath_Click);
            // 
            // ucSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpReceiver);
            this.Controls.Add(this.chkPurgePage);
            this.Controls.Add(this.chkMessagePage);
            this.Controls.Add(this.chkSenderPage);
            this.Controls.Add(this.chkReceiverPage);
            this.Controls.Add(this.grpPurge);
            this.Controls.Add(this.grpMessage);
            this.Controls.Add(this.grpSender);
            this.Name = "ucSettings";
            this.Size = new System.Drawing.Size(649, 494);
            this.grpReceiver.ResumeLayout(false);
            this.grpReceiver.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecConnTimeoutMS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecCmdTimeoutMS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecBadCmdLimit)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox grpReceiver;
        private System.Windows.Forms.GroupBox grpSender;
        private System.Windows.Forms.GroupBox grpMessage;
        private System.Windows.Forms.GroupBox grpPurge;
        private System.Windows.Forms.CheckBox chkReceiverPage;
        private System.Windows.Forms.CheckBox chkSenderPage;
        private System.Windows.Forms.CheckBox chkMessagePage;
        private System.Windows.Forms.CheckBox chkPurgePage;
        private System.Windows.Forms.Button cmdSaveRecCmdTimeoutMS;
        private System.Windows.Forms.Button cmdSaveRecConnTimeoutMS;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudRecCmdTimeoutMS;
        private System.Windows.Forms.NumericUpDown nudRecConnTimeoutMS;
        private System.Windows.Forms.Button cmdSaveRecBadCmdLimit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudRecBadCmdLimit;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbRecVerboseDebugging;
        private System.Windows.Forms.Button cmdSaveRecVerboseDebugging;
        private System.Windows.Forms.TextBox txtRecLogPath;
        private System.Windows.Forms.Button cmdSaveRecLogPath;
        private System.Windows.Forms.Label label5;
    }
}
