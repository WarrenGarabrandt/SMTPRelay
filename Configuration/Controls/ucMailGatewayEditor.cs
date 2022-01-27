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
    public partial class ucMailGatewayEditor : UserControl
    {
        public ucMailGatewayEditor()
        {
            InitializeComponent();
        }

        private long? SelectedGW = null;
        private bool Refreshing = false;

        public void RefreshUI()
        {
            Refreshing = true;
            lstMailGateways.Items.Clear();
            List<ListViewItem> MailGateways = new List<ListViewItem>();
            var dbGWs = SQLiteDB.MailGateway_GetAll();
            foreach (var dbgw in dbGWs)
            {
                ListViewItem gw = new ListViewItem(dbgw.MailGatewayID.Value.ToString());
                gw.SubItems.Add(dbgw.SMTPServer);
                gw.SubItems.Add(dbgw.Port.ToString());
                gw.SubItems.Add(dbgw.EnableSSL ? "Enabled" : "");
                gw.SubItems.Add(dbgw.Authenticate ? "Enabled" : "");
                gw.SubItems.Add(dbgw.Username);
                gw.SubItems.Add(dbgw.SenderOverride);
                gw.Tag = dbgw.MailGatewayID;
                MailGateways.Add(gw);
            }
            lstMailGateways.Items.AddRange(MailGateways.ToArray());
            if (SelectedGW == null)
            {
                txtSMTPServer.Enabled = false;
                nudPort.Enabled = false;
                chkEnableSSL.Enabled = false;
                txtSenderOverride.Enabled = false;
                chkAuthenticate.Enabled = false;
                txtUsername.Enabled = false;
                txtPassword.Enabled = false;
                cmdSaveChanges.Enabled = false;
                cmdDelete.Enabled = false;
                chkAuthenticate.Enabled = false;
                chkConnectionLimit.Enabled = false;
                nudConnectionLimit.Enabled = false;
                nudConnectionLimit.Value = 3;
            }
            else
            {
                for (int i = 0; i < lstMailGateways.Items.Count; i++)
                {
                    if (((ListViewItem)lstMailGateways.Items[i]).Tag as long? == SelectedGW)
                    {
                        ((ListViewItem)lstMailGateways.Items[i]).Focused = true;
                        ((ListViewItem)lstMailGateways.Items[i]).Selected = true;
                        ((ListViewItem)lstMailGateways.Items[i]).EnsureVisible();
                    }
                }
            }
            Refreshing = false;
        }

        private void ClearEditors()
        {
            txtSMTPServer.Text = "";
            txtSMTPServer.Enabled = false;
            nudPort.Value = 25;
            nudPort.Enabled = false;
            chkEnableSSL.Checked = false;
            chkEnableSSL.Enabled = false;
            txtSenderOverride.Text = "";
            txtSenderOverride.Enabled = false;
            chkAuthenticate.Checked = false;
            chkAuthenticate.Enabled = false;
            txtUsername.Text = "";
            txtUsername.Enabled = false;
            txtPassword.Text = "";
            txtPassword.Enabled = false;
            chkConnectionLimit.Checked = false;
            chkConnectionLimit.Enabled = false;
            nudConnectionLimit.Value = 3;
            nudConnectionLimit.Enabled = false;
            cmdSaveChanges.Enabled = false;
            cmdDelete.Enabled = false;
        }

        private void PopulateSelectedGW()
        {
            tblMailGateway dbGW = SQLiteDB.MailGateway_GetByID(SelectedGW.Value);
            txtSMTPServer.Text = dbGW.SMTPServer;
            nudPort.Value = dbGW.Port;
            chkEnableSSL.Checked = dbGW.EnableSSL;
            txtSenderOverride.Text = dbGW.SenderOverride;
            chkAuthenticate.Checked = dbGW.Authenticate;
            txtUsername.Text = dbGW.Username;
            txtPassword.Text = dbGW.Password;
            chkConnectionLimit.Checked = dbGW.ConnectionLimit.HasValue;
            nudConnectionLimit.Value = dbGW.ConnectionLimit.HasValue ? dbGW.ConnectionLimit.Value : 3;

            txtSMTPServer.Enabled = true;
            nudPort.Enabled = true;
            chkEnableSSL.Enabled = true;
            txtSenderOverride.Enabled = true;
            chkAuthenticate.Enabled = true;
            txtUsername.Enabled = chkAuthenticate.Checked;
            txtPassword.Enabled = chkAuthenticate.Checked;
            chkConnectionLimit.Enabled = true;
            nudConnectionLimit.Enabled = dbGW.ConnectionLimit.HasValue;
        }

        private void lstMailGateways_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (lstMailGateways.SelectedItems.Count == 0)
            {
                ClearEditors();
                return;
            }
            ListViewItem selGW = lstMailGateways.SelectedItems[0];
            SelectedGW = selGW.Tag as long?;
            if (SelectedGW.HasValue)
            {
                Refreshing = true;
                PopulateSelectedGW();
                cmdDelete.Enabled = true;
                Refreshing = false;
            }
        }

        private void cmdNew_Click(object sender, EventArgs e)
        {
            Refreshing = true;
            SelectedGW = -1;
            txtSMTPServer.Text = "";
            nudPort.Value = 25;
            chkEnableSSL.Checked = false;
            txtSenderOverride.Text = "";
            chkAuthenticate.Checked = false;
            txtUsername.Text = "";
            txtPassword.Text = "";

            txtSMTPServer.Enabled = true;
            nudPort.Enabled = true;
            chkEnableSSL.Enabled = true;
            txtSenderOverride.Enabled = true;
            chkAuthenticate.Enabled = true;
            txtUsername.Enabled = chkAuthenticate.Checked;
            txtPassword.Enabled = chkAuthenticate.Checked;
            chkConnectionLimit.Checked = false;
            chkConnectionLimit.Enabled = true;
            nudConnectionLimit.Value = 3;
            nudConnectionLimit.Enabled = false;
            cmdDelete.Enabled = true;
            Refreshing = false;
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (SelectedGW.HasValue && SelectedGW > 0)
            {
                if (MessageBox.Show("Are you sure you want to delete the selected gateway?\r\n" +
                                    "Any users assigned to this gateway will have their gateway cleared.",
                                    "Delete Gateway?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SQLiteDB.MailGateway_ClearUserDeviceByID(SelectedGW.Value);
                    SQLiteDB.MailGateway_DeleteByID(SelectedGW.Value);
                    SelectedGW = null;
                    ClearEditors();
                    RefreshUI();
                }
            }
            else
            {
                SelectedGW = null;
                ClearEditors();
            }
        }

        private void cmdSaveChanges_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSMTPServer.Text))
            {
                MessageBox.Show("Enter a mail server URL or IP address.");
                return;
            }
            if (SelectedGW != -1)
            {
                tblMailGateway updategw = SQLiteDB.MailGateway_GetByID(SelectedGW.Value);
                if (updategw == null)
                {
                    if (MessageBox.Show("The mail gateway you are editing has been deleted.\r\n" + 
                        "Would you like to add this as a new mail gateway instead?",
                        "Deleted Mail Gateway", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        SelectedGW = -1;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    updategw.SMTPServer = txtSMTPServer.Text;
                    updategw.Port = (int)nudPort.Value;
                    updategw.EnableSSL = chkEnableSSL.Checked;
                    updategw.SenderOverride = txtSenderOverride.Text;
                    updategw.Authenticate = chkAuthenticate.Checked;
                    updategw.Username = updategw.Authenticate ? txtUsername.Text : "";
                    updategw.Password = updategw.Authenticate ? txtPassword.Text : "";
                    updategw.ConnectionLimit = chkConnectionLimit.Checked ? (int?)nudConnectionLimit.Value : (int?)null;
                    SQLiteDB.MailGateway_AddUpdate(updategw);
                    RefreshUI();
                }
            }
            if (SelectedGW == -1)
            {
                tblMailGateway newGW = new tblMailGateway(txtSMTPServer.Text, (int)nudPort.Value, chkEnableSSL.Checked, chkAuthenticate.Checked,
                    chkAuthenticate.Checked ? txtUsername.Text : "", chkAuthenticate.Checked ? txtPassword.Text : "", txtSenderOverride.Text,
                    chkConnectionLimit.Checked ? (int?)nudConnectionLimit.Value : (int?)null);
                SQLiteDB.MailGateway_AddUpdate(newGW);
                SelectedGW = newGW.MailGatewayID;
                RefreshUI();
            }
        }

        private void txtSMTPServer_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void nudPort_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void chkEnableSSL_CheckedChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void txtSenderOverride_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void chkAuthenticate_CheckedChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            txtUsername.Enabled = chkAuthenticate.Checked;
            txtPassword.Enabled = chkAuthenticate.Checked;
            cmdSaveChanges.Enabled = true;
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void chkConnectionLimit_CheckedChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            nudConnectionLimit.Enabled = chkConnectionLimit.Checked;
            cmdSaveChanges.Enabled = true;
        }

        private void nudConnectionLimit_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }
    }
}
