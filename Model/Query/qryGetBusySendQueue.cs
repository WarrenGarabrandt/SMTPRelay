using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetBusySendQueue : DatabaseQuery
    {
        public List<tblSendQueue> Results;
        public qryGetBusySendQueue()
        {
            Results = null;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(List<tblSendQueue> value)
        {
            Results = value;
            DoneSignal.Set();
        }

        public List<tblSendQueue> GetResult()
        {
            DoneSignal.Wait();
            DoneSignal.Dispose();
            if (Aborted)
            {
                throw new OperationCanceledException();
            }
            return Results;
        }
    }
}
