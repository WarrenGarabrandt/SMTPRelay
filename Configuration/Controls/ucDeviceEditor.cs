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
    public partial class ucDeviceEditor : UserControl
    {
        public ucDeviceEditor()
        {
            InitializeComponent();
        }

        private long? SelectedDevice = null;
        private bool Refreshing = false;

        public void RefreshUI()
        {
            Refreshing = true;
            lstDevices.Items.Clear();
            List<ListViewItem> DeviceCollection = new List<ListViewItem>();
            var dbdevices = SQLiteDB.Device_GetAll();
            foreach (var dbdev in dbdevices)
            {
                ListViewItem dev = new ListViewItem(dbdev.DeviceID.Value.ToString());
                dev.SubItems.Add(dbdev.DisplayName);
                dev.SubItems.Add(dbdev.Hostname);
                dev.SubItems.Add(dbdev.Address);
                dev.SubItems.Add(dbdev.Enabled ? "Enabled" : "");
                dev.Tag = dbdev.DeviceID;
                DeviceCollection.Add(dev);
            }
            lstDevices.Items.AddRange(DeviceCollection.ToArray());
            if (SelectedDevice == null)
            {
                txtDisplayName.Enabled = false;
                txtHostname.Enabled = false;
                txtIPAddress.Enabled = false;
                chkEnabled.Enabled = false;
                cmdSaveChanges.Enabled = false;
                cmdDeleteDevice.Enabled = false;
                cmbGateways.SelectedItem = null;
                cmbGateways.Enabled = false;
            }
            else
            {
                for (int i = 0; i < lstDevices.Items.Count; i++)
                {
                    if (((ListViewItem)lstDevices.Items[i]).Tag as long? == SelectedDevice)
                    {
                        ((ListViewItem)lstDevices.Items[i]).Focused = true;
                        ((ListViewItem)lstDevices.Items[i]).Selected = true;
                        ((ListViewItem)lstDevices.Items[i]).EnsureVisible();
                    }
                }
            }
            Refreshing = false;
        }

        private void ClearEditors()
        {
            txtDisplayName.Text = "";
            txtDisplayName.Enabled = false;
            txtHostname.Text = "";
            txtHostname.Enabled = false;
            txtIPAddress.Text = "";
            txtIPAddress.Enabled = false;
            chkEnabled.Checked = false;
            chkEnabled.Enabled = false;
            cmdSaveChanges.Enabled = false;
            cmbGateways.SelectedItem = null;
            cmbGateways.Enabled = false;
        }

        private void PopulateSelectedUser()
        {
            tblDevice dbDev = SQLiteDB.Device_GetByID(SelectedDevice ?? -1);
            txtDisplayName.Text = dbDev.DisplayName;
            txtHostname.Text = dbDev.Hostname;
            txtIPAddress.Text = dbDev.Address;
            List<tblMailGateway> MailGateways = PopulateMailGateways();
            if (dbDev.MailGateway.HasValue)
            {
                foreach (var gw in MailGateways)
                {
                    if (gw.MailGatewayID == dbDev.MailGateway)
                    {
                        cmbGateways.SelectedItem = gw;
                        break;
                    }
                }
            }
            chkEnabled.Checked = dbDev.Enabled;
            txtDisplayName.Enabled = true;
            txtHostname.Enabled = true;
            txtIPAddress.Enabled = true;
            chkEnabled.Enabled = true;
        }

        private void lstDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (lstDevices.SelectedItems.Count == 0)
            {
                ClearEditors();
                return;
            }
            ListViewItem selDev = lstDevices.SelectedItems[0];
            SelectedDevice = selDev.Tag as long?;
            if (SelectedDevice.HasValue)
            {
                Refreshing = true;
                PopulateSelectedUser();
                cmdDeleteDevice.Enabled = true;
                Refreshing = false;
            }
        }

        private List<tblMailGateway> PopulateMailGateways()
        {
            cmbGateways.Enabled = true;
            List<tblMailGateway> MailGateways = SQLiteDB.MailGateway_GetAll();
            cmbGateways.Items.Clear();
            tblMailGateway unassign = new tblMailGateway(null, -1, false, false, null, null, null);
            cmbGateways.Items.Add(unassign);
            cmbGateways.Items.AddRange(MailGateways.ToArray());
            cmbGateways.SelectedItem = unassign;
            return MailGateways;
        }

        private void cmdNewDevice_Click(object sender, EventArgs e)
        {
            Refreshing = true;
            SelectedDevice = -1;
            txtDisplayName.Text = "";
            txtHostname.Text = "";
            txtIPAddress.Text = "";
            chkEnabled.Checked = true;
            PopulateMailGateways();
            txtDisplayName.Enabled = true;
            txtHostname.Enabled = true;
            txtIPAddress.Enabled = true;
            chkEnabled.Enabled = true;
            cmdDeleteDevice.Enabled = true;
            Refreshing = false;
        }

        private void cmdDeleteDevice_Click(object sender, EventArgs e)
        {
            if (SelectedDevice.HasValue && SelectedDevice > 0)
            {
                if (MessageBox.Show("Are you sure you want to delete the selected device?", "Delete Device", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // delete the user
                    SQLiteDB.Device_DeleteByID(SelectedDevice.Value);
                    SelectedDevice = null;
                    ClearEditors();
                    RefreshUI();
                }
            }
            else
            {
                SelectedDevice = null;
                ClearEditors();
            }
        }

        private void cmdSaveChanges_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtIPAddress.Text))
            {
                MessageBox.Show("No host IP Address specified.");
                return;
            }
            if (SelectedDevice != -1)
            {
                // update user
                tblDevice updateDev = SQLiteDB.Device_GetByID(SelectedDevice ?? -1);
                if (updateDev == null)
                {
                    if (MessageBox.Show("The device you are editing has been deleted.\r\n" +
                        "Would you like to add this as a new device instead?",
                        "Deleted Device", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        SelectedDevice = -1;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    updateDev.DisplayName = txtDisplayName.Text;
                    updateDev.Hostname = txtHostname.Text;
                    updateDev.Address = txtIPAddress.Text;
                    updateDev.Enabled = chkEnabled.Checked;
                    updateDev.MailGateway = ((tblMailGateway)cmbGateways.SelectedItem).MailGatewayID;
                    SQLiteDB.Device_AddUpdate(updateDev);
                    RefreshUI();
                }
            }
            if (SelectedDevice == -1)
            {
                // new user
                tblDevice newDev = new tblDevice(txtDisplayName.Text, txtIPAddress.Text, txtHostname.Text, chkEnabled.Checked, ((tblMailGateway)cmbGateways.SelectedItem).MailGatewayID);
                SQLiteDB.Device_AddUpdate(newDev);
                SelectedDevice = newDev.DeviceID;
                RefreshUI();
            }
        }

        private void txtDisplayName_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void txtHostname_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void txtIPAddress_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void cmbGateways_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }
    }
}
