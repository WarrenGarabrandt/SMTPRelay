using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetAllMailGateways : DatabaseQuery
    {
        public List<tblMailGateway> Results;
        public qryGetAllMailGateways()
        {
            Results = null;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(List<tblMailGateway> value)
        {
            Results = value;
            DoneSignal.Set();
        }

        public List<tblMailGateway> GetResult()
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
