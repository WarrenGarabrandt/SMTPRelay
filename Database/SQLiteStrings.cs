using System;
using System.Collections.Generic;

namespace SMTPRelay.Database
{
    public static class SQLiteStrings
    {
        private const string COMPATIBLE_DATABASE_VERSION = "1.2";
        public static string[] Format_Database = new string[]
        {
            // Contains configuration and version data.
            @"CREATE TABLE System (Category TEXT, Setting TEXT, Value TEXT);",

            // create unique constraint so that REPLACE INTO will function properly.
            @"CREATE UNIQUE INDEX idx_System_CategorySetting ON System(Category, Setting);",

            // User table
            @"CREATE TABLE User (UserID INTEGER PRIMARY KEY, DisplayName TEXT, Email TEXT, Salt TEXT NOT NULL, PassHash TEXT NOT NULL, Enabled INTEGER NOT NULL, Admin INTEGER NOT NULL, MailGatewayID INTEGER);",

            // Device table
            @"CREATE TABLE Device (DeviceID INTEGER PRIMARY KEY, DisplayName TEXT, Address TEXT, Hostname TEXT, Enabled INTEGER NOT NULL, MailGatewayID INTEGER);",

            // Mail Gateway Table
            @"CREATE TABLE MailGateway (MailGatewayID INTEGER PRIMARY KEY, SMTPServer TEXT NOT NULL, Port INTEGER NOT NULL, EnableSSL INTEGER NOT NULL, Authenticate INTEGER NOT NULL, Username TEXT, Password TEXT, SenderOverride TEXT);",

            // Basic email header info.
            @"CREATE TABLE Envelope (EnvelopeID INTEGER PRIMARY KEY, UserID INTEGER, DeviceID INTEGER, WhenReceived TEXT, Sender TEXT, Recipients TEXT, ChunkCount INTEGER, MsgID TEXT);",
            
            // Recipient email addresses
            @"CREATE TABLE Recipient (RecipientID INTEGER PRIMARY KEY, Address TEXT);",

            // Envelope Recipients
            @"CREATE TABLE EnvelopeRcpt(EnvelopeRcptID INTEGER PRIMARY KEY, EnvelopeID INTEGER, RecipientID INTEGER NOT NULL);",

            // Stores email body in chunks.
            @"CREATE TABLE MailChunk (EnvelopeID INTEGER NOT NULL, ChunkID INTEGER NOT NULL, Chunk BLOB);",

            // Queue of items to be transmitted. Items are removed as they are completed.
            // State: -1 = disable
            //         0 = Wait for RetryAfter Timer
            //         1 = Currently Running
            //         2 = Done. Ready for Cleanup.
            @"CREATE TABLE SendQueue (SendQueueID INTEGER PRIMARY KEY, EnvelopeID INTEGER NOT NULL, EnvelopeRcptID INTEGER NOT NULL, State INTEGER NOT NULL, AttemptCount INTEGER NOT NULL, RetryAfter TEXT);",
            
            // Process log. Each attempt to process an email will result in a row being generated with the result of that attempt
            @"CREATE TABLE SendLog (EnvelopeID INTEGER NOT NULL, EnvelopeRcptID INTEGER NOT NULL, WhenAttempted TEXT, Results TEXT, AttemptCount INTEGER, Successful INTEGER);",

            // IP Endpoint to listen for incoming connections.
            // Protocol: list the Protocol that this endpoint will allow. Other protocols may be implemented in the future.
            //          SMTP: Basic SMTP only EHLO rejected.
            //          ESMTP: Extended SMTP, (backwards compatible to SMTP)
            // TLSMode: 0 = No TLS available. Reject StartTLS attempts.
            //          1 = TLS available if client asks for it.
            //          2 = TLS Required. Client MUST Issue StartTLS before Auth
            @"CREATE TABLE IPEndpoint (IPEndpointID INTEGER PRIMARY KEY, Address TEXT, Port INTEGER, Protocol TEXT, TLSMode INTEGER, Hostname TEXT, CertFriendlyName TEXT);",

            // Delivery Reports
            // Generated when an email can't be sent to a recipient. It send the message to the original sender as well as the admin contact.
            // A new envelope is generated when this NDR is processed, then the email gets sent out via the standard process.
            // ReportType: 0 - Notice of successful relay
            //             1 - Warning of message delay
            //             2 - Permanent failure to relay.
            // Status: 0 - Ready to generate a delivery report
            //         1 - Delivery report picked up
            //         2 - Delivery report generated.
            //        -1 - Something went wrong and the delivery report failed to generate.
            @"CREATE TABLE DeliveryReport (DeliveryReportID INTEGER PRIMARY KEY, WhenScheduled TEXT, WhenGenerated TEXT, OriginalEnvelopeRcptID INTEGER, EnvelopeID INTEGER, ReportType INTEGER, Reason TEXT, Status INTEGER);",
        };

        public static List<Tuple<string, string, string>> DatabaseDefaults = new List<Tuple<string, string, string>>()
        {
            // current database version
            new Tuple<string, string, string>("System", "Version", COMPATIBLE_DATABASE_VERSION),
            // max message length = 30 MB
            new Tuple<string, string, string>("Message", "MaxLength", "31457280"),
            // max message recipients = 100
            new Tuple<string, string, string>("Message", "MaxRecipients", "100"),
            // max chunk size = 64 KB
            new Tuple<string, string, string>("Message", "ChunkSize", "65536"),
            // How long to retain data after all recipients have been sent, in minutes. (default 3 days = 4320 minutes)
            new Tuple<string, string, string>("Message", "DataRetainMins", "4320"),
            // How long to retain failed items before diving up and deleting them entirely. (default 30 days = 43200 minutes)
            new Tuple<string, string, string>("Message", "PurgeFailedMins", "43200"),
            // When the last data purge was run.
            new Tuple<string, string, string>("Purge", "LastRun", ""),
            // How often to perform the purge, in minutes
            new Tuple<string, string, string>("Purge", "FrequencyMins", "360"),
            // SMTP server Host Name it advertises.
            new Tuple<string, string, string>("SMTPServer", "Hostname", "mailrelay.local"),
            // Client has 15 seconds to send HELO or EHLO or we abort the connection.
            new Tuple<string, string, string>("SMTPServer", "ConnectionTimeoutMS", "15000"),
            // A connection can stay idle for up to 2 minutes without MAIL being at least started, or after a MAIL successfully processes. After that, we close even if they are still there.
            new Tuple<string, string, string>("SMTPServer", "CommandTimeoutMS", "120000"),
            // Max number of bad commands before the connection is aborted = 10
            new Tuple<string, string, string>("SMTPServer", "BadCommandLimit", "10"),
            // For SMTPSenderQueue, Refresh interval in milliseconds.
            new Tuple<string, string, string>("SMTPSenderQueue", "RefreshMS", "10000")
        };

        public static string Table_LastRowID = @"SELECT last_insert_rowid();";

        public static string System_GetAll = @"SELECT Category, Setting, Value FROM System ORDER BY Category ASC, Setting ASC;";
        public static string System_Select = @"SELECT Value FROM System WHERE Category = $Category AND Setting = $Setting;";
        public static string System_Insert = @"REPLACE INTO System(Category, Setting, Value) VALUES ($Category, $Setting, $Value);";
        
        public static string User_GetAll = @"SELECT UserID, DisplayName, Email, Salt, PassHash, Enabled, Admin, MailGatewayID FROM User;";
        public static string User_GetByEmail = @"SELECT UserID, DisplayName, Email, Salt, PassHash, Enabled, Admin, MailGatewayID FROM User WHERE Email = $Email COLLATE NOCASE;";
        public static string User_GetByID = @"SELECT UserID, DisplayName, Email, Salt, PassHash, Enabled, Admin, MailGatewayID FROM User WHERE UserID = $UserID;";
        public static string User_Insert = @"INSERT INTO User(DisplayName, Email, Salt, PassHash, Enabled, Admin, MailGatewayID) VALUES ($DisplayName, $Email, $Salt, $PassHash, $Enabled, $Admin, $MailGatewayID);";
        public static string User_Update = @"UPDATE User SET DisplayName = $DisplayName, Email = $Email, Salt = $Salt, PassHash = $PassHash, Enabled = $Enabled, Admin = $Admin, MailGatewayID = $MailGatewayID WHERE UserID = $UserID;";
        public static string User_ClearGatewayByID = @"UPDATE User SET MailGatewayID = NULL WHERE MailGatewayID = $MailGatewayID;";
        public static string User_DeleteByID = @"DELETE FROM User WHERE UserID = $UserID;";

        public static string Device_GetAll = @"SELECT DeviceID, DisplayName, Address, Hostname, Enabled, MailGatewayID FROM Device;";
        public static string Device_GetByID = @"SELECT DeviceID, DisplayName, Address, Hostname, Enabled, MailGatewayID FROM Device WHERE DeviceID = $DeviceID;";
        public static string Device_GetByAddress = @"SELECT DeviceID, DisplayName, Address, Hostname, Enabled, MailGatewayID FROM Device WHERE Address = $Address AND Enabled = 1;";
        public static string Device_Insert = @"INSERT INTO Device (DisplayName, Address, Hostname, Enabled, MailGatewayID) VALUES ($DisplayName, $Address, $Hostname, $Enabled, $MailGatewayID);";
        public static string Device_Update = @"UPDATE Device SET DisplayName = $DisplayName, Address = $Address, Hostname = $Hostname, Enabled = $Enabled, MailGatewayID = $MailGatewayID WHERE DeviceID = $DeviceID;";
        public static string Device_ClearGatewayByID = @"UPDATE Device SET MailGatewayID = NULL WHERE MailGatewayID = $MailGatewayID;";
        public static string Device_DeleteByID = @"DELETE FROM Device WHERE DeviceID = $DeviceID;";

        public static string Envelope_GetAll = @"SELECT EnvelopeID, UserID, DeviceID, WhenReceived, Sender, Recipients, ChunkCount, MsgID FROM Envelope;";
        public static string Envelope_GetByID = @"SELECT EnvelopeID, UserID, DeviceID, WhenReceived, Sender, Recipients, ChunkCount, MsgID FROM Envelope WHERE EnvelopeID = $EnvelopeID;";
        public static string Envelope_Insert = @"INSERT INTO Envelope(UserID, DeviceID, WhenReceived, Sender, Recipients, ChunkCount, MsgID) VALUES ($UserID, $DeviceID, $WhenReceived, $Sender, $Recipients, $ChunkCount, $MsgID);";
        public static string Envelope_UpdateChunkCount = @"UPDATE Envelope SET ChunkCount = $ChunkCount WHERE EnvelopeID = $EnvelopeID;";

        public static string MailGateway_GetAll = @"SELECT MailGatewayID, SMTPServer, Port, EnableSSL, Authenticate, Username, Password, SenderOverride FROM MailGateway;";
        public static string MailGateway_GetByID = @"SELECT MailGatewayID, SMTPServer, Port, EnableSSL, Authenticate, Username, Password, SenderOverride FROM MailGateway WHERE MailGatewayID = $MailGatewayID;";
        public static string MailGateway_Insert = @"INSERT INTO MailGateway (SMTPServer, Port, EnableSSL, Authenticate, Username, Password, SenderOverride) VALUES ($SMTPServer, $Port, $EnableSSL, $Authenticate, $Username, $Password, $SenderOverride);";
        public static string MailGateway_Update = @"UPDATE MailGateway SET SMTPServer = $SMTPServer, Port = $Port, EnableSSL = $EnableSSL, Authenticate = $Authenticate, Username = $Username, Password = $Password, SenderOverride = $SenderOverride WHERE MailGatewayID = $MailGatewayID;";
        public static string MailGateway_DeleteByID = @"DELETE FROM MailGateway WHERE MailGatewayID = $MailGatewayID;";

        public static string MailChunk_GetChunk = @"SELECT LENGTH(Chunk), Chunk FROM MailChunk WHERE EnvelopeID = $EnvelopeID AND ChunkID = $ChunkID;";
        public static string MailChunk_AddChunk = @"INSERT INTO MailChunk (EnvelopeID, ChunkID, Chunk) VALUES ($EnvelopeID, $ChunkID, $Chunk);";
        public static string MailChunk_DeleteMailData = @"DELETE FROM MailChunk WHERE EnvelopeID = $EnvelopeID;";
        public static string MailChunk_GetMailSize = @"SELECT SUM(LENGTH(Chunk)) FROM MailChunk WHERE EnvelopeID = $EnvelopeID;";

        public static string SendQueue_GetAll = @"SELECT SendQueueID, EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter FROM SendQueue;";
        public static string SendQueue_GetReady = @"SELECT SendQueueID, EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter FROM SendQueue WHERE RetryAfter < $RetryAfter AND SendQueue.State = 0 ORDER BY RetryAfter;";
        public static string SendQueue_GetBusy = @"SELECT SendQueueID, EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter FROM SendQueue WHERE SendQueue.State = 1;";
        public static string SendQueue_GetByID = @"SELECT SendQueueID, EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter FROM SendQueue WHERE SendQueueID = $SendQueueID;";
        public static string SendQueue_Insert = @"INSERT INTO SendQueue(EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter) VALUES ($EnvelopeID, $EnvelopeRcptID, $State, $AttemptCount, $RetryAfter);";
        public static string SendQueue_Update = @"UPDATE SendQueue SET State = $State, AttemptCount = $AttemptCount, RetryAfter = $RetryAfter WHERE SendQueueID = $SendQueueID;";
        public static string SendQueue_DeleteByID = @"DELETE FROM SendQueue WHERE SendQueueID = $SendQueueID";

        public static string SendLog_GetPage = @"SELCT EnvelopeID, EnvelopeRcptID, WhenAttempted, Results, AttemptCount, Successful FROM SendLog LIMIT $RowCount OFFSET $RowStart ORDER BY WhenAttempted DESC;";
        public static string SendLog_Insert = @"INSERT INTO SendLog (EnvelopeID, EnvelopeRcptID, WhenAttempted, Results, AttemptCount, Successful) VALUES ($EnvelopeID, $EnvelopeRcptID, $WhenAttempted, $Results, $AttemptCount, $Successful);";

        public static string Recipient_GetByAddress = @"SELECT RecipientID FROM Recipient WHERE Address = $Address;";
        public static string Recipeint_Insert = @"INSERT INTO Recipient (Address) VALUES ($Address);";

        public static string EnvelopeRcpt_GetByID = @"SELECT EnvelopeRcpt.EnvelopeRcptID, EnvelopeRcpt.EnvelopeID, Recipient.Address FROM EnvelopeRcpt INNER JOIN Recipient ON EnvelopeRcpt.RecipientID = Recipient.RecipientID WHERE EnvelopeRcpt.EnvelopeRcptID = $EnvelopeRcptID;";
        public static string EnvelopeRcpt_GetByEnvelopeID = "SELECT EnvelopeRcpt.EnvelopeRcptID, EnvelopeRcpt.EnvelopeID, Recipient.Address FROM EnvelopeRcpt INNER JOIN Recipient ON EnvelopeRcpt.RecipientID = Recipient.RecipientID WHERE EnvelopeRcpt.EnvelopeID = $EnvelopeID;";
        public static string EnvelopeRcpt_Insert = @"INSERT INTO EnvelopeRcpt (EnvelopeID, RecipientID) VALUES ($EnvelopeID, $RecipientID);";

        public static string IPEndpoint_GetAll = @"SELECT IPEndpointID, Address, Port, Protocol, TLSMode, Hostname, CertFriendlyName FROM IPEndpoint;";
        public static string IPEndpoint_GetByID = @"SELECT IPEndpointID, Address, Port, Protocol, TLSMode, Hostname, CertFriendlyName FROM IPEndpoint WHERE IPEndpointID = $IPEndpointID;";
        public static string IPEndpoint_Insert = @"INSERT INTO IPEndpoint (Address, Port, Protocol, TLSMode, Hostname, CertFriendlyName) VALUES ($Address, $Port, $Protocol, $TLSMode, $Hostname, $CertFriendlyName);";
        public static string IPEndpoint_Update = @"UPDATE IPEndpoint SET Address = $Address, Port = $Port, Protocol = $Protocol, TLSMode = $TLSMode, Hostname = $Hostname, CertFriendlyName = $CertFriendlyName WHERE IPEndpointID = $IPEndpointID;";
        public static string IPEndpoint_DeleteByID = @"DELETE FROM IPEndpoint WHERE IPEndpointID = $IPEndpointID;";

        //@"CREATE TABLE DeliveryReport (DeliveryReportID INTEGER PRIMARY KEY, WhenScheduled TEXT, WhenGenerated TEXT, OriginalEnvelopeRcptID INTEGER, EnvelopeID INTEGER, ReportType INTEGER, Reason TEXT, Status INTEGER);"
        public static string DeliveryReport_GetReady = @"SELECT DeliveryReportID, WhenScheduled, WhenGenerated, OriginalEnvelopeRcptID, EnvelopeID, ReportType, Reason, Status FROM DeliveryReport WHERE Status = 0";
        public static string DeliveryReport_InsertEnque = @"INSERT INTO DeliveryReport (WhenScheduled, WhenGenerated, OriginalEnvelopeRcptID, EnvelopeID, ReportType, Reason, Status) VALUES ($WhenScheduled, NULL, $OriginalEnvelopeRcptID, NULL, $ReportType, $Reason, 0);";
        public static string DeliveryReport_MarkRunning = @"UPDATE DeliveryReport SET Status = 1 WHERE DeliveryReportID = $DeliveryReportID;";
        public static string DeliveryReport_UpdateDone = @"UPDATE DeliveryReport SET WhenGenerated = $WhenGenerated, EnvelopeID = $EnvelopeID, Status = $Status WHERE DeliveryReportID = $DeliveryReportID;";

        public static string vwMailQueue_GetQueue = @"SELECT Envelope.Sender, Envelope.Recipients, Envelope.WhenReceived, SendQueue.RetryAfter, SendQueue.AttemptCount FROM SendQueue INNER JOIN Envelope ON SendQueue.EnvelopeID = Envelope.EnvelopeID WHERE SendQueue.State = 0 ORDER BY SendQueue.RetryAfter ASC;";

    }
}
