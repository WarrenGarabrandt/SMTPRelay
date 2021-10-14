using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblIPEndpoint
    {
        //"CREATE TABLE IPEndpoint (IPEndpointID INTEGER PRIMARY KEY, Address TEXT, Port INTEGER, Protocol TEXT, TLSMode INTEGER, CertFriendlyName TEXT);"

        /// <summary>
        /// Creates new instance that has not been saved into the database yet.
        /// </summary>
        public tblIPEndpoint(string address, int port, IPEndpointProtocols protocol, IPEndpointTLSModes tlsMode, string hostname, string certFriendlyName)
        {
            IPEndpointID = null;
            Address = address;
            Port = port;
            Protocol = protocol;
            TLSMode = tlsMode;
            Hostname = hostname;
            CertFriendlyName = certFriendlyName;
        }

        /// <summary>
        /// Creates a new instance that exists in the database.
        /// </summary>
        public tblIPEndpoint(long ipendpointID, string address, int port, string protocol, int tlsMode, string hostname, string certFriendlyName)
        {
            IPEndpointID = ipendpointID;
            Address = address;
            Port = port;
            ProtocolString = protocol;
            TLSModeInt = tlsMode;
            Hostname = hostname;
            CertFriendlyName = certFriendlyName;
        }

        public enum IPEndpointProtocols
        {
            None,
            SMTP,
            ESMTP
        }

        public enum IPEndpointTLSModes
        {
            Disabled,
            Enabled,
            Enforced
        }

        public long? IPEndpointID { get; set; }

        public string Address { get; set; }

        public int Port { get; set; }

        public IPEndPoint IPEndPoint
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(Address) && Port >= 1 && Port <= 65535)
                    {
                        IPAddress ipaddr;
                        if (Address == "0.0.0.0")
                        {
                            return new IPEndPoint(IPAddress.Any, Port);
                        }
                        else if (Address == "::" || Address == "::/0" || Address == "::/128")
                        {
                            return new IPEndPoint(IPAddress.IPv6Any, Port);
                        }
                        else if (Address == "127.0.0.1")
                        {
                            return new IPEndPoint(IPAddress.Loopback, Port);
                        }
                        else if (Address == "::1")
                        {
                            return new IPEndPoint(IPAddress.IPv6Loopback, Port);
                        }
                        else if (IPAddress.TryParse(Address, out ipaddr))
                        {
                            return new IPEndPoint(ipaddr, Port);
                        }
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    Address = value.Address.ToString();
                    Port = value.Port;
                }
                else
                {
                    Address = "";
                    Port = -1;
                }
            }
        }

        public string ProtocolString
        {
            get
            {
                switch (Protocol)
                {
                    case IPEndpointProtocols.SMTP:
                        return "SMTP";
                    case IPEndpointProtocols.ESMTP:
                        return "ESMTP";
                    default:
                        return "";
                }
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Protocol = IPEndpointProtocols.None;
                }
                else if (value.ToUpper() == "SMTP")
                {
                    Protocol = IPEndpointProtocols.SMTP;
                }
                else if (value.ToUpper() == "ESMTP")
                {
                    Protocol = IPEndpointProtocols.ESMTP;
                }
                else
                {
                    Protocol = IPEndpointProtocols.None;
                }
            }
        }

        public IPEndpointProtocols Protocol { get; set; }

        public int TLSModeInt
        {
            get
            {
                switch (TLSMode)
                {
                    case IPEndpointTLSModes.Disabled:
                        return 0;
                    case IPEndpointTLSModes.Enabled:
                        return 1;
                    case IPEndpointTLSModes.Enforced:
                        return 2;
                    default:
                        return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 0:
                        TLSMode = IPEndpointTLSModes.Disabled;
                        break;
                    case 1:
                        TLSMode = IPEndpointTLSModes.Enabled;
                        break;
                    case 2:
                        TLSMode = IPEndpointTLSModes.Enforced;
                        break;
                    default:
                        TLSMode = IPEndpointTLSModes.Disabled;
                        break;
                }
            }
        }

        public string TLSModeString
        {
            get
            {
                switch (TLSMode)
                {
                    case IPEndpointTLSModes.Disabled:
                        return "Disabled";
                    case IPEndpointTLSModes.Enabled:
                        return "Enabled";
                    case IPEndpointTLSModes.Enforced:
                        return "Enforced";
                    default:
                        return "Disabled";
                }
            }
        }

        public IPEndpointTLSModes TLSMode { get; set; }

        public string Hostname { get; set; }

        public string CertFriendlyName { get; set; }
    }
}
