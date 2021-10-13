using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetIPEndpointByID : DatabaseQuery
    {
        public long IPEndpointID { get; private set; }
        public tblIPEndpoint ValueResult { get; private set; }

        public qryGetIPEndpointByID(long ipendpointID)
        {
            IPEndpointID = ipendpointID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(tblIPEndpoint value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public tblIPEndpoint GetResult()
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
