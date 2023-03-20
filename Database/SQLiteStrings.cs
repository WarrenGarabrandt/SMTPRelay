using System;
using System.Collections.Generic;

namespace SMTPRelay.Database
{
    public static class SQLiteStrings
    {
        private const string COMPATIBLE_DATABASE_VERSION = "1.7";
        public static string[] Format_Database = new string[]
        {
            // Contains configuration and version data.
            @"CREATE TABLE System (Category TEXT, Setting TEXT, Value TEXT);",

            // create unique constraint so that REPLACE INTO will function properly.
            @"CREATE UNIQUE INDEX idx_System_CategorySetting ON System(Category, Setting);",

            // User table
            @"CREATE TABLE User (UserID INTEGER PRIMARY KEY, DisplayName TEXT, Email TEXT, Salt TEXT NOT NULL, PassHash TEXT NOT NULL, Enabled INTEGER NOT NULL, Admin INTEGER NOT NULL, Maildrop INTEGER NOT NULL, MailGatewayID INTEGER);",

            // Device table
            @"CREATE TABLE Device (DeviceID INTEGER PRIMARY KEY, DisplayName TEXT, Address TEXT, Hostname TEXT, Enabled INTEGER NOT NULL, MailGatewayID INTEGER);",

            // Mail Gateway Table
            @"CREATE TABLE MailGateway (MailGatewayID INTEGER PRIMARY KEY, SMTPServer TEXT NOT NULL, Port INTEGER NOT NULL, EnableSSL INTEGER NOT NULL, Authenticate INTEGER NOT NULL, Username TEXT, Password TEXT, SenderOverride TEXT, ConnectionLimit INTEGER);",

            // Basic email header info.
            @"CREATE TABLE Envelope (EnvelopeID INTEGER PRIMARY KEY, UserID INTEGER, DeviceID INTEGER, WhenReceived TEXT, Sender TEXT, Recipients TEXT, ChunkCount INTEGER, MsgID TEXT);",
            
            // Recipient email addresses
            @"CREATE TABLE Recipient (RecipientID INTEGER PRIMARY KEY, Address TEXT);",

            // Envelope Recipients
            @"CREATE TABLE EnvelopeRcpt(EnvelopeRcptID INTEGER PRIMARY KEY, EnvelopeID INTEGER, RecipientID INTEGER NOT NULL);",

            // Stores email body in chunks.
            @"CREATE TABLE MailChunk (EnvelopeID INTEGER NOT NULL, ChunkID INTEGER NOT NULL, Chunk BLOB);",

            // Stores a Mail Item received from a MailDrop enabled endpoint. 
            @"CREATE TABLE MailItem (MailItemID INTEGER PRIMARY KEY, UserID INTEGER NOT NULL, EnvelopeID INTEGER, Unread INTEGER NOT NULL);",

            // Queue of items to be transmitted. Items are removed as they are completed.
            // State: -1 = disable
            //         0 = Wait for RetryAfter Timer
            //         1 = Currently Running
            //         2 = Done. Ready for Cleanup.
            @"CREATE TABLE ProcessQueue (ProcessQueueID INTEGER PRIMARY KEY, EnvelopeID INTEGER NOT NULL, EnvelopeRcptID INTEGER NOT NULL, State INTEGER NOT NULL, AttemptCount INTEGER NOT NULL, RetryAfter TEXT);",
            
            // Process log. Each attempt to process an email will result in a row being generated with the result of that attempt
            @"CREATE TABLE SendLog (EnvelopeID INTEGER NOT NULL, EnvelopeRcptID INTEGER NOT NULL, WhenAttempted TEXT, Results TEXT, AttemptCount INTEGER, Successful INTEGER);",

            // IP Endpoint to listen for incoming connections.
            // Protocol: list the Protocol that this endpoint will allow. Other protocols may be implemented in the future.
            //          SMTP: Basic SMTP only EHLO rejected.
            //          ESMTP: Extended SMTP, (backwards compatible to SMTP)
            // TLSMode: 0 = No TLS available. Reject StartTLS attempts.
            //          1 = TLS available if client asks for it.
            //          2 = TLS Required. Client MUST Issue StartTLS before Auth
            // Maildrop: 0 = Not a maildrop, requires authentication
            //           1 = Is a maildrop, no authentication required. A maildrop user must be a recipient to accept mail.
            @"CREATE TABLE IPEndpoint (IPEndpointID INTEGER PRIMARY KEY, Address TEXT, Port INTEGER, Protocol TEXT, TLSMode INTEGER, Hostname TEXT, CertFriendlyName TEXT, Maildrop INTEGER NOT NULL);",

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
            // How many items to process in a purge batch.
            new Tuple<string, string, string>("Purge", "BatchSize", "100"),
            // 0 - disables debug logging. 1 - creates a text log with query details for troubleshooting.
            new Tuple<string, string, string>("Purge", "DebugLog", "0"),
            // Path for debug logs to be saved
            new Tuple<string, string, string>("Purge", "DebugLogPath", "C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Purge\\"),
            // Client has 15 seconds to send HELO or EHLO or we abort the connection.
            new Tuple<string, string, string>("SMTPServer", "ConnectionTimeoutMS", "15000"),
            // A connection can stay idle for up to 2 minutes without MAIL being at least started, or after a MAIL successfully processes. After that, we close even if they are still there.
            new Tuple<string, string, string>("SMTPServer", "CommandTimeoutMS", "120000"),
            // Max number of bad commands before the connection is aborted = 10
            new Tuple<string, string, string>("SMTPServer", "BadCommandLimit", "10"),
            // SMTP Server Verbose Debugging
            new Tuple<string, string, string>("SMTPServer", "VerboseDebuggingEnabled", "0"),
            // SMTP Server Verbose Debugging Path
            new Tuple<string, string, string>("SMTPServer", "VerboseDebuggingPath", "C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Receive\\"),
            // SMTP Server Verbose Debugging Include Mail Body
            new Tuple<string, string, string>("SMTPServer", "VerboseDebuggingIncludeBody", "0"),
            // Hostname used when sending mail
            new Tuple<string, string, string>("SMTPSender", "Hostname", "mailrelay.local"),
            // SMTP Client Verbose Debugging
            new Tuple<string, string, string>("SMTPSender", "VerboseDebuggingEnabled", "0"),
            // SMTP Client Verbose Debugging Path
            new Tuple<string, string, string>("SMTPSender", "VerboseDebuggingPath", "C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Send\\"),
            // SMTP Client Verbose Debugging Include Mail Body
            new Tuple<string, string, string>("SMTPSender", "VerboseDebuggingIncludeBody", "0"),
            // For SMTPSenderQueue, Refresh interval in milliseconds.
            new Tuple<string, string, string>("SMTPSender", "QueueRefreshMS", "10000"),
            
        };

        public static string Table_LastRowID = @"SELECT last_insert_rowid();";

        public static string System_GetAll = @"SELECT Category, Setting, Value FROM System ORDER BY Category ASC, Setting ASC;";
        public static string System_Select = @"SELECT Value FROM System WHERE Category = $Category AND Setting = $Setting;";
        public static string System_Insert = @"REPLACE INTO System(Category, Setting, Value) VALUES ($Category, $Setting, $Value);";
        
        public static string User_GetAll = @"SELECT UserID, DisplayName, Email, Salt, PassHash, Enabled, Admin, Maildrop, MailGatewayID FROM User;";
        public static string User_GetByEmail = @"SELECT UserID, DisplayName, Email, Salt, PassHash, Enabled, Admin, Maildrop, MailGatewayID FROM User WHERE Email = $Email COLLATE NOCASE;";
        public static string User_GetByID = @"SELECT UserID, DisplayName, Email, Salt, PassHash, Enabled, Admin, Maildrop, MailGatewayID FROM User WHERE UserID = $UserID;";
        public static string User_Insert = @"INSERT INTO User(DisplayName, Email, Salt, PassHash, Enabled, Admin, Maildrop, MailGatewayID) VALUES ($DisplayName, $Email, $Salt, $PassHash, $Enabled, $Admin, $Maildrop, $MailGatewayID);";
        public static string User_Update = @"UPDATE User SET DisplayName = $DisplayName, Email = $Email, Salt = $Salt, PassHash = $PassHash, Enabled = $Enabled, Admin = $Admin, Maildrop = $Maildrop, MailGatewayID = $MailGatewayID WHERE UserID = $UserID;";
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
        public static string Envelope_DeleteByID = @"DELETE FROM Envelope WHERE EnvelopeID = $EnvelopeID;";
        public static string Envelope_GetAllOld = @"SELECT EnvelopeID FROM Envelope WHERE (SELECT COUNT(*) FROM ProcessQueue WHERE ProcessQueue.EnvelopeID = Envelope.EnvelopeID AND (ProcessQueue.State = 0 OR ProcessQueue.State = 1)) = 0 AND " +
            "(SELECT COUNT(*) FROM ProcessQueue WHERE ProcessQueue.EnvelopeID = Envelope.EnvelopeID AND ProcessQueue.State = -1 AND ProcessQueue.RetryAfter > $FailedCutoff) = 0 AND " + 
            "(SELECT COUNT(*) FROM ProcessQueue WHERE ProcessQueue.EnvelopeID = Envelope.EnvelopeID AND ProcessQueue.State = 2 AND ProcessQueue.RetryAfter > $CompleteCutoff) = 0 AND " +
            "(SELECT COUNT(*) FROM MailItem WHERE MailItem.EnvelopeID = Envelope.EnvelopeID) = 0 LIMIT $LimitValue;";

        public static string MailGateway_GetAll = @"SELECT MailGatewayID, SMTPServer, Port, EnableSSL, Authenticate, Username, Password, SenderOverride, ConnectionLimit FROM MailGateway;";
        public static string MailGateway_GetByID = @"SELECT MailGatewayID, SMTPServer, Port, EnableSSL, Authenticate, Username, Password, SenderOverride, ConnectionLimit FROM MailGateway WHERE MailGatewayID = $MailGatewayID;";
        public static string MailGateway_Insert = @"INSERT INTO MailGateway (SMTPServer, Port, EnableSSL, Authenticate, Username, Password, SenderOverride, ConnectionLimit) VALUES ($SMTPServer, $Port, $EnableSSL, $Authenticate, $Username, $Password, $SenderOverride, $ConnectionLimit);";
        public static string MailGateway_Update = @"UPDATE MailGateway SET SMTPServer = $SMTPServer, Port = $Port, EnableSSL = $EnableSSL, Authenticate = $Authenticate, Username = $Username, Password = $Password, SenderOverride = $SenderOverride, ConnectionLimit = $ConnectionLimit WHERE MailGatewayID = $MailGatewayID;";
        public static string MailGateway_DeleteByID = @"DELETE FROM MailGateway WHERE MailGatewayID = $MailGatewayID;";

        public static string MailChunk_GetChunk = @"SELECT LENGTH(Chunk), Chunk FROM MailChunk WHERE EnvelopeID = $EnvelopeID AND ChunkID = $ChunkID;";
        public static string MailChunk_AddChunk = @"INSERT INTO MailChunk (EnvelopeID, ChunkID, Chunk) VALUES ($EnvelopeID, $ChunkID, $Chunk);";
        public static string MailChunk_DeleteMailData = @"DELETE FROM MailChunk WHERE EnvelopeID = $EnvelopeID;";
        public static string MailChunk_GetMailSize = @"SELECT SUM(LENGTH(Chunk)) FROM MailChunk WHERE EnvelopeID = $EnvelopeID;";

        public static string MailItem_GetByID = @"SELECT MailItemID, UserID, EnvelopeID, Unread FROM MailItem WHERE MailItemID = $MailItemID;";
        public static string MailItem_GetAllByUserId = @"SELECT MailItemID, UserID, EnvelopeID, Unread FROM MailItem WHERE UserID = $UserID;";
        public static string MailItem_Insert = @"INSERT INTO MailItem (UserID, EnvelopeID, Unread) VALUES ($UserID, $EnvelopeID, $Unread);";
        public static string MailItem_UpdateUnread = @"UPDATE MailItem SET Unread = $Unread WHERE MailItemID = $MailItemID;";

        public static string ProcessQueue_GetAll = @"SELECT ProcessQueueID, EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter FROM ProcessQueue;";
        public static string ProcessQueue_GetReady = @"SELECT ProcessQueueID, EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter FROM ProcessQueue WHERE RetryAfter < $RetryAfter AND ProcessQueue.State = 0 ORDER BY RetryAfter;";
        public static string ProcessQueue_GetBusy = @"SELECT ProcessQueueID, EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter FROM ProcessQueue WHERE ProcessQueue.State = 1;";
        public static string ProcessQueue_GetByID = @"SELECT ProcessQueueID, EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter FROM ProcessQueue WHERE ProcessQueueID = $ProcessQueueID;";
        public static string ProcessQueue_Insert = @"INSERT INTO ProcessQueue(EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter) VALUES ($EnvelopeID, $EnvelopeRcptID, $State, $AttemptCount, $RetryAfter);";
        public static string ProcessQueue_Update = @"UPDATE ProcessQueue SET State = $State, AttemptCount = $AttemptCount, RetryAfter = $RetryAfter WHERE ProcessQueueID = $ProcessQueueID;";
        public static string ProcessQueue_DeleteByID = @"DELETE FROM ProcessQueue WHERE ProcessQueueID = $ProcessQueueID";
        public static string ProcessQueue_DeleteByEnvelopeID = @"DELETE FROM ProcessQueue WHERE EnvelopeID = $EnvelopeID;";

        public static string SendLog_GetPage = @"SELCT EnvelopeID, EnvelopeRcptID, WhenAttempted, Results, AttemptCount, Successful FROM SendLog LIMIT $RowCount OFFSET $RowStart ORDER BY WhenAttempted DESC;";
        public static string SendLog_Insert = @"INSERT INTO SendLog (EnvelopeID, EnvelopeRcptID, WhenAttempted, Results, AttemptCount, Successful) VALUES ($EnvelopeID, $EnvelopeRcptID, $WhenAttempted, $Results, $AttemptCount, $Successful);";
        public static string SendLog_DeleteByEnvelopeID = @"DELETE FROM SendLog WHERE EnvelopeID = $EnvelopeID;";

        public static string Recipient_GetByAddress = @"SELECT RecipientID FROM Recipient WHERE Address = $Address;";
        public static string Recipeint_Insert = @"INSERT INTO Recipient (Address) VALUES ($Address);";

        public static string EnvelopeRcpt_GetByID = @"SELECT EnvelopeRcpt.EnvelopeRcptID, EnvelopeRcpt.EnvelopeID, Recipient.Address FROM EnvelopeRcpt INNER JOIN Recipient ON EnvelopeRcpt.RecipientID = Recipient.RecipientID WHERE EnvelopeRcpt.EnvelopeRcptID = $EnvelopeRcptID;";
        public static string EnvelopeRcpt_GetByEnvelopeID = "SELECT EnvelopeRcpt.EnvelopeRcptID, EnvelopeRcpt.EnvelopeID, Recipient.Address FROM EnvelopeRcpt INNER JOIN Recipient ON EnvelopeRcpt.RecipientID = Recipient.RecipientID WHERE EnvelopeRcpt.EnvelopeID = $EnvelopeID;";
        public static string EnvelopeRcpt_Insert = @"INSERT INTO EnvelopeRcpt (EnvelopeID, RecipientID) VALUES ($EnvelopeID, $RecipientID);";
        public static string EnvelopeRcpt_DeleteByEnvelopeID = @"DELETE FROM EnvelopeRcpt WHERE EnvelopeID = $EnvelopeID;";

        public static string IPEndpoint_GetAll = @"SELECT IPEndpointID, Address, Port, Protocol, TLSMode, Hostname, CertFriendlyName, Maildrop FROM IPEndpoint;";
        public static string IPEndpoint_GetByID = @"SELECT IPEndpointID, Address, Port, Protocol, TLSMode, Hostname, CertFriendlyName, Maildrop FROM IPEndpoint WHERE IPEndpointID = $IPEndpointID;";
        public static string IPEndpoint_Insert = @"INSERT INTO IPEndpoint (Address, Port, Protocol, TLSMode, Hostname, CertFriendlyName, Maildrop) VALUES ($Address, $Port, $Protocol, $TLSMode, $Hostname, $CertFriendlyName, $Maildrop);";
        public static string IPEndpoint_Update = @"UPDATE IPEndpoint SET Address = $Address, Port = $Port, Protocol = $Protocol, TLSMode = $TLSMode, Hostname = $Hostname, CertFriendlyName = $CertFriendlyName, Maildrop = $Maildrop WHERE IPEndpointID = $IPEndpointID;";
        public static string IPEndpoint_DeleteByID = @"DELETE FROM IPEndpoint WHERE IPEndpointID = $IPEndpointID;";

        //@"CREATE TABLE DeliveryReport (DeliveryReportID INTEGER PRIMARY KEY, WhenScheduled TEXT, WhenGenerated TEXT, OriginalEnvelopeRcptID INTEGER, EnvelopeID INTEGER, ReportType INTEGER, Reason TEXT, Status INTEGER);"
        public static string DeliveryReport_GetReady = @"SELECT DeliveryReportID, WhenScheduled, WhenGenerated, OriginalEnvelopeRcptID, EnvelopeID, ReportType, Reason, Status FROM DeliveryReport WHERE Status = 0";
        public static string DeliveryReport_InsertEnque = @"INSERT INTO DeliveryReport (WhenScheduled, WhenGenerated, OriginalEnvelopeRcptID, EnvelopeID, ReportType, Reason, Status) VALUES ($WhenScheduled, NULL, $OriginalEnvelopeRcptID, NULL, $ReportType, $Reason, 0);";
        public static string DeliveryReport_MarkRunning = @"UPDATE DeliveryReport SET Status = 1 WHERE DeliveryReportID = $DeliveryReportID;";
        public static string DeliveryReport_UpdateDone = @"UPDATE DeliveryReport SET WhenGenerated = $WhenGenerated, EnvelopeID = $EnvelopeID, Status = $Status WHERE DeliveryReportID = $DeliveryReportID;";

        public static string vwMailQueue_GetQueue = @"SELECT Envelope.Sender, Envelope.Recipients, Envelope.WhenReceived, ProcessQueue.RetryAfter, ProcessQueue.AttemptCount FROM ProcessQueue INNER JOIN Envelope ON ProcessQueue.EnvelopeID = Envelope.EnvelopeID WHERE ProcessQueue.State = 0 ORDER BY ProcessQueue.RetryAfter ASC;";
        public static string vwMailInbox_GetInboxViewByUserId = @"SELECT MailItem.MailItemID, MailItem.EnvelopeID, MailItem.Unread, Envelope.WhenReceived, Envelope.Sender FROM MailItem INNER JOIN Envelope ON MailItem.EnvelopeID = Envelope.EnvelopeID WHERE MailItem.UserID = $UserID ORDER BY Envelope.WhenReceived DESC;";


    }
}
