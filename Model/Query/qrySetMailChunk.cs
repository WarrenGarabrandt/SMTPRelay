using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qrySetMailChunk : DatabaseQuery
    {
        public long EnvelopeID { get; private set; }
        public int ChunkID { get; private set; }
        public byte[] Buffer { get; private set; }

        public bool SuccessResult { get; private set; }

        public qrySetMailChunk(long envelopeID, int chunkID, byte[] buffer)
        {
            EnvelopeID = envelopeID;
            ChunkID = chunkID;
            Buffer = buffer;
            DoneSignal = new System.Threading.ManualResetEventSlim();
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
