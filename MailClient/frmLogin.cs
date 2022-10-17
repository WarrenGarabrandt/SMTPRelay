using SMTPRelay.Database;
using SMTPRelay.Model;
using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MailClient
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        public tblUser LoggedInUser = null;

        private void cmdExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmdLogin_Click(object sender, EventArgs e)
        {
            tblUser user = SQLiteDB.User_GetByEmailPassword(txtEmail.Text, txtPassword.Text);
            if (user != null && user.Enabled && user.Maildrop)
            {
                LoggedInUser = user;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid email or password");
            }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            try
            {
                WorkerReport InitReport = SQLiteDB.InitDatabase();
                if (InitReport != null && !string.IsNullOrEmpty(InitReport.LogError))
                {
                    MessageBox.Show(InitReport.LogError);
                    Application.Exit();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Exit();
            }
        }
    }
}
