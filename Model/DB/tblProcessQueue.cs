using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblProcessQueue
    {
        
        /// <summary>
        /// Creates a new instance for an item that already exists in the database.
        /// </summary>
        public tblProcessQueue(long processQueueID, long envelopeID, long envelopeRcptID, int state, int attemptCount, string retryAfter)
        {
            ProcessQueueID = processQueueID;
            EnvelopeID = envelopeID;
            EnvelopeRcptID = envelopeRcptID;
            StateInt = state;
            AttemptCount = attemptCount;
            RetryAfterStr = retryAfter;
        }

        /// <summary>
        /// Creates a new instance for an item that does not yet exist in the databse.
        /// </summary>
        public tblProcessQueue(long envelopeID, long envelopeRcptID, QueueState state, int attemptCount, DateTime? retryAfter)
        {
            ProcessQueueID = null;
            EnvelopeID = envelopeID;
            EnvelopeRcptID = envelopeRcptID;
            State = state;
            AttemptCount = attemptCount;
            RetryAfter = retryAfter;
        }

        public long? ProcessQueueID { get; set; }

        // private set because there isn't a SQL UPDATE statement for this field.
        public long EnvelopeID { get; private set; }

        // private set because there isn't a SQL UPDATE statement for this field.
        public long EnvelopeRcptID { get; private set; }

        public QueueState State { get; set; }

        public int StateInt
        {
            get
            {
                return (int)State;
            }
            set
            {
                State = (QueueState)value;
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
