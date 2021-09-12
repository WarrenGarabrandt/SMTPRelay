using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblEnvelopeRcpt
    {
        /// <summary>
        /// Creates a new instance for a record that already exists in the database.
        /// </summary>
        public tblEnvelopeRcpt(long envelopeRcptID, long envelopeID, string recipient)
        {
            EnvelopeRcptID = envelopeRcptID;
            EnvelopeID = envelopeID;
            Recipient = recipient;
        }

        /// <summary>
        /// Creates a new instance for a record that does not yet exist in the database.
        /// </summary>
        public tblEnvelopeRcpt(long envelopeID, string recipient)
        {
            EnvelopeRcptID = null;
            EnvelopeID = envelopeID;
            Recipient = recipient;
        }

        public long? EnvelopeRcptID { get; set; }

        public long EnvelopeID { get; set; }

        public string Recipient { get; set; }
    }
}
