using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblDeliveryReport
    {
        //@"CREATE TABLE DeliveryReport (DeliveryReportID INTEGER PRIMARY KEY, WhenScheduled TEXT, WhenGenerated TEXT, OriginalEnvelopeRcptID INTEGER, EnvelopeID INTEGER, ReportType INTEGER, Reason TEXT, Status INTEGER);"

        /// <summary>
        /// Creates a new instance for an item that already exists in the database
        /// </summary>
        public tblDeliveryReport(long deliveryReportID, string whenScheduled, string whenGenerated, long originalEnvelopeRcptID, long? envelopeID, int reportType, string reason, int status)
        {
            DeliveryReportID = deliveryReportID;
            WhenScheduledStr = whenScheduled;
            WhenGeneratedStr = whenGenerated;
            OriginalEnvelopeRcptID = originalEnvelopeRcptID;
            EnvelopeID = envelopeID;
            ReportTypeInt = reportType;
            Reason = reason;
            StatusInt = status;
        }

        public long DeliveryReportID { get; set; }

        public DateTime? WhenScheduled { get; set; }

        public string WhenScheduledStr
        {
            get
            {
                if (WhenScheduled.HasValue)
                {
                    return WhenScheduled.Value.ToUniversalTime().ToString("O");
                }
                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    WhenScheduled = null;
                }
                else
                {
                    DateTime parse;
                    if (DateTime.TryParse(value, out parse))
                    {
                        WhenScheduled = parse;
                    }
                    else
                    {
                        WhenScheduled = null;
                    }
                }
            }
        }

        public DateTime? WhenGenerated { get; set; }

        public string WhenGeneratedStr
        {
            get
            {
                if (WhenGenerated.HasValue)
                {
                    return WhenGenerated.Value.ToUniversalTime().ToString("O");
                }
                return null;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    WhenGenerated = null;
                }
                else
                {
                    DateTime parse;
                    if (DateTime.TryParse(value, out parse))
                    {
                        WhenGenerated = parse;
                    }
                    else
                    {
                        WhenGenerated = null;
                    }
                }
            }
        }

        public long OriginalEnvelopeRcptID { get; set; }

        public long? EnvelopeID { get; set; }

        public ReportType ReportType { get; set; }

        public int ReportTypeInt
        {
            get
            {
                return (int)ReportType;
            }
            set
            {
                ReportType = (ReportType)value;
            }
        }

        public string Reason { get; set; }

        public QueueState Status { get; set; }

        public int StatusInt
        {
            get
            {
                return (int)Status;
            }
            set
            {
                Status = (QueueState)value;
            }
        }

    }
}
