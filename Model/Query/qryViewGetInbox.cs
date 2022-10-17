using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryViewGetInbox : DatabaseQuery
    {
        public List<vwMailInbox> Results;
        public long UserId { get; private set; }
        public qryViewGetInbox(long userId)
        {
            UserId = userId;
            Results = null;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(List<vwMailInbox> value)
        {
            Results = value;
            DoneSignal.Set();
        }

        public List<vwMailInbox> GetResult()
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
