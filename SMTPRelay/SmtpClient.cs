using SMTPRelay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay
{
    /// <summary>
    /// Sends queued item to a remote SMTP server, logs the result.
    /// </summary>
    public class SmtpClient
    {
        private BackgroundWorker worker;
        public SmtpClient(SendQueue item)
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync(item);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            SendQueue item = e.Argument as SendQueue;
            if (item == null)
            {
                e.Result = new StatusMessage("Failed to process SendQueue Item. No argument provided.", MessagePriority.Error);
            }
            Envelope env = SQLiteDB.GetEnvelopeByID(item.EnvelopeID);
            if (env == null)
            {
                // no envelope associated with this SendQueue
                // make SendLog entry
                // delete SendQueue entry
                e.Result = new StatusMessage(string.Format("Failed to send to <{0}>. No envelope found. ", item.Recipient), MessagePriority.Error);
                return;
            }
            if (env.ChunkCount == 0)
            {
                // Envelope is empty; no message body.
                // make SendLog entry
                // delete SendQueue entry
                e.Result = new StatusMessage(string.Format("Failed to send from <{0}> to <{1}>. Envelope was empty.", env.Sender, item.Recipient), MessagePriority.Error);
                return;
            }
            int ActualChunkCount = 0; // query this
            if (ActualChunkCount != env.ChunkCount)
            {
                // unexpected chunk count
                // make SendLog entry
                // delete SendQueue entry
                e.Result = new StatusMessage(string.Format("Failed to send from <{0}> to <{1}>. Unexpected envelope chunk count.", env.Sender, item.Recipient), MessagePriority.Error);
                return;
            }

            // establish a connection to the smart host

            // log in

            // start a new mail item

            // specify the recipient

            // if rejected, log and queue for retry

            // Start writing out chunks, querying SQLite for each as we go.

            // If error at any point, abort the connection and log the error. Mark the SendQueue item for retry

            // If successfully sent, get the receipt and log success. Delete the SendQueue item.

        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusMessage msg = e.UserState as StatusMessage;
            if (msg != null)
            {
                Messages.Enqueue(msg);
            }
        }


        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusMessage msg = e.Result as StatusMessage;
            if (msg != null)
            {
                Messages.Enqueue(msg);
            }
            Done = true;
        }

        public Queue<StatusMessage> Messages = new Queue<StatusMessage>();

        public bool Done { get; private set; }

        public void Dispose()
        {
            try
            {
                if (worker.IsBusy)
                {
                    worker.CancelAsync();
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    while (worker.IsBusy && sw.ElapsedMilliseconds < 5000)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }
            }
            catch { }
        }
    }
}
