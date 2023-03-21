
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
            this.cmdSaveRecLogPath = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.cmdSaveRecVerboseDebugging = new System.Windows.Forms.Button();
            this.txtRecLogPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbRecVerboseDebugging = new System.Windows.Forms.ComboBox();
            this.cmdSaveRecBadCmdLimit = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.nudRecBadCmdLimit = new System.Windows.Forms.NumericUpDown();
            this.cmdSaveRecCmdTimeoutMS = new System.Windows.Forms.Button();
            this.cmdSaveRecConnTimeoutMS = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.nudRecCmdTimeoutMS = new System.Windows.Forms.NumericUpDown();
            this.nudRecConnTimeoutMS = new System.Windows.Forms.NumericUpDown();
            this.grpSender = new System.Windows.Forms.GroupBox();
            this.txtSndHostname = new System.Windows.Forms.TextBox();
            this.cmdSaveSndLogPath = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.cmdSaveSndVerboseDebugging = new System.Windows.Forms.Button();
            this.txtSndLogPath = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbSndVerboseDebugging = new System.Windows.Forms.ComboBox();
            this.cmdSaveSndIntervalMS = new System.Windows.Forms.Button();
            this.cmdSaveSndHostname = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.nudSndIntervalMS = new System.Windows.Forms.NumericUpDown();
            this.grpMessage = new System.Windows.Forms.GroupBox();
            this.cmdSaveMsgFailedRetentionMins = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.nudMsgFailedRetentionMins = new System.Windows.Forms.NumericUpDown();
            this.cmdSaveMsgRetentionMins = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.nudMsgRetentionMins = new System.Windows.Forms.NumericUpDown();
            this.cmdSaveMsgChunkSize = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.nudMsgChunkSize = new System.Windows.Forms.NumericUpDown();
            this.cmdSaveMsgRecipCount = new System.Windows.Forms.Button();
            this.cmdSaveMsgMaxSize = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.nudMsgRecipCount = new System.Windows.Forms.NumericUpDown();
            this.nudMsgMaxSize = new System.Windows.Forms.NumericUpDown();
            this.grpPurge = new System.Windows.Forms.GroupBox();
            this.cmdSavePrgLogPath = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.txtPrgLogPath = new System.Windows.Forms.TextBox();
            this.cmbPrgLogging = new System.Windows.Forms.ComboBox();
            this.cmdSavePrgLogging = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.cmdSavePrgBatchSize = new System.Windows.Forms.Button();
            this.cmdSavePrgFrequencyMins = new System.Windows.Forms.Button();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.nudPrgBatchSize = new System.Windows.Forms.NumericUpDown();
            this.nudPrgFrequencyMins = new System.Windows.Forms.NumericUpDown();
            this.chkReceiverPage = new System.Windows.Forms.CheckBox();
            this.chkSenderPage = new System.Windows.Forms.CheckBox();
            this.chkMessagePage = new System.Windows.Forms.CheckBox();
            this.chkPurgePage = new System.Windows.Forms.CheckBox();
            this.grpReceiver.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecBadCmdLimit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecCmdTimeoutMS)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecConnTimeoutMS)).BeginInit();
            this.grpSender.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSndIntervalMS)).BeginInit();
            this.grpMessage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgFailedRetentionMins)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgRetentionMins)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgChunkSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgRecipCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgMaxSize)).BeginInit();
            this.grpPurge.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrgBatchSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrgFrequencyMins)).BeginInit();
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
            // cmdSaveRecLogPath
            // 
            this.cmdSaveRecLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSaveRecLogPath.Location = new System.Drawing.Point(413, 185);
            this.cmdSaveRecLogPath.Name = "cmdSaveRecLogPath";
            this.cmdSaveRecLogPath.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveRecLogPath.TabIndex = 14;
            this.cmdSaveRecLogPath.Text = "Save";
            this.cmdSaveRecLogPath.UseVisualStyleBackColor = true;
            this.cmdSaveRecLogPath.Visible = false;
            this.cmdSaveRecLogPath.Click += new System.EventHandler(this.cmdSaveRecLogPath_Click);
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
            // cmdSaveRecVerboseDebugging
            // 
            this.cmdSaveRecVerboseDebugging.Location = new System.Drawing.Point(255, 149);
            this.cmdSaveRecVerboseDebugging.Name = "cmdSaveRecVerboseDebugging";
            this.cmdSaveRecVerboseDebugging.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveRecVerboseDebugging.TabIndex = 12;
            this.cmdSaveRecVerboseDebugging.Text = "Save";
            this.cmdSaveRecVerboseDebugging.UseVisualStyleBackColor = true;
            this.cmdSaveRecVerboseDebugging.Visible = false;
            this.cmdSaveRecVerboseDebugging.Click += new System.EventHandler(this.cmdSaveRecVerboseDebugging_Click);
            // 
            // txtRecLogPath
            // 
            this.txtRecLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRecLogPath.Location = new System.Drawing.Point(155, 187);
            this.txtRecLogPath.Name = "txtRecLogPath";
            this.txtRecLogPath.Size = new System.Drawing.Size(252, 20);
            this.txtRecLogPath.TabIndex = 11;
            this.txtRecLogPath.TextChanged += new System.EventHandler(this.txtRecLogPath_TextChanged);
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
            this.cmbRecVerboseDebugging.Size = new System.Drawing.Size(86, 21);
            this.cmbRecVerboseDebugging.TabIndex = 9;
            this.cmbRecVerboseDebugging.SelectedIndexChanged += new System.EventHandler(this.cmbRecVerboseDebugging_SelectedIndexChanged);
            // 
            // cmdSaveRecBadCmdLimit
            // 
            this.cmdSaveRecBadCmdLimit.Location = new System.Drawing.Point(255, 112);
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
            this.nudRecBadCmdLimit.Size = new System.Drawing.Size(86, 20);
            this.nudRecBadCmdLimit.TabIndex = 6;
            this.nudRecBadCmdLimit.ValueChanged += new System.EventHandler(this.nudRecBadCmdLimit_ValueChanged);
            // 
            // cmdSaveRecCmdTimeoutMS
            // 
            this.cmdSaveRecCmdTimeoutMS.Location = new System.Drawing.Point(255, 73);
            this.cmdSaveRecCmdTimeoutMS.Name = "cmdSaveRecCmdTimeoutMS";
            this.cmdSaveRecCmdTimeoutMS.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveRecCmdTimeoutMS.TabIndex = 5;
            this.cmdSaveRecCmdTimeoutMS.Text = "Save";
            this.cmdSaveRecCmdTimeoutMS.UseVisualStyleBackColor = true;
            this.cmdSaveRecCmdTimeoutMS.Visible = false;
            this.cmdSaveRecCmdTimeoutMS.Click += new System.EventHandler(this.cmdSaveRecCmdTimeoutMS_Click);
            // 
            // cmdSaveRecConnTimeoutMS
            // 
            this.cmdSaveRecConnTimeoutMS.Location = new System.Drawing.Point(255, 34);
            this.cmdSaveRecConnTimeoutMS.Name = "cmdSaveRecConnTimeoutMS";
            this.cmdSaveRecConnTimeoutMS.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveRecConnTimeoutMS.TabIndex = 4;
            this.cmdSaveRecConnTimeoutMS.Text = "Save";
            this.cmdSaveRecConnTimeoutMS.UseVisualStyleBackColor = true;
            this.cmdSaveRecConnTimeoutMS.Visible = false;
            this.cmdSaveRecConnTimeoutMS.Click += new System.EventHandler(this.cmdSaveRecConnTimeoutMS_Click);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Connection Timeout (ms)";
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
            this.nudRecCmdTimeoutMS.Size = new System.Drawing.Size(86, 20);
            this.nudRecCmdTimeoutMS.TabIndex = 1;
            this.nudRecCmdTimeoutMS.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudRecCmdTimeoutMS.ValueChanged += new System.EventHandler(this.nudRecCmdTimeoutMS_ValueChanged);
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
            this.nudRecConnTimeoutMS.Size = new System.Drawing.Size(86, 20);
            this.nudRecConnTimeoutMS.TabIndex = 0;
            this.nudRecConnTimeoutMS.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudRecConnTimeoutMS.ValueChanged += new System.EventHandler(this.nudRecConnTimeoutMS_ValueChanged);
            // 
            // grpSender
            // 
            this.grpSender.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSender.Controls.Add(this.txtSndHostname);
            this.grpSender.Controls.Add(this.cmdSaveSndLogPath);
            this.grpSender.Controls.Add(this.label6);
            this.grpSender.Controls.Add(this.cmdSaveSndVerboseDebugging);
            this.grpSender.Controls.Add(this.txtSndLogPath);
            this.grpSender.Controls.Add(this.label7);
            this.grpSender.Controls.Add(this.cmbSndVerboseDebugging);
            this.grpSender.Controls.Add(this.cmdSaveSndIntervalMS);
            this.grpSender.Controls.Add(this.cmdSaveSndHostname);
            this.grpSender.Controls.Add(this.label9);
            this.grpSender.Controls.Add(this.label10);
            this.grpSender.Controls.Add(this.nudSndIntervalMS);
            this.grpSender.Location = new System.Drawing.Point(137, 3);
            this.grpSender.Name = "grpSender";
            this.grpSender.Size = new System.Drawing.Size(509, 488);
            this.grpSender.TabIndex = 6;
            this.grpSender.TabStop = false;
            this.grpSender.Text = "SMTP Sender Settings";
            this.grpSender.Visible = false;
            // 
            // txtSndHostname
            // 
            this.txtSndHostname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSndHostname.Location = new System.Drawing.Point(155, 36);
            this.txtSndHostname.Name = "txtSndHostname";
            this.txtSndHostname.Size = new System.Drawing.Size(252, 20);
            this.txtSndHostname.TabIndex = 30;
            this.txtSndHostname.TextChanged += new System.EventHandler(this.txtSndHostname_TextChanged);
            // 
            // cmdSaveSndLogPath
            // 
            this.cmdSaveSndLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSaveSndLogPath.Location = new System.Drawing.Point(412, 150);
            this.cmdSaveSndLogPath.Name = "cmdSaveSndLogPath";
            this.cmdSaveSndLogPath.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveSndLogPath.TabIndex = 29;
            this.cmdSaveSndLogPath.Text = "Save";
            this.cmdSaveSndLogPath.UseVisualStyleBackColor = true;
            this.cmdSaveSndLogPath.Visible = false;
            this.cmdSaveSndLogPath.Click += new System.EventHandler(this.cmdSaveSndLogPath_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(98, 155);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 28;
            this.label6.Text = "Log Path";
            // 
            // cmdSaveSndVerboseDebugging
            // 
            this.cmdSaveSndVerboseDebugging.Location = new System.Drawing.Point(254, 114);
            this.cmdSaveSndVerboseDebugging.Name = "cmdSaveSndVerboseDebugging";
            this.cmdSaveSndVerboseDebugging.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveSndVerboseDebugging.TabIndex = 27;
            this.cmdSaveSndVerboseDebugging.Text = "Save";
            this.cmdSaveSndVerboseDebugging.UseVisualStyleBackColor = true;
            this.cmdSaveSndVerboseDebugging.Visible = false;
            this.cmdSaveSndVerboseDebugging.Click += new System.EventHandler(this.cmdSaveSndVerboseDebugging_Click);
            // 
            // txtSndLogPath
            // 
            this.txtSndLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSndLogPath.Location = new System.Drawing.Point(154, 152);
            this.txtSndLogPath.Name = "txtSndLogPath";
            this.txtSndLogPath.Size = new System.Drawing.Size(252, 20);
            this.txtSndLogPath.TabIndex = 26;
            this.txtSndLogPath.TextChanged += new System.EventHandler(this.txtSndLogPath_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(47, 118);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Verbose Debugging";
            // 
            // cmbSndVerboseDebugging
            // 
            this.cmbSndVerboseDebugging.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSndVerboseDebugging.FormattingEnabled = true;
            this.cmbSndVerboseDebugging.Items.AddRange(new object[] {
            "Disabled",
            "Envelope",
            "Full"});
            this.cmbSndVerboseDebugging.Location = new System.Drawing.Point(154, 115);
            this.cmbSndVerboseDebugging.Name = "cmbSndVerboseDebugging";
            this.cmbSndVerboseDebugging.Size = new System.Drawing.Size(86, 21);
            this.cmbSndVerboseDebugging.TabIndex = 24;
            this.cmbSndVerboseDebugging.SelectedIndexChanged += new System.EventHandler(this.cmbSndVerboseDebugging_SelectedIndexChanged);
            // 
            // cmdSaveSndIntervalMS
            // 
            this.cmdSaveSndIntervalMS.Location = new System.Drawing.Point(254, 74);
            this.cmdSaveSndIntervalMS.Name = "cmdSaveSndIntervalMS";
            this.cmdSaveSndIntervalMS.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveSndIntervalMS.TabIndex = 20;
            this.cmdSaveSndIntervalMS.Text = "Save";
            this.cmdSaveSndIntervalMS.UseVisualStyleBackColor = true;
            this.cmdSaveSndIntervalMS.Visible = false;
            this.cmdSaveSndIntervalMS.Click += new System.EventHandler(this.cmdSaveSndIntervalMS_Click);
            // 
            // cmdSaveSndHostname
            // 
            this.cmdSaveSndHostname.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSaveSndHostname.Location = new System.Drawing.Point(412, 35);
            this.cmdSaveSndHostname.Name = "cmdSaveSndHostname";
            this.cmdSaveSndHostname.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveSndHostname.TabIndex = 19;
            this.cmdSaveSndHostname.Text = "Save";
            this.cmdSaveSndHostname.UseVisualStyleBackColor = true;
            this.cmdSaveSndHostname.Visible = false;
            this.cmdSaveSndHostname.Click += new System.EventHandler(this.cmdSaveSndHostname_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 78);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(135, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Send Refresh Interval (MS)";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(56, 39);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(92, 13);
            this.label10.TabIndex = 17;
            this.label10.Text = "Sender Hostname";
            // 
            // nudSndIntervalMS
            // 
            this.nudSndIntervalMS.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudSndIntervalMS.Location = new System.Drawing.Point(154, 76);
            this.nudSndIntervalMS.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudSndIntervalMS.Minimum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudSndIntervalMS.Name = "nudSndIntervalMS";
            this.nudSndIntervalMS.Size = new System.Drawing.Size(86, 20);
            this.nudSndIntervalMS.TabIndex = 16;
            this.nudSndIntervalMS.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudSndIntervalMS.ValueChanged += new System.EventHandler(this.nudSndIntervalMS_ValueChanged);
            // 
            // grpMessage
            // 
            this.grpMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMessage.Controls.Add(this.cmdSaveMsgFailedRetentionMins);
            this.grpMessage.Controls.Add(this.label11);
            this.grpMessage.Controls.Add(this.nudMsgFailedRetentionMins);
            this.grpMessage.Controls.Add(this.cmdSaveMsgRetentionMins);
            this.grpMessage.Controls.Add(this.label8);
            this.grpMessage.Controls.Add(this.nudMsgRetentionMins);
            this.grpMessage.Controls.Add(this.cmdSaveMsgChunkSize);
            this.grpMessage.Controls.Add(this.label12);
            this.grpMessage.Controls.Add(this.nudMsgChunkSize);
            this.grpMessage.Controls.Add(this.cmdSaveMsgRecipCount);
            this.grpMessage.Controls.Add(this.cmdSaveMsgMaxSize);
            this.grpMessage.Controls.Add(this.label13);
            this.grpMessage.Controls.Add(this.label14);
            this.grpMessage.Controls.Add(this.nudMsgRecipCount);
            this.grpMessage.Controls.Add(this.nudMsgMaxSize);
            this.grpMessage.Location = new System.Drawing.Point(137, 3);
            this.grpMessage.Name = "grpMessage";
            this.grpMessage.Size = new System.Drawing.Size(509, 488);
            this.grpMessage.TabIndex = 7;
            this.grpMessage.TabStop = false;
            this.grpMessage.Text = "Message Settings";
            this.grpMessage.Visible = false;
            // 
            // cmdSaveMsgFailedRetentionMins
            // 
            this.cmdSaveMsgFailedRetentionMins.Location = new System.Drawing.Point(254, 187);
            this.cmdSaveMsgFailedRetentionMins.Name = "cmdSaveMsgFailedRetentionMins";
            this.cmdSaveMsgFailedRetentionMins.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveMsgFailedRetentionMins.TabIndex = 29;
            this.cmdSaveMsgFailedRetentionMins.Text = "Save";
            this.cmdSaveMsgFailedRetentionMins.UseVisualStyleBackColor = true;
            this.cmdSaveMsgFailedRetentionMins.Visible = false;
            this.cmdSaveMsgFailedRetentionMins.Click += new System.EventHandler(this.cmdSaveMsgFailedRetentionMins_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(24, 190);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(124, 13);
            this.label11.TabIndex = 28;
            this.label11.Text = "Failed Retention Minutes";
            // 
            // nudMsgFailedRetentionMins
            // 
            this.nudMsgFailedRetentionMins.Increment = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudMsgFailedRetentionMins.Location = new System.Drawing.Point(156, 188);
            this.nudMsgFailedRetentionMins.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudMsgFailedRetentionMins.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMsgFailedRetentionMins.Name = "nudMsgFailedRetentionMins";
            this.nudMsgFailedRetentionMins.Size = new System.Drawing.Size(86, 20);
            this.nudMsgFailedRetentionMins.TabIndex = 27;
            this.nudMsgFailedRetentionMins.Value = new decimal(new int[] {
            4320,
            0,
            0,
            0});
            this.nudMsgFailedRetentionMins.ValueChanged += new System.EventHandler(this.nudMsgFailedRetentionMins_ValueChanged);
            // 
            // cmdSaveMsgRetentionMins
            // 
            this.cmdSaveMsgRetentionMins.Location = new System.Drawing.Point(254, 150);
            this.cmdSaveMsgRetentionMins.Name = "cmdSaveMsgRetentionMins";
            this.cmdSaveMsgRetentionMins.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveMsgRetentionMins.TabIndex = 26;
            this.cmdSaveMsgRetentionMins.Text = "Save";
            this.cmdSaveMsgRetentionMins.UseVisualStyleBackColor = true;
            this.cmdSaveMsgRetentionMins.Visible = false;
            this.cmdSaveMsgRetentionMins.Click += new System.EventHandler(this.cmdSaveMsgRetentionMins_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(27, 154);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(121, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "Send Retention Minutes";
            // 
            // nudMsgRetentionMins
            // 
            this.nudMsgRetentionMins.Increment = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nudMsgRetentionMins.Location = new System.Drawing.Point(156, 152);
            this.nudMsgRetentionMins.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudMsgRetentionMins.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMsgRetentionMins.Name = "nudMsgRetentionMins";
            this.nudMsgRetentionMins.Size = new System.Drawing.Size(86, 20);
            this.nudMsgRetentionMins.TabIndex = 24;
            this.nudMsgRetentionMins.Value = new decimal(new int[] {
            1440,
            0,
            0,
            0});
            this.nudMsgRetentionMins.ValueChanged += new System.EventHandler(this.nudMsgRetentionMins_ValueChanged);
            // 
            // cmdSaveMsgChunkSize
            // 
            this.cmdSaveMsgChunkSize.Location = new System.Drawing.Point(253, 112);
            this.cmdSaveMsgChunkSize.Name = "cmdSaveMsgChunkSize";
            this.cmdSaveMsgChunkSize.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveMsgChunkSize.TabIndex = 23;
            this.cmdSaveMsgChunkSize.Text = "Save";
            this.cmdSaveMsgChunkSize.UseVisualStyleBackColor = true;
            this.cmdSaveMsgChunkSize.Visible = false;
            this.cmdSaveMsgChunkSize.Click += new System.EventHandler(this.cmdSaveMsgChunkSize_Click);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(27, 116);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(121, 13);
            this.label12.TabIndex = 22;
            this.label12.Text = "Data Chunk Size (bytes)";
            // 
            // nudMsgChunkSize
            // 
            this.nudMsgChunkSize.Increment = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.nudMsgChunkSize.Location = new System.Drawing.Point(156, 114);
            this.nudMsgChunkSize.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudMsgChunkSize.Minimum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.nudMsgChunkSize.Name = "nudMsgChunkSize";
            this.nudMsgChunkSize.Size = new System.Drawing.Size(86, 20);
            this.nudMsgChunkSize.TabIndex = 21;
            this.nudMsgChunkSize.Value = new decimal(new int[] {
            32768,
            0,
            0,
            0});
            this.nudMsgChunkSize.ValueChanged += new System.EventHandler(this.nudMsgChunkSize_ValueChanged);
            // 
            // cmdSaveMsgRecipCount
            // 
            this.cmdSaveMsgRecipCount.Location = new System.Drawing.Point(253, 73);
            this.cmdSaveMsgRecipCount.Name = "cmdSaveMsgRecipCount";
            this.cmdSaveMsgRecipCount.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveMsgRecipCount.TabIndex = 20;
            this.cmdSaveMsgRecipCount.Text = "Save";
            this.cmdSaveMsgRecipCount.UseVisualStyleBackColor = true;
            this.cmdSaveMsgRecipCount.Visible = false;
            this.cmdSaveMsgRecipCount.Click += new System.EventHandler(this.cmdSaveMsgRecipCount_Click);
            // 
            // cmdSaveMsgMaxSize
            // 
            this.cmdSaveMsgMaxSize.Location = new System.Drawing.Point(253, 34);
            this.cmdSaveMsgMaxSize.Name = "cmdSaveMsgMaxSize";
            this.cmdSaveMsgMaxSize.Size = new System.Drawing.Size(75, 23);
            this.cmdSaveMsgMaxSize.TabIndex = 19;
            this.cmdSaveMsgMaxSize.Text = "Save";
            this.cmdSaveMsgMaxSize.UseVisualStyleBackColor = true;
            this.cmdSaveMsgMaxSize.Visible = false;
            this.cmdSaveMsgMaxSize.Click += new System.EventHandler(this.cmdSaveMsgMaxSize_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(42, 77);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(106, 13);
            this.label13.TabIndex = 18;
            this.label13.Text = "Max Recipient Count";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(40, 38);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(108, 13);
            this.label14.TabIndex = 17;
            this.label14.Text = "Maximum Size (bytes)";
            // 
            // nudMsgRecipCount
            // 
            this.nudMsgRecipCount.Location = new System.Drawing.Point(156, 75);
            this.nudMsgRecipCount.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudMsgRecipCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMsgRecipCount.Name = "nudMsgRecipCount";
            this.nudMsgRecipCount.Size = new System.Drawing.Size(86, 20);
            this.nudMsgRecipCount.TabIndex = 16;
            this.nudMsgRecipCount.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.nudMsgRecipCount.ValueChanged += new System.EventHandler(this.nudMsgRecipCount_ValueChanged);
            // 
            // nudMsgMaxSize
            // 
            this.nudMsgMaxSize.Increment = new decimal(new int[] {
            32768,
            0,
            0,
            0});
            this.nudMsgMaxSize.Location = new System.Drawing.Point(155, 36);
            this.nudMsgMaxSize.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudMsgMaxSize.Minimum = new decimal(new int[] {
            32768,
            0,
            0,
            0});
            this.nudMsgMaxSize.Name = "nudMsgMaxSize";
            this.nudMsgMaxSize.Size = new System.Drawing.Size(86, 20);
            this.nudMsgMaxSize.TabIndex = 15;
            this.nudMsgMaxSize.Value = new decimal(new int[] {
            32768,
            0,
            0,
            0});
            this.nudMsgMaxSize.ValueChanged += new System.EventHandler(this.nudMsgMaxSize_ValueChanged);
            // 
            // grpPurge
            // 
            this.grpPurge.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPurge.Controls.Add(this.cmdSavePrgLogPath);
            this.grpPurge.Controls.Add(this.label20);
            this.grpPurge.Controls.Add(this.txtPrgLogPath);
            this.grpPurge.Controls.Add(this.cmbPrgLogging);
            this.grpPurge.Controls.Add(this.cmdSavePrgLogging);
            this.grpPurge.Controls.Add(this.label17);
            this.grpPurge.Controls.Add(this.cmdSavePrgBatchSize);
            this.grpPurge.Controls.Add(this.cmdSavePrgFrequencyMins);
            this.grpPurge.Controls.Add(this.label18);
            this.grpPurge.Controls.Add(this.label19);
            this.grpPurge.Controls.Add(this.nudPrgBatchSize);
            this.grpPurge.Controls.Add(this.nudPrgFrequencyMins);
            this.grpPurge.Location = new System.Drawing.Point(137, 3);
            this.grpPurge.Name = "grpPurge";
            this.grpPurge.Size = new System.Drawing.Size(509, 488);
            this.grpPurge.TabIndex = 8;
            this.grpPurge.TabStop = false;
            this.grpPurge.Text = "Purge Settings";
            this.grpPurge.Visible = false;
            // 
            // cmdSavePrgLogPath
            // 
            this.cmdSavePrgLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdSavePrgLogPath.Location = new System.Drawing.Point(414, 148);
            this.cmdSavePrgLogPath.Name = "cmdSavePrgLogPath";
            this.cmdSavePrgLogPath.Size = new System.Drawing.Size(75, 23);
            this.cmdSavePrgLogPath.TabIndex = 48;
            this.cmdSavePrgLogPath.Text = "Save";
            this.cmdSavePrgLogPath.UseVisualStyleBackColor = true;
            this.cmdSavePrgLogPath.Visible = false;
            this.cmdSavePrgLogPath.Click += new System.EventHandler(this.cmdSavePrgLogPath_Click);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(100, 153);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(50, 13);
            this.label20.TabIndex = 47;
            this.label20.Text = "Log Path";
            // 
            // txtPrgLogPath
            // 
            this.txtPrgLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPrgLogPath.Location = new System.Drawing.Point(156, 150);
            this.txtPrgLogPath.Name = "txtPrgLogPath";
            this.txtPrgLogPath.Size = new System.Drawing.Size(252, 20);
            this.txtPrgLogPath.TabIndex = 46;
            this.txtPrgLogPath.TextChanged += new System.EventHandler(this.txtPrgLogPath_TextChanged);
            // 
            // cmbPrgLogging
            // 
            this.cmbPrgLogging.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPrgLogging.FormattingEnabled = true;
            this.cmbPrgLogging.Items.AddRange(new object[] {
            "Disabled",
            "Enabled"});
            this.cmbPrgLogging.Location = new System.Drawing.Point(156, 113);
            this.cmbPrgLogging.Name = "cmbPrgLogging";
            this.cmbPrgLogging.Size = new System.Drawing.Size(86, 21);
            this.cmbPrgLogging.TabIndex = 45;
            this.cmbPrgLogging.SelectedIndexChanged += new System.EventHandler(this.cmbPrgLogging_SelectedIndexChanged);
            // 
            // cmdSavePrgLogging
            // 
            this.cmdSavePrgLogging.Location = new System.Drawing.Point(253, 112);
            this.cmdSavePrgLogging.Name = "cmdSavePrgLogging";
            this.cmdSavePrgLogging.Size = new System.Drawing.Size(75, 23);
            this.cmdSavePrgLogging.TabIndex = 38;
            this.cmdSavePrgLogging.Text = "Save";
            this.cmdSavePrgLogging.UseVisualStyleBackColor = true;
            this.cmdSavePrgLogging.Visible = false;
            this.cmdSavePrgLogging.Click += new System.EventHandler(this.cmdSavePrgLogging_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(68, 117);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(80, 13);
            this.label17.TabIndex = 37;
            this.label17.Text = "Debug Logging";
            // 
            // cmdSavePrgBatchSize
            // 
            this.cmdSavePrgBatchSize.Location = new System.Drawing.Point(253, 73);
            this.cmdSavePrgBatchSize.Name = "cmdSavePrgBatchSize";
            this.cmdSavePrgBatchSize.Size = new System.Drawing.Size(75, 23);
            this.cmdSavePrgBatchSize.TabIndex = 35;
            this.cmdSavePrgBatchSize.Text = "Save";
            this.cmdSavePrgBatchSize.UseVisualStyleBackColor = true;
            this.cmdSavePrgBatchSize.Visible = false;
            this.cmdSavePrgBatchSize.Click += new System.EventHandler(this.cmdSavePrgBatchSize_Click);
            // 
            // cmdSavePrgFrequencyMins
            // 
            this.cmdSavePrgFrequencyMins.Location = new System.Drawing.Point(253, 34);
            this.cmdSavePrgFrequencyMins.Name = "cmdSavePrgFrequencyMins";
            this.cmdSavePrgFrequencyMins.Size = new System.Drawing.Size(75, 23);
            this.cmdSavePrgFrequencyMins.TabIndex = 34;
            this.cmdSavePrgFrequencyMins.Text = "Save";
            this.cmdSavePrgFrequencyMins.UseVisualStyleBackColor = true;
            this.cmdSavePrgFrequencyMins.Visible = false;
            this.cmdSavePrgFrequencyMins.Click += new System.EventHandler(this.cmdSavePrgFrequencyMins_Click);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(90, 77);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(58, 13);
            this.label18.TabIndex = 33;
            this.label18.Text = "Batch Size";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(28, 38);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(120, 13);
            this.label19.TabIndex = 32;
            this.label19.Text = "Run Frequency Minutes";
            // 
            // nudPrgBatchSize
            // 
            this.nudPrgBatchSize.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudPrgBatchSize.Location = new System.Drawing.Point(156, 75);
            this.nudPrgBatchSize.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nudPrgBatchSize.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudPrgBatchSize.Name = "nudPrgBatchSize";
            this.nudPrgBatchSize.Size = new System.Drawing.Size(86, 20);
            this.nudPrgBatchSize.TabIndex = 31;
            this.nudPrgBatchSize.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nudPrgBatchSize.ValueChanged += new System.EventHandler(this.nudPrgBatchSize_ValueChanged);
            // 
            // nudPrgFrequencyMins
            // 
            this.nudPrgFrequencyMins.Increment = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudPrgFrequencyMins.Location = new System.Drawing.Point(155, 36);
            this.nudPrgFrequencyMins.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.nudPrgFrequencyMins.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudPrgFrequencyMins.Name = "nudPrgFrequencyMins";
            this.nudPrgFrequencyMins.Size = new System.Drawing.Size(86, 20);
            this.nudPrgFrequencyMins.TabIndex = 30;
            this.nudPrgFrequencyMins.Value = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.nudPrgFrequencyMins.ValueChanged += new System.EventHandler(this.nudPrgFrequencyMins_ValueChanged);
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
            // ucSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkPurgePage);
            this.Controls.Add(this.chkMessagePage);
            this.Controls.Add(this.chkSenderPage);
            this.Controls.Add(this.chkReceiverPage);
            this.Controls.Add(this.grpPurge);
            this.Controls.Add(this.grpMessage);
            this.Controls.Add(this.grpSender);
            this.Controls.Add(this.grpReceiver);
            this.Name = "ucSettings";
            this.Size = new System.Drawing.Size(649, 494);
            this.grpReceiver.ResumeLayout(false);
            this.grpReceiver.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecBadCmdLimit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecCmdTimeoutMS)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudRecConnTimeoutMS)).EndInit();
            this.grpSender.ResumeLayout(false);
            this.grpSender.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSndIntervalMS)).EndInit();
            this.grpMessage.ResumeLayout(false);
            this.grpMessage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgFailedRetentionMins)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgRetentionMins)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgChunkSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgRecipCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMsgMaxSize)).EndInit();
            this.grpPurge.ResumeLayout(false);
            this.grpPurge.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrgBatchSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPrgFrequencyMins)).EndInit();
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
        private System.Windows.Forms.Button cmdSaveSndLogPath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button cmdSaveSndVerboseDebugging;
        private System.Windows.Forms.TextBox txtSndLogPath;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbSndVerboseDebugging;
        private System.Windows.Forms.Button cmdSaveSndIntervalMS;
        private System.Windows.Forms.Button cmdSaveSndHostname;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown nudSndIntervalMS;
        private System.Windows.Forms.TextBox txtSndHostname;
        private System.Windows.Forms.Button cmdSaveMsgChunkSize;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown nudMsgChunkSize;
        private System.Windows.Forms.Button cmdSaveMsgRecipCount;
        private System.Windows.Forms.Button cmdSaveMsgMaxSize;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown nudMsgRecipCount;
        private System.Windows.Forms.NumericUpDown nudMsgMaxSize;
        private System.Windows.Forms.Button cmdSaveMsgFailedRetentionMins;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.NumericUpDown nudMsgFailedRetentionMins;
        private System.Windows.Forms.Button cmdSaveMsgRetentionMins;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nudMsgRetentionMins;
        private System.Windows.Forms.Button cmdSavePrgLogging;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button cmdSavePrgBatchSize;
        private System.Windows.Forms.Button cmdSavePrgFrequencyMins;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.NumericUpDown nudPrgBatchSize;
        private System.Windows.Forms.NumericUpDown nudPrgFrequencyMins;
        private System.Windows.Forms.Button cmdSavePrgLogPath;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox txtPrgLogPath;
        private System.Windows.Forms.ComboBox cmbPrgLogging;
    }
}
