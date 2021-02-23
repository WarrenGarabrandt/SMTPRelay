using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Models
{
    //@"CREATE TABLE MailChunk (EnvelopeID NOT NULL INTEGER, ChunkID NOT NULL INTEGER, Chunk TEXT);",
    public class MailChunk
    {
        public long EnvelopeID { get; set; }
        public long ChunkID { get; set; }
        public string Chunk { get; set; }
    }
}
