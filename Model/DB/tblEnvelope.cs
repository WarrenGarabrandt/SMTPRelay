using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblEnvelope
    {
        //CREATE TABLE Envelope (EnvelopeID INTEGER PRIMARY KEY, UserID INTEGER NOT NULL, WhenReceived TEXT, Sender TEXT, ChunkCount INTEGER);

        /// <summary>
        /// Creates a new instance that has not been saved into the database yet.
        /// </summary>
        public tblEnvelope(long userID, DateTime whenReceived, string sender, string recipients, int chunkCount)
        {
            EnvelopeID = null;
            WhenReceived = whenReceived;
            Sender = sender;
            Recipients = recipients;
            ChunkCount = chunkCount;
        }

        /// <summary>
        /// Creates a new instance that existes in the database.
        /// </summary>
        public tblEnvelope(long envelopeID, long userID, string whenReceived, string sender, string recipients, int chunkCount)
        {
            EnvelopeID = envelopeID;
            DateTime.UtcNow.ToString();
            WhenReceived = DateTime.Parse(whenReceived);
            Sender = sender;
            Recipients = recipients;
            ChunkCount = chunkCount;
        }

        public long? EnvelopeID { get; set; }

        public long UserID { get; set; }

        public DateTime WhenReceived { get; set; }

        public string WhenReceivedString
        {
            get
            {
                return WhenReceived.ToUniversalTime().ToString("O");
            }
            set
            {
                WhenReceived = DateTime.Parse(value);
            }
        }

        public string Sender { get; set; }

        public string Recipients { get; set; }

        public int ChunkCount { get; set; }
    }
}
