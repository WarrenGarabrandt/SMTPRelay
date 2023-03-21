using SMTPRelay.Configuration.Helpers;
using SMTPRelay.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

        #region Populate Controls

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

        private void PopulateSMTPSenderSettings()
        {
            grpSender.Visible = true;
            grpSender.BringToFront();
            chkSenderPage.Checked = true;
            cmdSaveSndHostname.Visible = false;
            cmdSaveSndIntervalMS.Visible = false;
            cmdSaveSndLogPath.Visible = false;
            cmdSaveSndVerboseDebugging.Visible = false;
            txtSndHostname.Text = SettingsHelper.GetStrValue("SMTPSender", "Hostname");
            SetNudValSafe(nudSndIntervalMS, SettingsHelper.GetIntValue("SMTPSender", "QueueRefreshMS"));
            int dbg = SettingsHelper.GetIntValue("SMTPSender", "VerboseDebuggingEnabled");
            int dbgmsg = SettingsHelper.GetIntValue("SMTPSender", "VerboseDebuggingIncludeBody");
            if (dbg == 0)
            {
                cmbSndVerboseDebugging.SelectedIndex = 0;
            }
            else if (dbgmsg == 0)
            {
                cmbSndVerboseDebugging.SelectedIndex = 1;
            }
            else
            {
                cmbSndVerboseDebugging.SelectedIndex = 2;
            }
            txtSndLogPath.Text = SettingsHelper.GetStrValue("SMTPSender", "VerboseDebuggingPath");
        }

        private void PopulateMessageSettings()
        {
            grpMessage.Visible = true;
            grpMessage.BringToFront();
            chkMessagePage.Checked = true;
            cmdSaveMsgMaxSize.Visible = false;
            cmdSaveMsgRecipCount.Visible = false;
            cmdSaveMsgChunkSize.Visible = false;
            cmdSaveMsgRetentionMins.Visible = false;
            cmdSaveMsgFailedRetentionMins.Visible = false;
            SetNudValSafe(nudMsgMaxSize, SettingsHelper.GetIntValue("Message", "MaxLength"));
            SetNudValSafe(nudMsgRecipCount, SettingsHelper.GetIntValue("Message", "MaxRecipients"));
            SetNudValSafe(nudMsgChunkSize, SettingsHelper.GetIntValue("Message", "ChunkSize"));
            SetNudValSafe(nudMsgRetentionMins, SettingsHelper.GetIntValue("Message", "DataRetainMins"));
            SetNudValSafe(nudMsgFailedRetentionMins, SettingsHelper.GetIntValue("Message", "PurgeFailedMins"));
        }

        private void PopulatePurgeSettings()
        {
            grpPurge.Visible = true;
            grpPurge.BringToFront();
            chkPurgePage.Checked = true;
        }

        #endregion

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
            if (cmdSaveRecCmdTimeoutMS.Visible)
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
            if (cmdSaveRecBadCmdLimit.Visible)
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

        #region SMTP Sender Save
        private void txtSndHostname_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveSndHostname.Visible)
            {
                return;
            }
            cmdSaveSndHostname.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveSndHostname_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("SMTPSender", "Hostname", txtSndHostname.Text.Trim());

            EditingCount--;
            if (EditingCount <= 0)
            {
                Editing = false;
                EditingCount = 0;
            }
            cmdSaveSndHostname.Visible = false;
        }

        private void nudSndIntervalMS_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveSndIntervalMS.Visible)
            {
                return;
            }
            cmdSaveSndIntervalMS.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveSndIntervalMS_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("SMTPSender", "QueueRefreshMS", ((int)nudSndIntervalMS.Value).ToString());
            EditingCount--;
            if (EditingCount <= 0)
            {
                EditingCount = 0;
                Editing = false;
            }
            cmdSaveSndIntervalMS.Visible = false;
        }

        private void cmbSndVerboseDebugging_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveSndVerboseDebugging.Visible)
            {
                return;
            }
            cmdSaveSndVerboseDebugging.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveSndVerboseDebugging_Click(object sender, EventArgs e)
        {
            switch (cmbSndVerboseDebugging.SelectedIndex)
            {
                case 0:
                    SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingEnabled", "0");
                    SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingIncludeBody", "0");
                    break;
                case 1:
                    SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingEnabled", "1");
                    SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingIncludeBody", "0");
                    break;
                case 2:
                    SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingEnabled", "1");
                    SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingIncludeBody", "1");
                    break;
            }

            EditingCount--;
            if (EditingCount <= 0)
            {
                Editing = false;
                EditingCount = 0;
            }
            cmdSaveSndVerboseDebugging.Visible = false;
        }

        private void txtSndLogPath_TextChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveSndLogPath.Visible)
            {
                return;
            }
            cmdSaveSndLogPath.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveSndLogPath_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingPath", txtSndLogPath.Text.Trim());

            EditingCount--;
            if (EditingCount <= 0)
            {
                Editing = false;
                EditingCount = 0;
            }
            cmdSaveSndLogPath.Visible = false;
        }
        #endregion

        #region Message Save
        private void nudMsgMaxSize_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveMsgMaxSize.Visible)
            {
                return;
            }
            cmdSaveMsgMaxSize.Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveMsgMaxSize_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("Message", "MaxLength", ((int)nudMsgMaxSize.Value).ToString());
            EditingCount--;
            if (EditingCount <= 0)
            {
                EditingCount = 0;
                Editing = false;
            }
            cmdSaveMsgMaxSize.Visible = false;
        }

        private void nudMsgRecipCount_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveMsgRecipCount.Visible)
            {
                return;
            }
            cmdSaveMsgRecipCount .Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveMsgRecipCount_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("Message", "MaxRecipients", ((int)nudMsgRecipCount.Value).ToString());
            EditingCount--;
            if (EditingCount <= 0)
            {
                EditingCount = 0;
                Editing = false;
            }
            cmdSaveMsgRecipCount.Visible = false;
        }

        private void nudMsgChunkSize_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveMsgChunkSize.Visible)
            {
                return;
            }
            cmdSaveMsgChunkSize .Visible = true;
            Editing = true;
            EditingCount++;

        }

        private void cmdSaveMsgChunkSize_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("Message", "ChunkSize", ((int)nudMsgChunkSize.Value).ToString());
            EditingCount--;
            if (EditingCount <= 0)
            {
                EditingCount = 0;
                Editing = false;
            }
            cmdSaveMsgChunkSize.Visible = false;
        }

        private void nudMsgRetentionMins_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveMsgRetentionMins.Visible)
            {
                return;
            }
            cmdSaveMsgRetentionMins .Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveMsgRetentionMins_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("Message", "DataRetainMins", ((int)nudMsgRetentionMins.Value).ToString());
            EditingCount--;
            if (EditingCount <= 0)
            {
                EditingCount = 0;
                Editing = false;
            }
            cmdSaveMsgRetentionMins.Visible = false;
        }

        private void nudMsgFailedRetentionMins_ValueChanged(object sender, EventArgs e)
        {
            if (Refreshing)
            {
                return;
            }
            if (cmdSaveMsgFailedRetentionMins.Visible)
            {
                return;
            }
            cmdSaveMsgFailedRetentionMins .Visible = true;
            Editing = true;
            EditingCount++;
        }

        private void cmdSaveMsgFailedRetentionMins_Click(object sender, EventArgs e)
        {
            SQLiteDB.System_AddUpdateValue("Message", "PurgeFailedMins", ((int)nudMsgFailedRetentionMins.Value).ToString());
            EditingCount--;
            if (EditingCount <= 0)
            {
                EditingCount = 0;
                Editing = false;
            }
            cmdSaveMsgFailedRetentionMins.Visible = false;
        }
        #endregion
    }
}
