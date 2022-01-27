using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public enum QueueState
    {
        Disabled = -2,
        SendAdminFailure = -1,
        Ready = 0,
        InProgress = 1,
        Done = 2
    }
}
