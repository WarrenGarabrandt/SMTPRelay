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
        private BackgroundWorker Worker = null;
        public bool Running;
        public ConcurrentQueue<WorkerReport> WorkerReports;

        public SMTPListener()
        {
            WorkerReports = new ConcurrentQueue<WorkerReport>();
            Running = true;
            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += Worker_DoWork;
            Worker.ProgressChanged += Worker_ProgressChanged;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        public void Cancel()
        {
            Worker.CancelAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            TcpListener listener = null;
            List<SMTPReceiver> Receivers = new List<SMTPReceiver>();

            try
            {
                // create listener
                listener = new TcpListener(System.Net.IPAddress.Any, 25);
                listener.Start();
                while (!Worker.CancellationPending)
                {
                    bool busy = false;
                    if (listener.Pending())
                    {
                        TcpClient newClient = listener.AcceptTcpClient();
                        SMTPReceiver rcv = new SMTPReceiver(newClient);
                        Receivers.Add(rcv);
                        busy = true;
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
