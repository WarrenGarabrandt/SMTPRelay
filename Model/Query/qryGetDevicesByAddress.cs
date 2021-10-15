using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetDevicesByAddress : DatabaseQuery
    {
        public string Address { get; private set; }
        public List<tblDevice> ValueResult { get; set; }

        public qryGetDevicesByAddress(string address)
        {
            Address = address;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(List<tblDevice> value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public List<tblDevice> GetResult()
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
