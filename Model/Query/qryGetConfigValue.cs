using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetConfigValue : DatabaseQuery
    {
        public string Category { get; private set; }
        public string Setting { get; private set; }
        public string ValueResult { get; set; }

        public qryGetConfigValue(string category, string setting)
        {
            Category = category;
            Setting = setting;
            DoneSignal = new System.Threading.ManualResetEventSlim();
            Aborted = false;
        }

        public void SetResult(string value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public string GetResult()
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
