using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qrySetDeliveryReportEnque : DatabaseQuery
    {
        public bool SuccessResult { get; private set; }

        public tblDeliveryReport DeliveryReport { get; private set; }

        /// <summary>
        /// Inserts a new DeliveryReport record. Make sure the following fields are populated:
        /// WhenScheduled, OriginalEnvelopeRcptID, ReportType, Reason
        /// </summary>
        /// <param name="deliveryReport"></param>
        public qrySetDeliveryReportEnque(tblDeliveryReport deliveryReport)
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
