using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetUserByID : DatabaseQuery
    {
        public long UserID { get; private set; }
        public tblUser ValueResult { get; set; }

        public qryGetUserByID(long userID)
        {
            UserID = userID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(tblUser value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public tblUser GetResult()
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
