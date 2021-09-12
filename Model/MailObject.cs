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
            ChunkData = new StringBuilder();
            MessageSize = 0;
        }

        public string Sender { get; set; }
        public List<string> Recipients { get; set; }
        public StringBuilder ChunkData { get; set; }
        public long MessageSize { get; set; }
    }
}
