using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryDeleteMailGatwayByID : DatabaseQuery
    {
        public long MailGatewayID { get; private set; }
        public bool ValueResult { get; set; }

        public qryDeleteMailGatwayByID(long mailGatewayID)
        {
            MailGatewayID = mailGatewayID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
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
