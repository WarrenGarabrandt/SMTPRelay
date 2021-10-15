using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetDeviceByID : DatabaseQuery
    {
        public long DeviceID { get; private set; }
        public tblDevice ValueResult { get; set; }

        public qryGetDeviceByID(long deviceID)
        {
            DeviceID = deviceID;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(tblDevice value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public tblDevice GetResult()
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
