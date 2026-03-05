using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetProcessQueueByID : DatabaseQuery
    {
        public long ProcessQueueID { get; private set; }
        public tblProcessQueue ValueResult { get; set; }

        public qryGetProcessQueueByID(long processQueueID)
        {

            ProcessQueueID = processQueueID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(tblProcessQueue value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public tblProcessQueue GetResult()
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
