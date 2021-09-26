using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetMailGatewayByID : DatabaseQuery
    {
        public long MailGatewayID { get; private set; }
        public tblMailGateway ValueResult { get; set; }

        public qryGetMailGatewayByID(long mailGatewayID)
        {
            MailGatewayID = mailGatewayID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(tblMailGateway value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public tblMailGateway GetResult()
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
