using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblDevice
    {

        //DeviceID INTEGER PRIMARY KEY, DisplayName TEXT, Address TEXT, Hostname TEXT, Enabled INTEGER NOT NULL, MailGatewayID INTEGER

        /// <summary>
        /// Creates a new instance that has not been saved in the database
        /// </summary>
        public tblDevice(string displayName, string address, string hostname, bool enabled, long? mailGateway)
        {
            DeviceID = null;
            DisplayName = displayName;
            Address = address;
            Hostname = hostname;
            Enabled = enabled;
            MailGateway = mailGateway;
        }

        /// <summary>
        /// Creates a new instance that exists in the database.
        /// </summary>
        public tblDevice(long deviceID, string displayName, string address, string hostname, int enabled, long? mailGateway)
        {
            DeviceID = deviceID;
            DisplayName = displayName;
            Address = address;
            Hostname = hostname;
            EnabledInt = enabled;
            MailGateway = mailGateway;
        }

        public long? DeviceID { get; set; }

        public string DisplayName { get; set; }

        public string Address { get; set; }

        public string Hostname { get; set; }

        public bool Enabled { get; set; }

        public int EnabledInt
        {
            get
            {
                return Enabled ? 1 : 0;
            }
            set
            {
                Enabled = value > 0;
            }
        }

        public long? MailGateway { get; set; }
    }
}
