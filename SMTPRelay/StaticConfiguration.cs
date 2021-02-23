using SMTPRelay.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay
{
    public static class StaticConfiguration
    {
        static StaticConfiguration()
        {
            EndPoints = new List<EndPoint>();
            SmartHost = new SmartHost();
        }

        public static void LoadSettings(string path = null)
        {
            if (path == null)
            {
                path = "settings.inf";
            }
            using (TextReader r = File.OpenText(path))
            {
                string str = r.ReadLine();
                if (str.StartsWith("HOSTNAME="))
                {
                    ThisHostName = str.Substring(9);
                }
                else
                {
                    throw new Exception("HOSTNAME= line expected on line 1");
                }
                str = r.ReadLine();
                if (str.StartsWith("SMARTHOSTURL="))
                {
                    SmartHost = new SmartHost();
                    SmartHost.EndPoint = new EndPoint();
                    SmartHost.EndPoint.Address = str.Substring(13);
                }
                else
                {
                    throw new Exception("SMARTHOSTURL= line expected on line 2");
                }
                str = r.ReadLine();
                if (str.StartsWith("SMARTHOSTPORT="))
                {
                    str = str.Substring(14);
                    SmartHost.EndPoint.Port = short.Parse(str); 
                }
                else
                {
                    throw new Exception("SMARTHOSTPORT= line expected on line 3");
                }
                str = r.ReadLine();
                if (str.StartsWith("USERNAME="))
                {
                    str = str.Substring(9);
                    SmartHost.Credentials = new SmtpCredentials();
                    SmartHost.Credentials.Username = str;
                }
                else
                {
                    throw new Exception("USERNAME= line expected on line 4");
                }
                str = r.ReadLine();
                if (str.StartsWith("PASSWORD="))
                {
                    str = str.Substring(9);
                    SmartHost.Credentials.Password = str;
                }
                else
                {
                    throw new Exception("PASSWORD= line expected on line 5");
                }
                str = r.ReadLine();
                if (str.StartsWith("SENDER="))
                {
                    str = str.Substring(7);
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        SenderOverride = null;
                    }
                    else
                    {
                        SenderOverride = str;
                    }
                    str = r.ReadLine();
                }
                int epCount = 0;
                if (str.StartsWith("ENDPOINTS="))
                {
                    str = str.Substring(10);
                    epCount = int.Parse(str);
                }
                else
                {
                    throw new Exception("ENDPOINTS= line expected on line 6");
                }

                int linenum = 6;
                if (!string.IsNullOrWhiteSpace(SenderOverride))
                {
                    linenum++;
                }

                for (int i = 0; i < epCount; ++i)
                {
                    EndPoint ep = new EndPoint();
                    str = r.ReadLine();
                    linenum++;
                    if (str.StartsWith(string.Format("ADDRESS{0}=", i)))
                    {
                        str = str.Substring(string.Format("ADDRESS{0}=", i).Length);
                        ep.Address = str;
                    }
                    else
                    {
                        throw new Exception(string.Format("ADDRESS{0}= line expected on line {1}", i, linenum));
                    }
                    str = r.ReadLine();
                    linenum++;
                    if (str.StartsWith(string.Format("PORT{0}=", i)))
                    {
                        str = str.Substring(string.Format("PORT{0}=", i).Length);
                        ep.Port = short.Parse(str);
                    }
                    else
                    {
                        throw new Exception(string.Format("PORT{0}= line expected on line {1}", i, linenum));
                    }
                    EndPoints.Add(ep);
                }

                str = r.ReadLine();
                if (!string.IsNullOrWhiteSpace(str) && str.StartsWith("DATABASE="))
                {
                    str = str.Substring(9);
                    DatabasePath = str;
                }
            }
        }
        // local IP end points (address and port) to listen for client connection on.
        public static List<EndPoint> EndPoints { get; private set; }
        // smart host to connect to (IP end point, credentials)
        public static SmartHost SmartHost { get; private set; }
        // server name to report to clients/servers
        public static string ThisHostName { get; private set; }
        /// <summary>
        /// The SMTP email address to send FROM. If this is blank, the original sender will be used as specified by the SMTP conversation
        /// </summary>
        public static string SenderOverride { get; private set; }
        /// <summary>
        /// How long max in milliseconds to wait for client to send a message. If client stops transmitting for this long in any state, the connection is reset.
        /// </summary>
        public const int MaxClientTimeoutms = 120000;
        /// <summary>
        /// how long max in milliseconds to wait for client to send MAIL TO: command. This only applies to idle connection state while waiting for MAIL TO: command.
        /// </summary>
        public const int MaxClientIdleTimeoutms = 30000;
        /// <summary>
        /// How long max to wait in milliseconds while attempting to connect to smart host. This has to be shorter than MaxClientTimeoutms or the converstaion will abort while waiting for smart host connection
        /// </summary>
        public const int MaxHostTimeoutms = 30000;

        public const int CheckOutboundQueueIntervalms = 15000;
        /// <summary>
        /// Location on the file system where the SQLite database is stored.
        /// </summary>
        public static string DatabasePath { get; private set; }

    }
}
