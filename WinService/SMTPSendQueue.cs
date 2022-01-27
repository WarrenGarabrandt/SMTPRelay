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
        private const int PURGE_CHECK_INTERVAL = 60000;

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
            List<tblProcessQueue> pendingQueue = new List<tblProcessQueue>();
            System.Diagnostics.Stopwatch purgeSW = new System.Diagnostics.Stopwatch();
            purgeSW.Start();
            
            try
            {
                Dictionary<long, int> GatewayUsageTrack = new Dictionary<long, int>();
                int QueryUpdateInterval = Int32.Parse(SQLiteDB.System_GetValue("SMTPSenderQueue", "RefreshMS"));
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogMessage = "Started Outbound Queue Monitor."
                });
                ResetPendingSendQueue();
                bool GatewayBackedUp = false;
                while (!Worker.CancellationPending)
                {
                    if (dbquerySW.ElapsedMilliseconds >= QueryUpdateInterval)
                    {
                        pendingQueue.Clear();
                        pendingQueue = SQLiteDB.ProcessQueue_GetReady();
                        dbquerySW.Restart();
                    }
                    if (purgeSW.ElapsedMilliseconds >= PURGE_CHECK_INTERVAL)
                    {
                        PurgeOldMail();
                        purgeSW.Restart();
                    }
                    bool busy = false;
                    if (pendingQueue.Count() > 0 && Senders.Count < MAX_ACTIVE_SENDERS)
                    {
                        tblProcessQueue sq = pendingQueue.First();
                        pendingQueue.RemoveAt(0);
                        // check to see if this item is using a MailGateway that is already at the usage limit. If so, we skip this item this tiem.
                        bool skipThisTime = false;
                        tblEnvelope envelope = SQLiteDB.Envelope_GetByID(sq.EnvelopeID);
                        tblUser user = null;
                        tblDevice device = null;
                        if (envelope.UserID.HasValue)
                        {
                            user = SQLiteDB.User_GetByID(envelope.UserID.Value);
                        }
                        if (envelope.DeviceID.HasValue)
                        {
                            device = SQLiteDB.Device_GetByID(envelope.DeviceID.Value);
                        }
                        tblMailGateway gateway = null;
                        if (user != null && user.MailGateway.HasValue)
                        {
                            gateway = SQLiteDB.MailGateway_GetByID(user.MailGateway.Value);
                        }
                        if (gateway == null && device != null && device.MailGateway.HasValue)
                        {
                            gateway = SQLiteDB.MailGateway_GetByID(device.MailGateway.Value);
                        }
                        long? GatewayToTrack = null;

                        if (gateway != null && gateway.ConnectionLimit.HasValue)
                        {
                            GatewayToTrack = gateway.MailGatewayID;
                            int currentConnCnt;
                            if (GatewayUsageTrack.TryGetValue(gateway.MailGatewayID.Value, out currentConnCnt))
                            {
                                if (currentConnCnt < gateway.ConnectionLimit)
                                {
                                    currentConnCnt++;
                                    GatewayUsageTrack[gateway.MailGatewayID.Value] = currentConnCnt;
                                }
                                else
                                {
                                    skipThisTime = true;
                                    GatewayBackedUp = true;
                                }
                            }
                            else
                            {
                                GatewayUsageTrack.Add(gateway.MailGatewayID.Value, 1);
                            }
                        }

                        if (!skipThisTime)
                        {
                            sq.State = QueueState.InProgress;
                            SQLiteDB.ProcessQueue_AddUpdate(sq);
                            SMTPSender snd = new SMTPSender(sq, GatewayToTrack);
                            Senders.Add(snd);
                            if (pendingQueue.Count() > 0)
                            {
                                busy = true;
                            }
                        }
                    }
                    else if (GatewayBackedUp || (pendingQueue.Count() > 0 && Senders.Count >= MAX_ACTIVE_SENDERS && cleanupSW.ElapsedMilliseconds > 100))
                    {
                        CleanupSenders(Senders, ref GatewayUsageTrack);
                        cleanupSW.Restart();
                    }
                    if (Senders.Count > 4 && cleanupSW.ElapsedMilliseconds > 500)
                    {
                        CleanupSenders(Senders, ref GatewayUsageTrack);
                        cleanupSW.Restart();
                    }
                    else if (cleanupSW.ElapsedMilliseconds >= 1000)
                    {
                        CleanupSenders(Senders, ref GatewayUsageTrack);
                        cleanupSW.Restart();
                    }
                    if (!busy)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    GatewayBackedUp = false;
                }

                // wind down the senders
                foreach (var snd in Senders)
                {
                    snd.Cancel();
                }
                while (!CleanupSenders(Senders, ref GatewayUsageTrack))
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

        private void ResetPendingSendQueue()
        {
            // Get a list of mail queue items that are busy. Since we're starting up, these were interrupted and need to be retried.
            List<tblProcessQueue> redoQueue = SQLiteDB.ProcessQueue_GetBusy();
            foreach (tblProcessQueue redoItem in redoQueue)
            {
                redoItem.State = QueueState.Ready;
                SQLiteDB.ProcessQueue_AddUpdate(redoItem);
            }
        }

        private void PurgeOldMail()
        {
            if (!TimeToPurge())
            {
                return; 
            }
            Worker.ReportProgress(0, new WorkerReport()
            {
                LogMessage = "Running old email purge."
            });
            long retainMins = long.Parse(SQLiteDB.System_GetValue("Message", "DataRetainMins"));
            long purgeFailedMins = long.Parse(SQLiteDB.System_GetValue("Message", "PurgeFailedMins"));

            DateTime CompleteCutoff = DateTime.Now.AddMinutes(-retainMins);
            DateTime FailedCutoff = DateTime.Now.AddMinutes(-purgeFailedMins);

            //string CompleteCutoffStr = CompleteCutoff.ToUniversalTime().ToString("O");
            //string FailedCutoffStr = FailedCutoff.ToUniversalTime().ToString("O");

            try
            {
                long itemCount = SQLiteDB.Envelope_PurgeOldItems(CompleteCutoff, FailedCutoff);
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogMessage = string.Format("Email purge complete: {0} items.", itemCount)
                });
            }
            catch (Exception ex)
            {
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogMessage = string.Format("Email purge failed: {0}", ex.Message)
                });
            }
            SQLiteDB.System_AddUpdateValue("Purge", "LastRun", DateTime.Now.ToUniversalTime().ToString("O"));
        }

        private bool TimeToPurge()
        {
            string LastRunStr = SQLiteDB.System_GetValue("Purge", "LastRun");
            if (string.IsNullOrWhiteSpace(LastRunStr))
            {
                return true;    // never run
            }
            DateTime parse;
            if (DateTime.TryParse(LastRunStr, out parse))
            {
                long PurgeFrequency = long.Parse(SQLiteDB.System_GetValue("Purge", "FrequencyMins"));
                if (parse.AddMinutes(PurgeFrequency) < DateTime.Now)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return true;
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
        private bool CleanupSenders(List<SMTPSender> senders, ref Dictionary<long,int> gatewayUsageTrack)
        {
            List<SMTPSender> remove = new List<SMTPSender>();
            foreach (var snd in senders)
            {
                TakeReports(snd);
                if (!snd.Running)
                {
                    remove.Add(snd);
                    if (snd.GatewayIdInUse.HasValue)
                    {
                        int gwUseCount;
                        if (gatewayUsageTrack.TryGetValue(snd.GatewayIdInUse.Value, out gwUseCount))
                        {
                            gwUseCount--;
                            gatewayUsageTrack[snd.GatewayIdInUse.Value] = gwUseCount;
                        }
                    }
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
