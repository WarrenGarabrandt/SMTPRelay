using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qrySetUser : DatabaseQuery
    {
        public tblUser User { get; private set; }
        public bool ValueResult { get; private set; }
        public qrySetUser(tblUser user)
        {
            User = user;
            DoneSignal = new System.Threading.ManualResetEventSlim();
        }

        public void SetResult(bool value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public bool GetResult()
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
