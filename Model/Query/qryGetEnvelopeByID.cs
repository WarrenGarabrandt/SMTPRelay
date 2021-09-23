using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetEnvelopeByID : DatabaseQuery
    {
        public long EnvelopeID { get; private set; }
        public tblEnvelope ValueResult { get; set; }

        public qryGetEnvelopeByID(long envelopeID)
        {
            EnvelopeID = envelopeID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
        }

        public void SetResult(tblEnvelope value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public tblEnvelope GetResult()
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
