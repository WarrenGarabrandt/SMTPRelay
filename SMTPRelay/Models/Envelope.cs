using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Models
{
    //@"CREATE TABLE Envelope (EnvelopeID INTEGER PRIMARY KEY, WhenReceived TEXT, Sender TEXT, Recipients TEXT, ChunkCount INTEGER);",
    public class Envelope
    {
        public long EnvelopeID { get; set; }
        public DateTime WhenReceived { get; set; }
        public string Sender { get; set; }
        public string[] Recipients { get; set; }
        public long ChunkCount { get; set; }

    }
}
