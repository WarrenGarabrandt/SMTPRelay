using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetAllEnvelopes : DatabaseQuery
    {
        public List<tblEnvelope> Results;
        public qryGetAllEnvelopes()
        {
            Results = null;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(List<tblEnvelope> value)
        {
            Results = value;
            DoneSignal.Set();
        }

        public List<tblEnvelope> GetResult()
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
