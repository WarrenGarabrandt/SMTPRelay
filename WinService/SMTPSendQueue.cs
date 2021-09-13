using SMTPRelay.Model;
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
            try
            {
                //// create listener
                //listener = new TcpListener(System.Net.IPAddress.Any, 10025);

                //listener.Start();
                //Worker.ReportProgress(0, new WorkerReport()
                //{
                //    LogMessage = "Started SMTP Listener."
                //});
                while (!Worker.CancellationPending)
                {
                    bool busy = false;
                    //if (listener.Pending())
                    //{
                    //    TcpClient newClient = listener.AcceptTcpClient();
                    //    SMTPReceiver rcv = new SMTPReceiver(newClient);
                    //    Receivers.Add(rcv);
                    //    busy = true;
                    //}
                    if (!busy)
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                }

                //// wind down the clients.
                //foreach (var rcv in Receivers)
                //{
                //    rcv.Cancel();
                //}
                //while (!CleanupReceivers(Receivers))
                //{
                //    System.Threading.Thread.Sleep(100);
                //}
            }
            catch (Exception ex)
            {
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogError = ex.Message
                });
                //if (listener != null)
                //{
                //    try
                //    {
                //        listener.Stop();
                //    }
                //    catch { }
                //    listener = null;
                //}
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
