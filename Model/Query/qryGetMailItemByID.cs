using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetMailItemByID : DatabaseQuery
    {
        public long MailItemId { get; private set; }
        public tblMailItem ValueResult { get; set; }

        public qryGetMailItemByID(long mailitemId)
        {
            MailItemId = mailitemId;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(tblMailItem value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public tblMailItem GetResult()
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
