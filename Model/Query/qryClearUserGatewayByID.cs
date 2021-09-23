﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.Query
{
    public class qryClearUserGatewayByID : DatabaseQuery
    {

        public bool SuccessResult { get; private set; }

        public long GatewayID { get; private set; }

        public qryClearUserGatewayByID(long gatewayID)
        {
            GatewayID = gatewayID;
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
            if (Aborted)
            {
                throw new OperationCanceledException();
            }
            return SuccessResult;
        }

    }
}
