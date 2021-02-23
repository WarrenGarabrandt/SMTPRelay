using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Models
{
    //@"CREATE TABLE SendQueue (SendQueueID INTEGER PRIMARY KEY, EnvelopeID NOT NULL INTEGER, Recipient TEXT, State INTEGER, AttemptCount INTEGER, RetryAfter TEXT);",
    public class SendQueue
    {
        public long SendQueueID { get; set; }
        public long EnvelopeID { get; set; }
        public string Recipient { get; set; }
        public long State { get; set; }
        public long AttemptCount { get; set; }
        public DateTime RetryAfter { get; set; }
    }
}
