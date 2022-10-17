using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblEnvelope
    {

        /// <summary>
        /// Creates a new instance that has not been saved into the database yet.
        /// UserID, DeviceID, WhenReceived, Sender, Recipients, ChunkCount, MsgID
        /// </summary>
        public tblEnvelope(long? userID, long? deviceID, DateTime whenReceived, string sender, string recipients, int chunkCount, string msgId)
        {
            EnvelopeID = null;
            UserID = userID;
            DeviceID = deviceID;
            WhenReceived = whenReceived;
            Sender = sender;
            Recipients = recipients;
            ChunkCount = chunkCount;
            MsgID = msgId;
        }

        /// <summary>
        /// Creates a new instance that existes in the database.
        /// </summary>
        public tblEnvelope(long envelopeID, long? userID, long? deviceID, string whenReceived, string sender, string recipients, int chunkCount, string msgId)
        {
            EnvelopeID = envelopeID;
            UserID = userID;
            DeviceID = deviceID;
            //DateTime.UtcNow.ToString();
            WhenReceived = DateTime.Parse(whenReceived);
            Sender = sender;
            Recipients = recipients;
            ChunkCount = chunkCount;
            MsgID = msgId;
        }

        public long? EnvelopeID { get; set; }

        public long? UserID { get; set; }

        public long? DeviceID { get; set; }

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
        
        public string MsgID { get; set; }
    }
}
