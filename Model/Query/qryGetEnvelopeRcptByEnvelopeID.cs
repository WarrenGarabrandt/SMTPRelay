using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetEnvelopeRcptByEnvelopeID : DatabaseQuery
    {
        public long EnvelopeID { get; private set; }
        public List<tblEnvelopeRcpt> ValueResult { get; set; }

        public qryGetEnvelopeRcptByEnvelopeID(long envelopeID)
        {
            EnvelopeID = envelopeID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
        }

        public void SetResult(List<tblEnvelopeRcpt> value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public List<tblEnvelopeRcpt> GetResult()
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
