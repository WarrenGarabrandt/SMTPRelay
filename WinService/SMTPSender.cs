using SMTPRelay.Model;
using SMTPRelay.Model.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.WinService
{
    public class SMTPSender
    {
        private BackgroundWorker Worker;
        public bool Running;
        public ConcurrentQueue<WorkerReport> WorkerReports;

        public SMTPSender(tblSendQueue sendQueueItem )
        {
            WorkerReports = new ConcurrentQueue<WorkerReport>();
            Running = true;
            Worker = new BackgroundWorker();
            Worker.WorkerSupportsCancellation = true;
            Worker.WorkerReportsProgress = true;
            Worker.DoWork += Worker_DoWork;
            Worker.ProgressChanged += Worker_ProgressChanged;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            Worker.RunWorkerAsync(sendQueueItem);
        }

        public void Cancel()
        {
            Worker.CancelAsync();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is WorkerReport)
            {
                WorkerReports.Enqueue(e.UserState as WorkerReport);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is WorkerReport)
            {
                WorkerReports.Enqueue(e.Result as WorkerReport);
            }
            Running = false;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            tblSendQueue sendQueueItem = e.Argument as tblSendQueue;
            if (sendQueueItem == null)
            {
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogError = "No SendQueue Item specified to SMTPSender."
                });
                return;
            }

            // queue item is already marked as dispatched before SMTPSender is instantiated to prevent duplicate pickups.
            // retrieve necessary message info (sender, recipient, gateway, message size)
            // Sender
            string MailFromAddress = "";
            // Recipient
            string RcptToAddress = "";

            // attempt to connect to the server

            // negotiate the connection and switch to TLS if the gateway specifies requirements to do so.

            // negotiate the email from/rcpt

            // transmit the message body

            // get a final ack

            // close the tcp connection

            // store the ack as a SendLog and delete the SendQueue

            throw new NotImplementedException();
        }

    }
}
