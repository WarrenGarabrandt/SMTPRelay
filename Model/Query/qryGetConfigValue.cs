﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryGetConfigValue : DatabaseQuery
    {
        public qryGetConfigValue(string category, string setting)
        {
            Category = category;
            Setting = setting;
            DoneSignal = new System.Threading.ManualResetEventSlim();
        }

        private System.Threading.ManualResetEventSlim DoneSignal { get; set; }

        public void SetResult(string value)
        {
            ValueResult = value;
            DoneSignal.Set();
        }

        public string GetResult()
        {
            DoneSignal.Wait();
            DoneSignal.Dispose();
            return ValueResult;
        }

        public string Category { get; private set; }
        public string Setting { get; private set; }
        public string ValueResult { get; set; }
    }
}
