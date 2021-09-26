using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetSendLogPage : DatabaseQuery
    {
        public long Count { get; private set; }
        public long Offset { get; private set; }

        public List<tblSendLog> Results;
        public qryGetSendLogPage(long count, long offset)
        {
            Count = count;
            Offset = offset;
            Results = null;
            DoneSignal = new System.Threading.ManualResetEventSlim();
        }

        public void SetResult(List<tblSendLog> value)
        {
            Results = value;
            DoneSignal.Set();
        }

        public List<tblSendLog> GetResult()
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
