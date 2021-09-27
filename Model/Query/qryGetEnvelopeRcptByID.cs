using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetEnvelopeRcptByID : DatabaseQuery
    {
        public long EnvelopeRcptID { get; private set; }
        public tblEnvelopeRcpt ValueResult { get; set; }

        public qryGetEnvelopeRcptByID(long envelopeRcptID)
        {
            EnvelopeRcptID = envelopeRcptID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(tblEnvelopeRcpt value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public tblEnvelopeRcpt GetResult()
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
