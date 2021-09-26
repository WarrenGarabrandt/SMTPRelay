using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetAllUsers : DatabaseQuery
    {
        public List<tblUser> Results;
        public qryGetAllUsers()
        {
            Results = null;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(List<tblUser> value)
        {
            Results = value;
            DoneSignal.Set();
        }

        public List<tblUser> GetResult()
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
