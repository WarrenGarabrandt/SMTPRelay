using SMTPRelay.Database;
using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMTPRelay.Configuration.Controls
{
    public partial class ucSendQueueEditor : UserControl
    {
        public ucSendQueueEditor()
        {
            InitializeComponent();
        }

        public void RefreshUI()
        {
            tmrRefresh.Enabled = true;
        }

        public void SuspendUI()
        {
            tmrRefresh.Enabled = false;
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            lstQueue.Items.Clear();
            try
            {
                List<ListViewItem> MailQueue = new List<ListViewItem>();
                var dbqueue = SQLiteDB.MailQueue_QueryView();
                foreach (var dbq in dbqueue)
                {
                    ListViewItem q = new ListViewItem(dbq.Sender);
                    q.SubItems.Add(dbq.Recipients);
                    q.SubItems.Add(dbq.DateReceived.ToString());
                    q.SubItems.Add(dbq.DateRetry.ToString());
                    q.SubItems.Add(dbq.AttemptCount.ToString());
                    MailQueue.Add(q);
                }
                lstQueue.Items.AddRange(MailQueue.ToArray());
            }
            catch
            { }
        }
    }
}
