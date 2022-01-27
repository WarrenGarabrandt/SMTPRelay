using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblMailGateway
    {
        //"CREATE TABLE MailGateway (MailRouteID INTEGER PRIMARY KEY, SMTPServer TEXT NOT NULL, Port INTEGER NOT NULL, EnableSSL INTEGER NOT NULL, Authenticate INTEGER NOT NULL, Username TEXT, Password TEXT, SenderOverride TEXT);",
        /// <summary>
        /// Creates a new object that doesn't exist in the database yet.
        /// </summary>
        public tblMailGateway(string smtpServer, int port, bool enableSSL, bool authenticate, string username, string password, string senderOverride, int? connectionLimit)
        {
            MailGatewayID = null;
            SMTPServer = smtpServer;
            Port = port;
            EnableSSL = enableSSL;
            Authenticate = authenticate;
            Username = username;
            Password = password;
            SenderOverride = senderOverride;
            ConnectionLimit = connectionLimit;
        }

        /// <summary>
        /// Creates a new object that already exists in the database.
        /// </summary>
        public tblMailGateway(long mailRouteID, string smtpServer, int port, int enableSSLint, int authenticateInt, string username, string password, string senderOverride, int? connectionLimit)
        {
            MailGatewayID = mailRouteID;
            SMTPServer = smtpServer;
            Port = port;
            EnableSSLInt = enableSSLint;
            AuthenticateInt = authenticateInt;
            Username = username;
            Password = password;
            SenderOverride = senderOverride;
            ConnectionLimit = connectionLimit;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(SMTPServer) && Port == -1)
            {
                return string.Format("<unassigned>");
            }
            if (Authenticate)
            {
                if (string.IsNullOrEmpty(SenderOverride))
                {
                    return string.Format("({0}) {1} on {2}:{3}", MailGatewayID.HasValue ? MailGatewayID.Value : -1, Username, SMTPServer, Port);
                }
                else
                {
                    return string.Format("({0}) {1} on {2}:{3} as {4}", MailGatewayID.HasValue ? MailGatewayID.Value : -1, Username, SMTPServer, Port, SenderOverride);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(SenderOverride))
                {
                    return string.Format("({0}) {1}:{2}", MailGatewayID.HasValue ? MailGatewayID.Value : -1, SMTPServer, Port);
                }
                else
                {
                    return string.Format("({0}) {1}:{2} as {3}", MailGatewayID.HasValue ? MailGatewayID.Value : -1, SMTPServer, Port, SenderOverride);
                }
            }
        }
        public long? MailGatewayID { get; set; }
        public string SMTPServer { get; set; }
        public bool EnableSSL { get; set; }
        public int EnableSSLInt
        {
            get
            {
                return EnableSSL ? 1 : 0;
            }
            set
            {
                EnableSSL = value > 0;
            }
        }
        public int Port { get; set; }
        public bool Authenticate { get; set; }
        public int AuthenticateInt
        {
            get
            {
                return Authenticate ? 1 : 0;
            }
            set
            {
                Authenticate = value > 0;
            }
        }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderOverride { get; set; }
        public int? ConnectionLimit { get; set; }

    }
}
