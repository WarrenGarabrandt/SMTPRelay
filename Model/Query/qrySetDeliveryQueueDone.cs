using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qrySetDeliveryQueueDone : DatabaseQuery
    {
        public bool SuccessResult { get; private set; }

        public tblDeliveryReport DeliveryReport { get; private set; }

        /// <summary>
        /// Updates a DeliveryReport record, marks done. Make sure the following fields are populated:
        /// WhenGenerated, EnvelopeID, Status, DeliveryReportID
        /// </summary>
        public qrySetDeliveryQueueDone(tblDeliveryReport deliveryReport)
        {
            DeliveryReport = deliveryReport;
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
