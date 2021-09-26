using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qrySetEnvelopeChunkCount : DatabaseQuery
    {

        public bool SuccessResult { get; private set; }

        public long EnvelopeID { get; private set; }

        public int ChunkCount { get; private set; }

        public qrySetEnvelopeChunkCount(long envelopeID, int chunkCount)
        {
            EnvelopeID = envelopeID;
            ChunkCount = chunkCount;
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
