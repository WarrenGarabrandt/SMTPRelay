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
            @"CREATE TABLE SendQueue (SendQueueID INTEGER PRIMARY KEY, EnvelopeID INTEGER NOT NULL, Recipient TEXT, State INTEGER, AttemptCount INTEGER, RetryAfter TEXT);",
            // Process log. Each attempt to process an email will result in a row being generated with the result of that attempt
            @"CREATE TABLE SendLog (EnvelopeID INTEGER NOT NULL, Recipient TEXT, WhenSent TEXT, Results TEXT, AttemptCount INTEGER);"
        };
        
        public static string System_Get_Version = @"SELECT Value FROM System WHERE Category = $Category AND Setting = $Setting;";
        public static string System_Set_Version = @"INSERT INTO System(Category, Setting, Value) VALUES ($Category, $Setting, $Value);";
    }
}
