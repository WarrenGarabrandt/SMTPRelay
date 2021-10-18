using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblSendLog
    {
        
        /// <summary>
        /// Creates a new instance for an item that already exists in the database.
        /// </summary>
        public tblSendLog(long envelopeID, long envelopeRcptID, string whenAttempted, string results, int attemptCount, int successful)
        {
            EnvelopeID = envelopeID;
            EnvelopeRcptID = envelopeRcptID;
            WhenAttemptedStr = whenAttempted;
            Results = results;
            AttemptCount = attemptCount;
            SuccessfulInt = successful;
        }

        /// <summary>
        /// Creates a new instance for an item that doesn't yet exist in the database.
        /// </summary>
        public tblSendLog(long envelopeID, long envelopeRcptID, DateTime whenAttempted, string results, int attemptCount, bool successful)
        {
            EnvelopeID = envelopeID;
            EnvelopeRcptID = envelopeRcptID;
            WhenAttempted = whenAttempted;
            Results = results;
            AttemptCount = attemptCount;
            Successful = successful;
        }

        public long EnvelopeID { get; set; }
        
        public long EnvelopeRcptID { get; set; }

        public DateTime WhenAttempted { get; set; }

        public string WhenAttemptedStr
        {
            get
            {
                return WhenAttempted.ToUniversalTime().ToString("O");
            }
            set
            {
                WhenAttempted = DateTime.Parse(value);
            }
        }

        public string Results { get; set; }

        public int AttemptCount { get; set; }

        public bool Successful { get; set; }

        public int SuccessfulInt
        {
            get
            {
                return Successful ? 1 : 0;
            }
            set
            {
                Successful = value > 0;
            }
        }
    }
}
