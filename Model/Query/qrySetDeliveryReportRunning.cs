using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qrySetDeliveryReportRunning : DatabaseQuery
    {
        public bool SuccessResult { get; private set; }

        public long DeliveryReportID { get; private set; }

        public qrySetDeliveryReportRunning(long deliveryReportID)
        {
            DeliveryReportID = deliveryReportID;
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
