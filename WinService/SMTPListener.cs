using SMTPRelay.Database;
using SMTPRelay.Model;
using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Collections.Concurrent;

namespace SMTPRelay.WinService
{
    public class SMTPListener
    {
        private const int MAX_ACTIVE_RECEIVERS = 20;

        private BackgroundWorker Worker = null;
        public bool Running;
        public ConcurrentQueue<WorkerReport> WorkerReports;

        public SMTPListener(tblIPEndpoint endpoint)
        {
            WorkerReports = new ConcurrentQueue<WorkerReport>();
            Running = true;
            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += Worker_DoWork;
            Worker.ProgressChanged += Worker_ProgressChanged;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            Worker.RunWorkerAsync(endpoint);
        }

        public void Cancel()
        {
            Worker.CancelAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            tblIPEndpoint endpoint = e.Argument as tblIPEndpoint;
            if (endpoint == null)
            {
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogError = "SMTP Listener not provided a valid Endpoint."
                });
                return;
            }
            TcpListener listener = null;
            List<SMTPReceiver> Receivers = new List<SMTPReceiver>();
            System.Diagnostics.Stopwatch cleanupSW = new System.Diagnostics.Stopwatch();
            cleanupSW.Start();
            try
            {
                //string readValue = SQLiteDB.System_GetValue("SMTPListener", "ListenAddr");
                //IPAddress listenAddr = IPAddress.None;
                //if (readValue == "0.0.0.0")
                //{
                //    listenAddr = IPAddress.Any;
                //}
                //else
                //{
                //    if (!IPAddress.TryParse(readValue, out listenAddr))
                //    {
                //        throw new Exception("Invalid SMTPListener ListenAddr");
                //    }
                //}
                //readValue = SQLiteDB.System_GetValue("SMTPListener", "ListenPort");
                //int listenPort = 0;
                //if (!int.TryParse(readValue, out listenPort))
                //{
                //    throw new Exception("Invalid SMTPListener ListenPort");
                //}
                // create listener
                listener = new TcpListener(endpoint.IPEndPoint);
                listener.Start();
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogMessage = string.Format("Started {0} Listener on {1}:{2} with TLS {3} (Cert: {4}).", 
                    endpoint.ProtocolString, endpoint.Address, endpoint.Port, endpoint.TLSModeString, endpoint.CertFriendlyName)
                });
                while (!Worker.CancellationPending)
                {
                    bool busy = false;
                    if (listener.Pending() && Receivers.Count < MAX_ACTIVE_RECEIVERS)
                    {
                        TcpClient newClient = listener.AcceptTcpClient();
                        SMTPReceiver rcv = new SMTPReceiver(newClient);
                        Receivers.Add(rcv);
                        busy = true;
                    }
                    else if (listener.Pending() && Receivers.Count >= MAX_ACTIVE_RECEIVERS && cleanupSW.ElapsedMilliseconds > 100)
                    {
                        CleanupReceivers(Receivers);
                        cleanupSW.Restart();
                    }

                    if (Receivers.Count > 4 && cleanupSW.ElapsedMilliseconds > 500)
                    {
                        CleanupReceivers(Receivers);
                        cleanupSW.Restart();
                    }
                    else if (cleanupSW.ElapsedMilliseconds >= 1000)
                    {
                        CleanupReceivers(Receivers);
                        cleanupSW.Restart();
                    }
                    
                    if (!busy)
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                }

                // wind down the clients.
                foreach (var rcv in Receivers)
                {
                    rcv.Cancel();
                }
                while (!CleanupReceivers(Receivers))
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
                if (listener != null)
                {
                    try
                    {
                        listener.Stop();
                    }
                    catch { }
                    listener = null;
                }
            }
            finally
            {
                e.Result = new WorkerReport()
                {
                    LogMessage = "SMTP Listener shut down."
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
        /// Remove all dead receivers from the collection and returns True if the list if empty.
        /// </summary>
        /// <param name="receivers"></param>
        /// <returns></returns>
        private bool CleanupReceivers(List<SMTPReceiver> receivers)
        {
            List<SMTPReceiver> remove = new List<SMTPReceiver>();
            foreach (var rcv in receivers)
            {
                TakeReports(rcv);
                if (!rcv.Running)
                {
                    remove.Add(rcv);
                }
            }
            foreach (var rcv in remove)
            {
                receivers.Remove(rcv);
                TakeReports(rcv);
            }
            remove.Clear();
            return receivers.Count == 0;
        }

        private void TakeReports(SMTPReceiver receiver)
        {
            WorkerReport rpt;
            while (receiver.WorkerReports.TryDequeue(out rpt))
            {
                WorkerReports.Enqueue(rpt);
            }
        }

    }
}
