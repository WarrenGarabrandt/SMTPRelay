using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using SMTPRelay.Model;
using SMTPRelay.Database;

namespace Configuration
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool RunningAsConsole = false;
        System.Diagnostics.Process ConsoleProcess = null;


        private void cmdStartConsole_Click(object sender, EventArgs e)
        {
            if (RunningAsConsole)
            {
                MessageBox.Show("The console has already been started.\r\nClose it manually if you need to restart it.");
                return;
            }
            try
            {
                ServiceController sc = new ServiceController("SMTPRelay");
                if (sc.Status != ServiceControllerStatus.Stopped)
                {
                    MessageBox.Show("The SMTPRelay service is running on this computer already.\r\n" +
                                    "Use 127.0.0.1 to connect, or stop the service manually first.",
                                    "Service Running", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message == "The specified service does not exist as an installed service")
                {
                    // this is expected if the service isn't installed.
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }

            if (MessageBox.Show("This will start a console instance of the SMTP Relay.\r\n" +
                                "DO NOT DO THIS IF THE SERVICE IS RUNNING!\r\n" +
                                "Do you want to continue?", 
                                "Start Console?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                RunningAsConsole = true;
                ConsoleProcess = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startinfo = new System.Diagnostics.ProcessStartInfo();
                startinfo.FileName = "WinService.exe";
                startinfo.UseShellExecute = true;
                ConsoleProcess.StartInfo = startinfo;
                ConsoleProcess.EnableRaisingEvents = true;
                ConsoleProcess.Exited += ConsoleProcess_Exited;
                ConsoleProcess.Start();
            }
        }

        private void ConsoleProcess_Exited(object sender, EventArgs e)
        {
            MessageBox.Show("The console process has stopped.");
            ConsoleProcess.Dispose();
            ConsoleProcess = null;
            RunningAsConsole = false;

        }

        private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcMain.SelectedTab == tpUsers)
            {
                ucSendQueueEditor.SuspendUI();
                ucUserEditor.RefreshUI();
            }
            else if (tcMain.SelectedTab == tpMailGateway)
            {
                ucSendQueueEditor.SuspendUI();
                ucMailGatewayEditor.RefreshUI();
            }
            else if (tcMain.SelectedTab == tpSendQueue)
            {
                ucSendQueueEditor.RefreshUI();
            }
            else if (tcMain.SelectedTab == tpEndpoints)
            {
                ucSendQueueEditor.SuspendUI();
                ucEndpointEditor.RefreshUI();
            }
            else if (tcMain.SelectedTab == tpDevices)
            {
                ucSendQueueEditor.SuspendUI();
                ucDeviceEditor1.RefreshUI();
            }
            else if (tcMain.SelectedTab == tpConfig)
            {
                ucSendQueueEditor.SuspendUI();
                ucSettings1.RefreshUI();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
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

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (tcMain.SelectedTab == tpSendQueue)
            {
                ucSendQueueEditor.RefreshUI();
            }
            else
            {
                ucSendQueueEditor.SuspendUI();

            }
        }
    }
}
