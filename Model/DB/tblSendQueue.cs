using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblSendQueue
    {
        
        /// <summary>
        /// Creates a new instance for an item that already exists in the database.
        /// </summary>
        public tblSendQueue(long sendQueueID, long envelopeID, long envelopeRcptID, int state, int attemptCount, string retryAfter)
        {
            SendQueueID = sendQueueID;
            EnvelopeID = envelopeID;
            EnvelopeRcptID = envelopeRcptID;
            StateInt = state;
            AttemptCount = attemptCount;
            RetryAfterStr = retryAfter;
        }

        /// <summary>
        /// Creates a new instance for an item that does not yet exist in the databse.
        /// </summary>
        public tblSendQueue(long envelopeID, long envelopeRcptID, SendQueueState state, int attemptCount, DateTime? retryAfter)
        {
            SendQueueID = null;
            EnvelopeID = envelopeID;
            EnvelopeRcptID = envelopeRcptID;
            State = state;
            AttemptCount = attemptCount;
            RetryAfter = retryAfter;
        }

        public long? SendQueueID { get; set; }

        // private set because there isn't a SQL UPDATE statement for this field.
        public long EnvelopeID { get; private set; }

        // private set because there isn't a SQL UPDATE statement for this field.
        public long EnvelopeRcptID { get; private set; }

        public SendQueueState State { get; set; }

        public int StateInt
        {
            get
            {
                return (int)State;
            }
            set
            {
                State = (SendQueueState)value;
            }
        }

        public int AttemptCount { get; set; }

        public DateTime? RetryAfter { get; set; }

        public string RetryAfterStr
        {
            get
            {
                if (RetryAfter.HasValue)
                {
                    return RetryAfter.Value.ToUniversalTime().ToString("O");
                }
                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    RetryAfter = null;
                }
                else
                {
                    DateTime parse;
                    if (DateTime.TryParse(value, out parse))
                    {
                        RetryAfter = parse;
                    }
                    else
                    {
                        RetryAfter = null;
                    }
                }
            }
        }

    }
}
