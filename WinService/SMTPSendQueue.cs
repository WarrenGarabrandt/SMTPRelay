using SMTPRelay.Database;
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
    public class SMTPSendQueue
    {
        private const int MAX_ACTIVE_SENDERS = 10;

        private BackgroundWorker Worker;
        public bool Running = false;
        public ConcurrentQueue<WorkerReport> WorkerReports;
        public SMTPSendQueue()
        {
            WorkerReports = new ConcurrentQueue<WorkerReport>();
            Running = true;
            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            Worker.ProgressChanged += Worker_ProgressChanged;
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerAsync();
        }

        public void Cancel()
        {
            Worker.CancelAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            
            List<SMTPSender> Senders = new List<SMTPSender>();
            System.Diagnostics.Stopwatch cleanupSW = new System.Diagnostics.Stopwatch();
            cleanupSW.Start();
            System.Diagnostics.Stopwatch dbquerySW = new System.Diagnostics.Stopwatch();
            dbquerySW.Start();
            List<tblSendQueue> pendingQueue = new List<tblSendQueue>();

            try
            {
                int QueryUpdateInterval = Int32.Parse(SQLiteDB.System_GetValue("SMTPSenderQueue", "RefreshMS"));
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogMessage = "Started Outbound Queue Monitor."
                });
                while (!Worker.CancellationPending)
                {
                    if (dbquerySW.ElapsedMilliseconds >= QueryUpdateInterval)
                    {
                        pendingQueue.Clear();
                        pendingQueue = SQLiteDB.SendQueue_GetReady();
                        dbquerySW.Restart();
                    }
                    bool busy = false;
                    if (pendingQueue.Count() > 0 && Senders.Count < MAX_ACTIVE_SENDERS)
                    {
                        tblSendQueue sq = pendingQueue.First();
                        pendingQueue.RemoveAt(0);
                        sq.State = SendQueueState.InProgress;
                        SQLiteDB.SendQueue_AddUpdate(sq);
                        SMTPSender snd = new SMTPSender(sq);
                        Senders.Add(snd);
                        if (pendingQueue.Count() > 0)
                        {
                            busy = true;
                        }
                    }
                    else if (pendingQueue.Count() > 0 && Senders.Count >= MAX_ACTIVE_SENDERS && cleanupSW.ElapsedMilliseconds > 100)
                    {
                        CleanupSenders(Senders);
                        cleanupSW.Restart();
                    }

                    if (Senders.Count > 4 && cleanupSW.ElapsedMilliseconds > 500)
                    {
                        CleanupSenders(Senders);
                        cleanupSW.Restart();
                    }
                    else if (cleanupSW.ElapsedMilliseconds >= 1000)
                    {
                        CleanupSenders(Senders);
                        cleanupSW.Restart();
                    }
                    if (!busy)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }

                // wind down the senders
                foreach (var snd in Senders)
                {
                    snd.Cancel();
                }
                while (!CleanupSenders(Senders))
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogError = ex.Message
                });
            }
            finally
            {
                e.Result = new WorkerReport()
                {
                    LogMessage = "SMTP Send Queue shut down."
                };
            }
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

        /// <summary>
        /// Remove all dead senders from the collection and returns True if the list if empty.
        /// </summary>
        /// <param name="senders"></param>
        /// <returns></returns>
        private bool CleanupSenders(List<SMTPSender> senders)
        {
            List<SMTPSender> remove = new List<SMTPSender>();
            foreach (var snd in senders)
            {
                TakeReports(snd);
                if (!snd.Running)
                {
                    remove.Add(snd);
                }
            }
            foreach (var rcv in remove)
            {
                senders.Remove(rcv);
                TakeReports(rcv);
            }
            remove.Clear();
            return senders.Count == 0;
        }

        private void TakeReports(SMTPSender sender)
        {
            WorkerReport rpt;
            while (sender.WorkerReports.TryDequeue(out rpt))
            {
                WorkerReports.Enqueue(rpt);
            }
        }

    }
}
