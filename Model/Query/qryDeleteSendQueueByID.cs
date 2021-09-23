using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryDeleteSendQueueByID : DatabaseQuery
    {
        public long SendQueueID { get; private set; }
        public bool ValueResult { get; set; }

        public qryDeleteSendQueueByID(long sendQueueID)
        {
            SendQueueID = sendQueueID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
        }

        public void SetResult(bool value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public bool GetResult()
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
