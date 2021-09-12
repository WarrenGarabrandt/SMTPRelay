using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model
{
    /// <summary>
    /// Stores a mail item while the client sends the MAIL FROM and RCPT TO messages. 
    /// When the DATA command is sent, the object gets inserted into the database tables.
    /// </summary>
    public class MailObject
    {
        public MailObject(string sender)
        {
            Sender = sender;
            Recipients = new List<string>();
            ChunkCount = 0;
        }

        public string Sender { get; set; }
        public List<string> Recipients { get; set; }
        // Tracks the chunk index for next insert. Increment after each insert. update the count in the Envelope after successful chunk insert.
        public int ChunkCount { get; set; }
    }
}
