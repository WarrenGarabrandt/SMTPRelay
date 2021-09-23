using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryViewMailQueue : DatabaseQuery
    {
        public List<vwMailQueue> Results;
        public qryViewMailQueue()
        {
            Results = null;
            DoneSignal = new System.Threading.ManualResetEventSlim();
        }

        public void SetResult(List<vwMailQueue> value)
        {
            Results = value;
            DoneSignal.Set();
        }

        public List<vwMailQueue> GetResult()
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
