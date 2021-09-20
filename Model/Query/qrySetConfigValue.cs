using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qrySetConfigValue : DatabaseQuery
    {
        public qrySetConfigValue(string category, string setting, string value)
        {
            Category = category;
            Setting = setting;
            Value = value;
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
            return SuccessResult;
        }

        private System.Threading.ManualResetEventSlim DoneSignal { get; set; }

        public string Category { get; private set; }
        public string Setting { get; private set; }
        public string Value { get; private set; }

        public bool SuccessResult { get; private set; }
    }
}
