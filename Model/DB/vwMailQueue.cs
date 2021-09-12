using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class vwMailQueue
    {
        public vwMailQueue(string sender, string recipients, string dateReceived, string dateRetry, long attemptCount)
        {
            Sender = sender;
            Recipients = recipients;
            DateTime parse;
            if (DateTime.TryParse(dateReceived, out parse))
            {
                DateReceived = parse;
            }
            else
            {
                DateReceived = null;
            }
            if (DateTime.TryParse(dateRetry, out parse))
            {
                DateRetry = parse;
            }
            else
            {
                DateRetry = null;
            }
            AttemptCount = attemptCount;
        }

        public string Sender { get; set; }
        public string Recipients { get; set; }
        public DateTime? DateReceived { get; set; }
        public DateTime? DateRetry { get; set; }
        public long AttemptCount { get; set; }
    }
}
