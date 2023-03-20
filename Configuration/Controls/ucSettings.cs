using SMTPRelay.Configuration.Helpers;
using SMTPRelay.Database;
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
    public partial class ucSettings : UserControl
    {
        public ucSettings()
        {
            InitializeComponent();
        }

        private bool Refreshing = false;
        private bool Editing = false;
        private int EditingCount = 0;

        private enum ViewPages
        {
            None,
            SMTPReceiver,
            SMTPSender,
            Message,
            Purge
        }

        private ViewPages ActivePage = ViewPages.None;

        public void RefreshUI()
        {
            Refreshing = true;
            Editing = false;
            EditingCount = 0;
            if (ActivePage == ViewPages.None)
            {
                ActivePage = ViewPages.SMTPReceiver;
            }
            HideAllPages();
            switch (ActivePage)
            {
                case ViewPages.SMTPReceiver:
                    PopulateSMTPReceiverSettings();
                    break;
                case ViewPages.SMTPSender:
                    PopulateSMTPSenderSettings();
                    break;
                case ViewPages.Message:
                    PopulateMessageSettings();
                    break;
                case ViewPages.Purge:
                    PopulatePurgeSettings();
                    break;
                default:
                    MessageBox.Show(string.Format("Unknown settings page: {0}", ActivePage.ToString()));
                    Refreshing = false;
                    return;
            }
            Refreshing = false;
        }

        private void HideAllPages()
        {
            grpReceiver.Visible = false;
            grpSender.Visible = false;
            grpMessage.Visible = false;
            grpPurge.Visible = false;
            chkReceiverPage.Checked = false;
            chkSenderPage.Checked = false;
            chkMessagePage.Checked = false;
            chkPurgePage.Checked = false;
        }

        private void PopulateSMTPReceiverSettings()
        {
            grpReceiver.Visible = true;
            grpReceiver.BringToFront();
            chkReceiverPage.Checked = true;
            cmdSaveRecBadCmdLimit.Visible = false;
            cmdSaveRecCmdTimeoutMS.Visible = false;
            cmdSaveRecConnTimeoutMS.Visible = false;
            cmdSaveRecLogPath.Visible = false;
            cmdSaveRecVerboseDebugging.Visible = false;
            SetNudValSafe(nudRecConnTimeoutMS, SettingsHelper.GetIntValue("SMTPServer", "ConnectionTimeoutMS"));
            SetNudValSafe(nudRecCmdTimeoutMS, SettingsHelper.GetIntValue("SMTPServer", "CommandTimeoutMS"));
            SetNudValSafe(nudRecBadCmdLimit, SettingsHelper.GetIntValue("SMTPServer", "BadCommandLimit"));
            int dbg = SettingsHelper.GetIntValue("SMTPServer", "VerboseDebuggingEnabled");
            int dbgmsg = SettingsHelper.GetIntValue("SMTPServer", "VerboseDebuggingIncludeBody");
            if (dbg == 0)
            {
                cmbRecVerboseDebugging.SelectedIndex = 0;
            }
            else if (dbgmsg == 0)
            {
                cmbRecVerboseDebugging.SelectedIndex = 1;
            }
            else
            {
                cmbRecVerboseDebugging.SelectedIndex = 2;
            }
            txtRecLogPath.Text = SettingsHelper.GetStrValue("SMTPServer", "VerboseDebuggingPath");
        }

        private void SetNudValSafe(NumericUpDown nud, int val)
        {
            if (nud.Minimum > val)
            {
                nud.Minimum = val;
            }
            if (nud.Maximum < val)
            {
                nud.Maximum = val;
            }
            nud.Value = val;
        }

        private void PopulateSMTPSenderSettings()
        {
            grpSender.Visible = true;
            grpSender.BringToFront();
            chkSenderPage.Checked = true;
        }

        private void PopulateMessageSettings()
        {
            grpMessage.Visible = true;
            grpMessage.BringToFront();
            chkMessagePage.Checked = true;
        }

        private void PopulatePurgeSettings()
        {
            grpPurge.Visible = true;
            grpPurge.BringToFront();
            chkPurgePage.Checked = true;
        }

        private void PageChange(object sender, EventArgs e)
        {
            if (Refreshing)
            { 
                return;
            }
            CheckBox s = sender as CheckBox;
            if (s == null || s.Checked == false)
            {
                return;
            }
            string tag = s.Tag as string;
            switch(tag)
            {
                case "R":
                    if (ActivePage != ViewPages.SMTPReceiver)
                    {
                        if (Editing && !SwitchPromptContinue())
                        {
                            s.Checked = false;
                            return;
                        }
                        ActivePage = ViewPages.SMTPReceiver;
                        RefreshUI();
                        return;
                    }
                    break;
                case "S":
                    if (ActivePage != ViewPages.SMTPSender)
                    {
                        if (Editing && !SwitchPromptContinue())
                        {
                            s.Checked = false;
                            return;
                        }
                        ActivePage = ViewPages.SMTPSender;
                        RefreshUI();
                        return;
                    }
                    break;
                case "M":
                    if (ActivePage != ViewPages.Message)
                    {
                        if (Editing && !SwitchPromptContinue())
                        {
                            s.Checked = false;
                            return;
                        }
                        ActivePage = ViewPages.Message;
                        RefreshUI();
                        return;
                    }
                    break;
                case "P":
                    if (ActivePage != ViewPages.Purge)
                    {
                        if (Editing && !SwitchPromptContinue())
                        {
                            s.Checked = false;
                            return;
                        }
                        ActivePage = ViewPages.Purge;
                        RefreshUI();
                        return;
                    }
                    break;
            }
        }

        private bool SwitchPromptContinue()
        {
            if (MessageBox.Show("Unsaved changes will be lost. Continue?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region SMTP Receiver Save
        private void nudRecConnTimeoutMS_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveRecConnTimeoutMS.Visible)
            {
                return;
            }
            cmdSaveRecConnTimeoutMS.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveRecConnTimeoutMS_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("SMTPServer", "ConnectionTimeoutMS", ((int)nudRecConnTimeoutMS.Value).ToString());
            EditingCount--;
            if (EditingCount <= 0)
            {
                EditingCount = 0;
                Editing = false;
            }
            cmdSaveRecConnTimeoutMS.Visible = false;
        }

        private void nudRecCmdTimeoutMS_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (nudRecCmdTimeoutMS.Visible)
            {
                return;
            }
            cmdSaveRecCmdTimeoutMS.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveRecCmdTimeoutMS_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("SMTPServer", "CommandTimeoutMS", ((int)nudRecCmdTimeoutMS.Value).ToString());
            EditingCount--;
            if (EditingCount <= 0)
            {
                EditingCount = 0;
                Editing = false;
            }
            cmdSaveRecCmdTimeoutMS.Visible = false;
        }

        private void nudRecBadCmdLimit_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (nudRecBadCmdLimit.Visible)
            {
                return;
            }
            cmdSaveRecBadCmdLimit.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveRecBadCmdLimit_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("SMTPServer", "BadCommandLimit", ((int)nudRecBadCmdLimit.Value).ToString());
            EditingCount--;
            if (EditingCount <= 0)
            {
                EditingCount = 0;
                Editing = false;
            }
            cmdSaveRecBadCmdLimit.Visible = false;
        }

        private void cmbRecVerboseDebugging_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmbRecVerboseDebugging.SelectedIndex < 0)
            {
                cmbRecVerboseDebugging.SelectedIndex = 0;
                return;
            }
            if (cmdSaveRecVerboseDebugging.Visible)
            {
                return;
            }
            cmdSaveRecVerboseDebugging.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveRecVerboseDebugging_Click(object sender, EventArgs e)
        {
            switch (cmbRecVerboseDebugging.SelectedIndex)
            {
                case 0:
                    SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingEnabled", "0");
                    SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingIncludeBody", "0");
                    break;
                case 1:
                    SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingEnabled", "1");
                    SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingIncludeBody", "0");
                    break;
                case 2:
                    SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingEnabled", "1");
                    SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingIncludeBody", "1");
                    break;
            }

            EditingCount--;
            if (EditingCount <= 0)
            {
                Editing = false;
                EditingCount = 0;
            }
            cmdSaveRecVerboseDebugging.Visible = false;
        }

        private void txtRecLogPath_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveRecLogPath.Visible)
            {
                return;
            }
            cmdSaveRecLogPath.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveRecLogPath_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingPath", txtRecLogPath.Text.Trim());

            EditingCount--;
            if (EditingCount <= 0)
            {
                Editing = false;
                EditingCount = 0;
            }
            cmdSaveRecLogPath.Visible = false;
        }

        #endregion
    }
}
