using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Database
{
    public static class SQLiteStrings
    {

        public static string[] Format_Database = new string[]
        {
            // Contains configuration and version data.
            @"CREATE TABLE System (Category TEXT, Setting TEXT, Value TEXT);",

            // create unique constraint so that REPLACE INTO will function properly.
            @"CREATE UNIQUE INDEX idx_System_CategorySetting ON System(Category, Setting);",

            // User table
            @"CREATE TABLE User (UserID INTEGER PRIMARY KEY, DisplayName TEXT, Email TEXT, Salt TEXT NOT NULL, PassHash TEXT NOT NULL, Enabled INTEGER NOT NULL, Admin INTEGER NOT NULL, MailGatewayID INTEGER);",

            // Mail Gateway Table
            @"CREATE TABLE MailGateway (MailGatewayID INTEGER PRIMARY KEY, SMTPServer TEXT NOT NULL, Port INTEGER NOT NULL, EnableSSL INTEGER NOT NULL, Authenticate INTEGER NOT NULL, Username TEXT, Password TEXT, SenderOverride TEXT);",

            // Basic email header info.
            @"CREATE TABLE Envelope (EnvelopeID INTEGER PRIMARY KEY, WhenReceived TEXT, Sender TEXT, Recipients TEXT, ChunkCount INTEGER);",

            // Envelope Recipients
            @"CREATE TABLE EnvelopeRcpt(EnvelopeRcptID INTEGER PRIMARY KEY, EnvelopeID INTEGER, Recipient TEXT);",

            // Stores email body in chunks.
            @"CREATE TABLE MailChunk (EnvelopeID INTEGER NOT NULL, ChunkID INTEGER NOT NULL, Chunk TEXT);",

            // Queue of items to be transmitted. Items are removed as they are completed.
            @"CREATE TABLE SendQueue (SendQueueID INTEGER PRIMARY KEY, EnvelopeID INTEGER NOT NULL, EnvelopeRcptID INTEGER NOT NULL, State INTEGER, AttemptCount INTEGER, RetryAfter TEXT);",
            
            // Process log. Each attempt to process an email will result in a row being generated with the result of that attempt
            @"CREATE TABLE SendLog (EnvelopeID INTEGER NOT NULL, EnvelopeRcptID INTEGER NOT NULL, WhenAttempted TEXT, Results TEXT, AttemptCount INTEGER);"
        
            
        };

        /// <summary>
        /// Retrieves the last ID that was inserted into a table.
        /// </summary>
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

        public static string Envelope_GetAll = @"SELECT EnvelopeID, WhenReceived, Sender, Recipients, ChunkCount FROM Envelope;";
        public static string Envelope_GetByID = @"SELECT EnvelopeID, WhenReceived, Sender, Recipients, ChunkCount FROM Envelope WHERE EnvelopeID = $EnvelopeID;";
        public static string Envelope_Insert = @"INSERT INTO Envelope(WhenReceived, Sender, Recipients, ChunkCount) VALUES ($WhenReceived, $Sender, $Recipients, $ChunkCount);";
        public static string Envelope_UpdateChunkCount = @"UPDATE Envelope SET ChunkCount = $ChunkCount WHERE EnvelopeID = $EnvelopeID;";

        public static string MailGateway_GetAll = @"SELECT MailGatewayID, SMTPServer, Port, EnableSSL, Authenticate, Username, Password, SenderOverride FROM MailGateway;";
        public static string MailGateway_GetByID = @"SELECT MailGatewayID, SMTPServer, Port, EnableSSL, Authenticate, Username, Password, SenderOverride FROM MailGateway WHERE MailGatewayID = $MailGatewayID;";
        public static string MailGateway_Insert = @"INSERT INTO MailGateway (SMTPServer, Port, EnableSSL, Authenticate, Username, Password, SenderOverride) VALUES ($SMTPServer, $Port, $EnableSSL, $Authenticate, $Username, $Password, $SenderOverride);";
        public static string MailGateway_Update = @"UPDATE MailGateway SET SMTPServer = $SMTPServer, Port = $Port, EnableSSL = $EnableSSL, Authenticate = $Authenticate, Username = $Username, Password = $Password, SenderOverride = $SenderOverride WHERE MailGatewayID = $MailGatewayID;";
        public static string MailGateway_DeleteByID = @"DELETE FROM MailGateway WHERE MailGatewayID = $MailGatewayID;";
    }
}
