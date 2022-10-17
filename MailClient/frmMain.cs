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

namespace MailClient
{
    public partial class frmMain : Form
    {
        public tblUser LoggedInUser = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void lstDevices_SelectedIndexChanged(object sender, EventArgs e)
        {

            
        }
    }
}
