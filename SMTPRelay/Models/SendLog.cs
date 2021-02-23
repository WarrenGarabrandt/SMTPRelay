using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Models
{
    //@"CREATE TABLE SendLog (EnvelopeID NOT NULL INTEGER, Recipient TEXT, WhenSent TEXT, Results TEXT, AttemptCount INTEGER);"
    public class SendLog
    {
        public long EnvelopeID { get; set; }
        public string Recipient { get; set; }
        public DateTime WhenSent { get; set; }
        public string Results { get; set; }
        public long AttemptCount { get; set; }
    }
}
