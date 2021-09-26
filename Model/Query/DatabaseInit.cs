using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class DatabaseInit : DatabaseQuery
    {
        public WorkerReport ValueResult { get; set; }

        public DatabaseInit()
        {
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }
        
        public void SetResult(WorkerReport value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public WorkerReport GetResult()
        {
            DoneSignal.Wait();
            DoneSignal.Dispose();
            if (Aborted)
            {
                throw new OperationCanceledException();
            }
            return ValueResult;
        }
    }
}
