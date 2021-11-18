using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetReadyProcessQueue : DatabaseQuery
    {
        public List<tblProcessQueue> Results;
        public qryGetReadyProcessQueue()
        {
            Results = null;
            DoneSignal = new System.Threading.ManualResetEventSlim();
        }

        public void SetResult(List<tblProcessQueue> value)
        {
            Results = value;
            DoneSignal.Set();
            Aborted = false;
        }

        public List<tblProcessQueue> GetResult()
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
