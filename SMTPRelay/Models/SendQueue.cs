using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Models
{
    //@"CREATE TABLE SendQueue (SendQueueID INTEGER PRIMARY KEY, EnvelopeID NOT NULL INTEGER, Recipient TEXT, State INTEGER, AttemptCount INTEGER, RetryAfter TEXT);",
    public class SendQueue
    {
        public long SendQueueID { get; set; }
        public long EnvelopeID { get; set; }
        public string Recipient { get; set; }
        public SendQueueStates State { get; set; }
        public long AttemptCount { get; set; }
        public DateTime RetryAfter { get; set; }
        /// <summary>
        /// Automatically adds this offset to the datetime('now') parameter if specified.
        /// </summary>
        public string RetryDelay { get; set; }

        public enum SendQueueStates
        {
            None = 0,
            Ready = 1,
            Busy = 2,
            SmartHostRejected = 3,
            SmartHostPasswordFail = 4
        }

        public long StateValue
        {
            get
            {
                return (long)State;
            }
            set
            {
                switch (value)
                {
                    case 1:
                        State = SendQueueStates.Ready;
                        break;
                    case 2:
                        State = SendQueueStates.Busy;
                        break;
                    case 3:
                        State = SendQueueStates.SmartHostRejected;
                        break;
                    case 4:
                        State = SendQueueStates.SmartHostPasswordFail;
                        break;
                    default:
                        State = SendQueueStates.None;
                        break;
                }
            }
        }


    }
}
