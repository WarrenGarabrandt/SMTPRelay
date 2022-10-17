using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MailClient
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            frmLogin loginForm = new frmLogin();
            Application.Run(loginForm);
            if (loginForm.LoggedInUser != null)
            {
                frmMain main = new frmMain();
                main.LoggedInUser = loginForm.LoggedInUser;
                Application.Run(main);
            }
        }
    }
}
