using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetMailChunkData : DatabaseQuery
    {
        public long EnvelopeID { get; private set; }
        public long ChunkID { get; private set; }
        public byte[] ValueResult { get; set; }

        public qryGetMailChunkData(long envelopeID, long chunkID)
        {
            EnvelopeID = envelopeID;
            ChunkID = chunkID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(byte[] value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public byte[] GetResult()
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
