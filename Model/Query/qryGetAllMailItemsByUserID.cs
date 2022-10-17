using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetAllMailItemsByUserID : DatabaseQuery
    {
        public long UserId { get; private set; }
        public List<tblMailItem> ValueResult { get; set; }

        public qryGetAllMailItemsByUserID(long userId)
        {
            UserId = userId;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(List<tblMailItem> value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public List<tblMailItem> GetResult()
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
