using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay
{
    public static class SQLiteStrings
    {
        public static string[] Format_Database = new string[]
        {
            // Contains configuration and version data.
            @"CREATE TABLE System (Category TEXT, Setting TEXT, Value TEXT);",
            // Basic email header info.
            @"CREATE TABLE Envelope (EnvelopeID INTEGER PRIMARY KEY, WhenReceived TEXT, Sender TEXT, Recipients TEXT, ChunkCount INTEGER);",
            // Stores email body in chunks.
            @"CREATE TABLE MailChunk (EnvelopeID INTEGER NOT NULL, ChunkID INTEGER NOT NULL, Chunk TEXT);",
            // Queue of items to be transmitted.
            // State Value: 0: Item is not to be processed. 1: Item is ready to run (if RetryAfter is met). 2: Item is running.
            @"CREATE TABLE SendQueue (SendQueueID INTEGER PRIMARY KEY, EnvelopeID INTEGER NOT NULL, Recipient TEXT, State INTEGER, AttemptCount INTEGER, RetryAfter TEXT);",
            // Process log. Each attempt to process an email will result in a row being generated with the result of that attempt
            @"CREATE TABLE SendLog (EnvelopeID INTEGER NOT NULL, Recipient TEXT, WhenSent TEXT, Results TEXT, AttemptCount INTEGER);"
        };
        
        public static string System_Get_Version = @"SELECT Value FROM System WHERE Category = $Category AND Setting = $Setting;";
        public static string System_Set_Version = @"INSERT INTO System(Category, Setting, Value) VALUES ($Category, $Setting, $Value);";

        public static string SendQueue_Get_Ready_Items = @"SELECT SendQueueID, EnvelopeID, Recipient, State, AttemptCount, RetryAfter FROM SendQueue WHERE State = 1 AND datetime('now') >= datetime(RetryAfter) ORDER BY RetryAfter;";
        public static string SendQueue_Reset_Busy_Items = @"UPDATE SendQueue SET State = 1 WHERE State = 2;";
        public static string SendQueue_Update_By_ID = @"UPDATE SendQueue SET State = $State, AttemptCount = $AttemptCount, RetryAfter = $RetryAfter WHERE SendQueueID = $SendQueueID;";
        public static string SendQueue_Update_By_ID_With_Delay = @"UPDATE SendQueue SET State = $State, AttemptCount = $AttemptCount, RetryAfter = datetime('now', $delay) WHERE SendQueueID = $SendQueueID;";
        public static string SendQueue_Delete_By_ID = @"DELETE FROM SendQueue WHERE SendQueueID = $SendQueueID;";

        public static string Envelope_Get_By_ID = @"SELECT EnvelopeID, WhenReceived, Sender, Recipients, ChunkCount FROM Envelope WHERE EnvelopeID = $EnvelopeID;";

        public static string SendLog_Insert = @"INSERT INTO SendLog(EnvelopeID, Recipient, WhenSent, Results, AttemptCount) VALUES $EnvelopeID, $Recipient, datetime('now'), $Results, $AttemptCount;";

        public static string MailChunk_Get_ChunkCount_For_Envelope = @"SELECT ChunkID FROM MailChunk WHERE EnvelopeID = $EnvelopeID AND Chunk IS NOT NULL ORDER BY ChunkID;";
    }
}
