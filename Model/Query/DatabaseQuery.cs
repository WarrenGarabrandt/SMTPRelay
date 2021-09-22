using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public abstract class DatabaseQuery
    {
        internal System.Threading.ManualResetEventSlim DoneSignal;
        public bool Aborted { get; internal set; }
        public void Abort()
        {
            Aborted = true;
            DoneSignal.Set();
        }
    }
}
