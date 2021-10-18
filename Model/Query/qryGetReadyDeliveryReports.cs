using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetReadyDeliveryReports : DatabaseQuery
    {
        public List<tblDeliveryReport> Results;
        public qryGetReadyDeliveryReports()
        {
            Results = null;
            DoneSignal = new System.Threading.ManualResetEventSlim();
        }

        public void SetResult(List<tblDeliveryReport> value)
        {
            Results = value;
            DoneSignal.Set();
            Aborted = false;
        }

        public List<tblDeliveryReport> GetResult()
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
