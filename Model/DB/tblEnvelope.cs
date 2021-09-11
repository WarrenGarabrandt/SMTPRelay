using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblEnvelope
    {
        //CREATE TABLE Envelope (EnvelopeID INTEGER PRIMARY KEY, WhenReceived TEXT, Sender TEXT, ChunkCount INTEGER);

        /// <summary>
        /// Creates a new instance that has not been saved into the database yet.
        /// </summary>
        public tblEnvelope(DateTime whenReceived, string sender, int chunkCount)
        {
            EnvelopeID = null;
            WhenReceived = whenReceived;
            Sender = sender;
            ChunkCount = chunkCount;
        }

        /// <summary>
        /// Creates a new instance that existes in the database.
        /// </summary>
        public tblEnvelope(int envelopeID, string whenReceived, string sender, int chunkCount)
        {
            EnvelopeID = envelopeID;
            DateTime.UtcNow.ToString();
            WhenReceived = DateTime.Parse(whenReceived);
            Sender = sender;
            ChunkCount = chunkCount;
        }

        public int? EnvelopeID { get; set; }

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

        public int ChunkCount { get; set; }
    }
}
