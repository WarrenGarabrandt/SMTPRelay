using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class vwMailInbox
    {
        public vwMailInbox(long mailItemId, long envelopeId, int unreadInt, string whenReceived, string sender)
        {
            MailItemID = mailItemId;
            EnvelopeID = envelopeId;
            UnreadInt = unreadInt;
            WhenReceivedString = whenReceived;
            Sender = sender;
        }

        public long MailItemID { get; set; }
        public long EnvelopeID { get; set; }
        public bool Unread { get; set; }
        public int UnreadInt
        {
            get
            {
                return Unread ? 1 : 0;
            }
            private set
            {
                Unread = value > 0;
            }
        }

        public DateTime WhenReceived { get; set; }

        public string WhenReceivedString
        {
            get
            {
                return WhenReceived.ToUniversalTime().ToString("O");
            }
            private set
            {
                WhenReceived = DateTime.Parse(value);
            }
        }

        public string Sender { get; set; }

    }
}
