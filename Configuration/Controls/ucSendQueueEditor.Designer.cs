
namespace SMTPRelay.Configuration.Controls
{
    partial class ucSendQueueEditor
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
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lstQueue = new System.Windows.Forms.ListView();
            this.colFrom = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDateReceived = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colRetryAfter = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colAttemptCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstQueue);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(703, 504);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Email Send Queue";
            // 
            // lstQueue
            // 
            this.lstQueue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstQueue.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFrom,
            this.colTo,
            this.colDateReceived,
            this.colRetryAfter,
            this.colAttemptCount});
            this.lstQueue.FullRowSelect = true;
            this.lstQueue.HideSelection = false;
            this.lstQueue.Location = new System.Drawing.Point(6, 19);
            this.lstQueue.MultiSelect = false;
            this.lstQueue.Name = "lstQueue";
            this.lstQueue.Size = new System.Drawing.Size(691, 479);
            this.lstQueue.TabIndex = 0;
            this.lstQueue.UseCompatibleStateImageBehavior = false;
            this.lstQueue.View = System.Windows.Forms.View.Details;
            // 
            // colFrom
            // 
            this.colFrom.Text = "From";
            this.colFrom.Width = 141;
            // 
            // colTo
            // 
            this.colTo.Text = "To";
            this.colTo.Width = 148;
            // 
            // colDateReceived
            // 
            this.colDateReceived.Text = "Date Received";
            this.colDateReceived.Width = 89;
            // 
            // colRetryAfter
            // 
            this.colRetryAfter.Text = "Retry After";
            this.colRetryAfter.Width = 90;
            // 
            // colAttemptCount
            // 
            this.colAttemptCount.Text = "Tries";
            this.colAttemptCount.Width = 50;
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Interval = 1000;
            this.tmrRefresh.Tick += new System.EventHandler(this.tmrRefresh_Tick);
            // 
            // ucSendQueueEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "ucSendQueueEditor";
            this.Size = new System.Drawing.Size(703, 504);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView lstQueue;
        private System.Windows.Forms.ColumnHeader colFrom;
        private System.Windows.Forms.ColumnHeader colTo;
        private System.Windows.Forms.ColumnHeader colDateReceived;
        private System.Windows.Forms.ColumnHeader colRetryAfter;
        private System.Windows.Forms.ColumnHeader colAttemptCount;
        private System.Windows.Forms.Timer tmrRefresh;
    }
}
