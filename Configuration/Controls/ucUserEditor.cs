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
    public partial class ucUserEditor : UserControl
    {
        public ucUserEditor()
        {
            InitializeComponent();
        }

        private long? SelectedUser = null;
        private bool UserPasswordChanged = false;

        private bool Refreshing = false;

        public void RefreshUI()
        {
            Refreshing = true;
            lstUsers.Items.Clear();
            List<ListViewItem> UserCollection = new List<ListViewItem>();
            var dbusers = SQLiteDB.User_GetAll();
            foreach (var dbuser in dbusers)
            {
                ListViewItem user = new ListViewItem(dbuser.UserID.Value.ToString());
                user.SubItems.Add(dbuser.DisplayName);
                user.SubItems.Add(dbuser.Email);
                user.SubItems.Add(dbuser.Enabled ? "Enabled" : "");
                user.SubItems.Add(dbuser.Admin ? "Admin" : "");
                user.Tag = dbuser.UserID;
                UserCollection.Add(user);
            }
            lstUsers.Items.AddRange(UserCollection.ToArray());
            if (SelectedUser == null)
            {
                txtDisplayName.Enabled = false;
                txtEmailAddress.Enabled = false;
                txtPassword1.Enabled = false;
                txtPassword2.Enabled = false;
                chkEnabled.Enabled = false;
                chkAdmin.Enabled = false;
                cmdSaveChanges.Enabled = false;
                cmdDeleteUser.Enabled = false;
                cmbGateways.SelectedItem = null;
                cmbGateways.Enabled = false;
            }
            else
            {
                for (int i = 0; i < lstUsers.Items.Count; i++)
                {
                    if (((ListViewItem)lstUsers.Items[i]).Tag as long? == SelectedUser)
                    {
                        ((ListViewItem)lstUsers.Items[i]).Focused = true;
                        ((ListViewItem)lstUsers.Items[i]).Selected = true;
                        ((ListViewItem)lstUsers.Items[i]).EnsureVisible();
                    }
                }
            }
            Refreshing = false;
        }

        private void ClearEditors()
        {
            txtDisplayName.Text = "";
            txtDisplayName.Enabled = false;
            txtEmailAddress.Text = "";
            txtEmailAddress.Enabled = false;
            txtPassword1.Text = "";
            txtPassword1.Enabled = false;
            txtPassword2.Text = "";
            txtPassword2.Enabled = false;
            chkEnabled.Checked = false;
            chkEnabled.Enabled = false;
            chkAdmin.Checked = false;
            chkAdmin.Enabled = false;
            cmdSaveChanges.Enabled = false;
            cmbGateways.SelectedItem = null;
            cmbGateways.Enabled = false;
        }

        private void PopulateSelectedUser()
        { 
            tblUser dbUser = SQLiteDB.User_GetByID(SelectedUser);
            txtDisplayName.Text = dbUser.DisplayName;
            txtEmailAddress.Text = dbUser.Email;
            txtPassword1.Text = dbUser.PassHash;
            txtPassword2.Text = dbUser.PassHash;
            List<tblMailGateway> MailGateways = PopulateMailGateways();
            if (dbUser.MailGateway.HasValue)
            {
                foreach (var gw in MailGateways)
                {
                    if (gw.MailGatewayID == dbUser.MailGateway)
                    {
                        cmbGateways.SelectedItem = gw;
                        break;
                    }
                }
            }
            chkEnabled.Checked = dbUser.Enabled;
            chkAdmin.Checked = dbUser.Admin;
            txtDisplayName.Enabled = true;
            txtEmailAddress.Enabled = true;
            txtPassword1.Enabled = true;
            txtPassword2.Enabled = true;
            chkEnabled.Enabled = true;
            chkAdmin.Enabled = true;
            UserPasswordChanged = false;
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (lstUsers.SelectedItems.Count == 0)
            {
                ClearEditors();
                return;
            }
            ListViewItem selUser = lstUsers.SelectedItems[0];
            SelectedUser = selUser.Tag as long?;
            if (SelectedUser.HasValue)
            {
                Refreshing = true;
                PopulateSelectedUser();
                cmdDeleteUser.Enabled = true;
                Refreshing = false;
            }
        }

        private List<tblMailGateway> PopulateMailGateways()
        {
            cmbGateways.Enabled = true;
            List<tblMailGateway>  MailGateways = SQLiteDB.MailGateway_GetAll();
            cmbGateways.Items.Clear();
            tblMailGateway unassign = new tblMailGateway(null, -1, false, false, null, null, null);
            cmbGateways.Items.Add(unassign);
            cmbGateways.Items.AddRange(MailGateways.ToArray());
            cmbGateways.SelectedItem = unassign;
            return MailGateways;
        }

        private void cmdNewUser_Click(object sender, EventArgs e)
        {
            Refreshing = true;
            SelectedUser = -1;
            UserPasswordChanged = false;
            txtDisplayName.Text = "";
            txtEmailAddress.Text = "";
            txtPassword1.Text = "";
            txtPassword2.Text = "";
            chkEnabled.Checked = true;
            chkAdmin.Checked = false;
            PopulateMailGateways();
            txtDisplayName.Enabled = true;
            txtEmailAddress.Enabled = true;
            txtPassword1.Enabled = true;
            txtPassword2.Enabled = true;
            chkEnabled.Enabled = true;
            chkAdmin.Enabled = true;
            cmdDeleteUser.Enabled = true;
            Refreshing = false;
        }

        private void cmdDeleteUser_Click(object sender, EventArgs e)
        {
            if (SelectedUser.HasValue && SelectedUser > 0)
            {
                if (MessageBox.Show("Are you sure you want to delete the selected user?", "Delete User", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // delete the user
                    SQLiteDB.User_DeleteByID(SelectedUser.Value);
                    SelectedUser = null;
                    ClearEditors();
                    RefreshUI();
                }
            }
            else
            {
                SelectedUser = null;
                ClearEditors();
            }
        }

        private void cmdSaveChanges_Click(object sender, EventArgs e)
        {
            if (txtPassword1.Text != txtPassword2.Text)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }
            if (SelectedUser != -1)
            {
                // update user
                tblUser updateUser = SQLiteDB.User_GetByID(SelectedUser);
                if (updateUser == null)
                {
                    if (MessageBox.Show("The user you are editing has been deleted.\r\n" + 
                        "Would you like to add this as a new user instead?", 
                        "Deleted User", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (!UserPasswordChanged)
                        {
                            MessageBox.Show("The password needs to be changed before the user can log in.");
                        }
                        SelectedUser = -1;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    updateUser.DisplayName = txtDisplayName.Text;
                    updateUser.Email = txtEmailAddress.Text;
                    updateUser.Enabled = chkEnabled.Checked;
                    updateUser.Admin = chkAdmin.Checked;
                    updateUser.MailGateway = ((tblMailGateway)cmbGateways.SelectedItem).MailGatewayID;
                    if (UserPasswordChanged)
                    {
                        SQLiteDB.GeneratePasswordHash(updateUser, txtPassword1.Text);
                        UserPasswordChanged = false;
                    }
                    SQLiteDB.User_AddUpdate(updateUser);
                    RefreshUI();
                }
            }
            if (SelectedUser == -1)
            {
                // new user
                tblUser newUser = new tblUser(txtDisplayName.Text, txtEmailAddress.Text, null, null, chkEnabled.Checked, chkAdmin.Checked, ((tblMailGateway)cmbGateways.SelectedItem).MailGatewayID);
                SQLiteDB.GeneratePasswordHash(newUser, txtPassword1.Text);
                UserPasswordChanged = false;
                SQLiteDB.User_AddUpdate(newUser);
                SelectedUser = newUser.UserID;
                RefreshUI();
            }
        }

        private void txtPassword1_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            UserPasswordChanged = true;
            cmdSaveChanges.Enabled = true;
        }

        private void txtPassword2_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            UserPasswordChanged = true;
            cmdSaveChanges.Enabled = true;
        }

        private void txtDisplayName_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void txtEmailAddress_TextChanged(object sender, EventArgs e)
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

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }

        private void chkAdmin_CheckedChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            cmdSaveChanges.Enabled = true;
        }
    }
}
