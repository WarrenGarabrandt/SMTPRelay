using SMTPRelay.Database;
using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMTPRelay.Configuration.Controls
{
    public partial class ucEndpointEditor : UserControl
    {
        public ucEndpointEditor()
        {
            InitializeComponent();
        }

        private long? SelectedIP = null;
        private bool Refreshing = false;

        public void RefreshUI()
        {
            Refreshing = true;
            lstEndpoints.Items.Clear();
            List<ListViewItem> IPEndpoints = new List<ListViewItem>();
            var dbIEs = SQLiteDB.IPEndpoint_GetAll();
            foreach (var dbie in dbIEs)
            {
                ListViewItem gw = new ListViewItem(dbie.IPEndpointID.Value.ToString());
                gw.SubItems.Add(dbie.Address);
                gw.SubItems.Add(dbie.Port.ToString());
                gw.SubItems.Add(dbie.ProtocolString);
                gw.SubItems.Add(dbie.TLSModeString);
                gw.SubItems.Add(dbie.Hostname);
                gw.SubItems.Add(dbie.CertFriendlyName);
                gw.SubItems.Add(dbie.Maildrop ? "Maildrop" : "");
                gw.Tag = dbie.IPEndpointID;
                IPEndpoints.Add(gw);
            }
            lstEndpoints.Items.AddRange(IPEndpoints.ToArray());
            if (SelectedIP == null)
            {
                cmbIPAddress.Enabled = false;
                nudPort.Enabled = false;
                cmbProtocol.Enabled = false;
                cmbTLSMode.Enabled = false;
                txtHostname.Enabled = false;
                cmbCertFriendlyName.Enabled = false;
                cmdSaveChanges.Enabled = false;
                cmdDelete.Enabled = false;
                chkMaildrop.Enabled = false;
            }
            else
            {
                for (int i = 0; i < lstEndpoints.Items.Count; i++)
                {
                    if (((ListViewItem)lstEndpoints.Items[i]).Tag as long? == SelectedIP)
                    {
                        ((ListViewItem)lstEndpoints.Items[i]).Focused = true;
                        ((ListViewItem)lstEndpoints.Items[i]).Selected = true;
                        ((ListViewItem)lstEndpoints.Items[i]).EnsureVisible();
                    }
                }
            }
            Refreshing = false;
        }

        private void ClearEditors()
        {
            cmbIPAddress.Enabled = false;
            nudPort.Value = 25;
            nudPort.Enabled = false;
            cmbProtocol.Enabled = false;
            cmbTLSMode.Enabled = false;
            txtHostname.Text = "";
            txtHostname.Enabled = false;
            chkMaildrop.Checked = false;
            chkMaildrop.Enabled = false;
            cmbCertFriendlyName.Enabled = false;
            cmdSaveChanges.Enabled = false;
            cmdDelete.Enabled = false;
            cmdSaveChanges.Enabled = false;
            cmdDelete.Enabled = false;
        }

        private void PopulateSelectedGW()
        {
            tblIPEndpoint dbEP = SQLiteDB.IPEndpoint_GetByID(SelectedIP.Value);
            cmbIPAddress.Text = dbEP.Address;
            nudPort.Value = dbEP.Port;
            cmbProtocol.SelectedIndex = (int)dbEP.Protocol;
            cmbTLSMode.SelectedIndex = (int)dbEP.TLSMode;
            txtHostname.Text = dbEP.Hostname;
            cmbCertFriendlyName.Text = dbEP.CertFriendlyName;
            chkMaildrop.Checked = dbEP.Maildrop;

            cmbIPAddress.Enabled = true;
            nudPort.Enabled = true;
            cmbProtocol.Enabled = true;
            cmbTLSMode.Enabled = true;
            txtHostname.Enabled = true;
            cmbCertFriendlyName.Enabled = true;
            chkMaildrop.Enabled = true;
        }

        private void lstEndpoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (lstEndpoints.SelectedItems.Count == 0)
            {
                ClearEditors();
                return;
            }
            ListViewItem selEP = lstEndpoints.SelectedItems[0];
            SelectedIP = selEP.Tag as long?;
            if (SelectedIP.HasValue)
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
            SelectedIP = -1;
            cmbIPAddress.Text = "0.0.0.0";
            nudPort.Value = 25;
            cmbProtocol.SelectedIndex = 2;
            cmbTLSMode.SelectedIndex = 0;
            txtHostname.Text = SQLiteDB.GetFQDN();
            cmbCertFriendlyName.Text = "";
            chkMaildrop.Checked = false;

            cmbIPAddress.Enabled = true;
            nudPort.Enabled = true;
            cmbProtocol.Enabled = true;
            cmbTLSMode.Enabled = true;
            cmbCertFriendlyName.Enabled = true;
            cmdDelete.Enabled = true;
            chkMaildrop.Enabled = true;
            Refreshing = false;
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            if (SelectedIP.HasValue && SelectedIP > 0)
            {
                if (MessageBox.Show("Are you sure you want to delete the Endpoint?\r\n" +
                                    "If the service is already running, you will have to manually restart it to take effect.",
                                    "Delete Endpoint?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    SQLiteDB.IPEndpoint_DeleteByID(SelectedIP.Value);
                    SelectedIP = null;
                    ClearEditors();
                    RefreshUI();
                }
            }
            else
            {
                SelectedIP = null;
                ClearEditors();
            }
        }

        private void cmdSaveChanges_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbIPAddress.Text))
            {
                MessageBox.Show("Enter or choose an IP address for the Endpoint.");
                return;
            }
            if (GetTLSMode() != tblIPEndpoint.IPEndpointTLSModes.Disabled && string.IsNullOrEmpty(cmbCertFriendlyName.Text))
            {
                MessageBox.Show("TLS is set to Enable or Enforced but no certificate is specified. Connections will fail.");
                return;
            }
            if (string.IsNullOrEmpty(txtHostname.Text))
            {
                MessageBox.Show("Enter a hostname to report to clients.");
                return;
            }
            if (SelectedIP != -1)
            {
                tblIPEndpoint updateep = SQLiteDB.IPEndpoint_GetByID(SelectedIP.Value);
                if (updateep == null)
                {
                    if (MessageBox.Show("The Endpoint you are editing has been deleted.\r\n" +
                        "Would you like to add this as a new Endpoint instead?",
                        "Deleted Endpoint", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        SelectedIP = -1;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    updateep.Address = cmbIPAddress.Text;
                    updateep.Port = (int)nudPort.Value;
                    updateep.Protocol = GetProtocol();
                    updateep.TLSMode = GetTLSMode();
                    updateep.Hostname = txtHostname.Text.Trim();
                    updateep.CertFriendlyName = cmbCertFriendlyName.Text;
                    updateep.Maildrop = chkMaildrop.Checked;
                    SQLiteDB.IPEndpoint_AddUpdate(updateep);
                    RefreshUI();
                }
            }
            if (SelectedIP == -1)
            {
                tblIPEndpoint newEP = new tblIPEndpoint(cmbIPAddress.Text, (int)nudPort.Value, GetProtocol(), GetTLSMode(), txtHostname.Text.Trim(), cmbCertFriendlyName.Text, chkMaildrop.Checked);
                SQLiteDB.IPEndpoint_AddUpdate(newEP);
                SelectedIP = newEP.IPEndpointID;
                RefreshUI();
            }
        }

        private tblIPEndpoint.IPEndpointProtocols GetProtocol()
        {
            if (cmbProtocol.Text == "SMTP")
            {
                return tblIPEndpoint.IPEndpointProtocols.SMTP;
            }
            else if (cmbProtocol.Text == "ESMTP")
            {
                return tblIPEndpoint.IPEndpointProtocols.ESMTP;
            }
            else
            {
                return tblIPEndpoint.IPEndpointProtocols.None;
            }
        }

        private tblIPEndpoint.IPEndpointTLSModes GetTLSMode()
        {
            if (cmbTLSMode.Text == "Disabled")
            {
                return tblIPEndpoint.IPEndpointTLSModes.Disabled;
            }
            else if (cmbTLSMode.Text == "Enabled")
            {
                return tblIPEndpoint.IPEndpointTLSModes.Enabled;
            }
            else if (cmbTLSMode.Text == "Enforced")
            {
                return tblIPEndpoint.IPEndpointTLSModes.Enforced;
            }
            else
            {
                return tblIPEndpoint.IPEndpointTLSModes.Disabled;
            }
        }

        private void cmbIPAddress_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void cmbIPAddress_SelectedValueChanged(object sender, EventArgs e)
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

        private void cmbProtocol_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void cmbTLSMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void cmbCertFriendlyName_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void cmbCertFriendlyName_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void cmbIPAddress_DropDown(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            string oldValue = cmbIPAddress.Text;
            int selindex = -1;
            int idx = -1;
            cmbIPAddress.Items.Clear();
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                //Console.WriteLine("Name: " + netInterface.Name);
                //Console.WriteLine("Description: " + netInterface.Description);
                //Console.WriteLine("Addresses: ");
                IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    idx++;
                    cmbIPAddress.Items.Add(addr.Address.ToString());
                    if (oldValue == addr.Address.ToString())
                    {
                        selindex = idx;
                    }
                    //Console.WriteLine(" " + addr.Address.ToString());
                }
                //Console.WriteLine("");
            }
            if (selindex != -1)
            {
                cmbIPAddress.SelectedIndex = selindex;
            }
        }

        private void cmbCertFriendlyName_DropDown(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            string oldValue = cmbCertFriendlyName.Text;
            int selindex = -1;
            int idx = -1;
            cmbCertFriendlyName.Items.Clear();

            var store = GetLocalCerts();
            foreach (var cert in store.Cast<X509Certificate2>())
            {
                idx++;
                cmbCertFriendlyName.Items.Add(cert.FriendlyName);
                if (oldValue == cert.FriendlyName)
                {
                    selindex = idx;
                }
            }
            if (selindex != -1)
            {
                cmbCertFriendlyName.SelectedIndex = selindex;
            }
        }

        private X509Certificate2Collection GetLocalCerts()
        {
            var localMachineStore = new X509Store(StoreLocation.LocalMachine);
            localMachineStore.Open(OpenFlags.ReadOnly);
            var certs = localMachineStore.Certificates;
            localMachineStore.Close();
            return certs;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void chkMaildrop_CheckedChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }
    }
}
