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
            SendQueue QueueItem = e.Argument as SendQueue;
            if (QueueItem == null)
            {
                e.Result = new StatusMessage("Failed to process SendQueue Item. No argument provided.", MessagePriority.Error);
            }


            

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
