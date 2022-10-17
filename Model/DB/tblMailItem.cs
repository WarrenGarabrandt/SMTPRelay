using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblMailItem
    {
        /// <summary>
        /// Creates a new instance that has not been saved in the database
        /// </summary>
        public tblMailItem(long userId, long envelopeId, bool unread)
        {
            MailItemID = null;
            UserID = userId;
            EnvelopeID = envelopeId;
            Unread = unread;
        }

        /// <summary>
        /// Creates a new instance that exists in the database
        /// </summary>
        public tblMailItem(long mailItemId, long userId, long envelopeId, int unreadInt)
        {
            MailItemID = mailItemId;
            UserID = userId;
            EnvelopeID = envelopeId;
            UnreadInt = unreadInt;
        }

        public long? MailItemID { get; set; }
        public long UserID { get; set; }
        public long EnvelopeID { get; set; }
        public bool Unread { get; set; }
        public int UnreadInt
        {
            get
            {
                return Unread ? 1 : 0;
            }
            set
            {
                Unread = value > 0;
            }
        }

    }
}
