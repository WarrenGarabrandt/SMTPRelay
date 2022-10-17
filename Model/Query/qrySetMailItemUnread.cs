using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qrySetMailItemUnread : DatabaseQuery
    {
        public bool SuccessResult { get; private set; }

        public long MailItemID { get; private set; }

        public bool Unread { get; private set; }

        public qrySetMailItemUnread(long mailItemID, bool unread)
        {
            MailItemID = mailItemID;
            Unread = unread;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(bool result)
        {
            SuccessResult = result;
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
            return SuccessResult;
        }

    }
}
