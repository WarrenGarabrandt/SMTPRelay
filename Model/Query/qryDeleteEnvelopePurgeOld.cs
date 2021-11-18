using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryDeleteEnvelopePurgeOld : DatabaseQuery
    {
        public DateTime SuccessCutOff { get; private set; }
        public DateTime FailedCutOFf { get; private set; }

        public long ValueResult { get; set; }

        public qryDeleteEnvelopePurgeOld(DateTime successCutOff, DateTime failedCutOFf)
        {
            SuccessCutOff = successCutOff;
            FailedCutOFf = failedCutOFf;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(long value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public long GetResult()
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
