using SMTPRelay.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay
{
    public static class SQLiteDB
    {
        private static string DatabaseConnectionString
        {
            get
            {
                return string.Format(string.Format("Data Source={0}", StaticConfiguration.DatabasePath));
            }
        }
        
        public static WorkerReport FormatNewDatabase()
        {
            try
            {
                SQLiteConnection.CreateFile(StaticConfiguration.DatabasePath);
                var parms = new List<KeyValuePair<string, object>>();
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    foreach (string cmdstr in SQLiteStrings.Format_Database)
                    {
                        RunNonQuery(s, cmdstr, parms);
                    }
                    parms.Add(new KeyValuePair<string, object>("$Category", "System"));
                    parms.Add(new KeyValuePair<string, object>("$Setting", "Version"));
                    parms.Add(new KeyValuePair<string, object>("$Value", "1.0"));
                    RunNonQuery(s, SQLiteStrings.System_Set_Version, parms);
                }
                return null;
            }
            catch (Exception ex)
            {
                return new WorkerReport()
                {
                    LogError = string.Format("Unable to format the database. {0}", ex.Message)
                };
            }
        }

        public static WorkerReport InitDatabase()
        {
            try
            {
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    List<KeyValuePair<string, object>> parms = new List<KeyValuePair<string, object>>();
                    parms.Add(new KeyValuePair<string, object>("$Category", "System"));
                    parms.Add(new KeyValuePair<string, object>("$Setting", "Version"));
                    string version = RunValueQuery(s, SQLiteStrings.System_Get_Version, parms);
                    if (string.IsNullOrEmpty(version))
                    {
                        throw new Exception("Incompatible database version.");
                    }
                    // Chance to detect old version and perform an in place database upgrade.
                    switch (version)
                    {
                        case "1.0":
                            return null;
                        default:
                            throw new Exception("Incompatible database version.");
                    }
                }
            }
            catch (Exception ex)
            {
                return new WorkerReport()
                {
                    LogError = string.Format("Unable to start the database. {0}", ex.Message),
                };
            }
        }

        /// <summary>
        /// Gets SendQueue items that are ready to check out
        /// </summary>
        /// <returns></returns>
        public static List<SendQueue> SendQueueGetReadyItems(int max = -1)
        {
            List<SendQueue> results = new List<SendQueue>();
            if (max == 0)
            {
                return results;
            }
            using (var conn = new SQLiteConnection(DatabaseConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = SQLiteStrings.SendQueue_Get_Ready_Items;
                using (var reader = cmd.ExecuteReader())
                {
                    int count = 0;
                    while (reader.Read())
                    { 
                        count++;
                        if (max < 0 || count <= max)
                        {
                            SendQueue item = new SendQueue();
                            item.SendQueueID = reader.GetInt64(0);
                            item.EnvelopeID = reader.GetInt64(1);
                            item.Recipient = reader.GetString(2);
                            item.StateValue = reader.GetInt64(3);
                            item.AttemptCount = reader.GetInt64(4);
                            item.RetryAfter = reader.GetDateTime(5);
                            results.Add(item);
                        }
                    }
                }
            }
            return results;
        }

        /// <summary>
        /// Set to Ready (state = 1) all SendQueue items that have been marked busy (state = 2).
        /// </summary>
        public static void SendQueueResetBusyItems()
        {
            using (var conn = new SQLiteConnection(DatabaseConnectionString))
            {
                conn.Open();
                RunNonQuery(conn, SQLiteStrings.SendQueue_Reset_Busy_Items, new List<KeyValuePair<string, object>>());
            }
        }

        /// <summary>
        /// Checks in a SendQueue item
        /// </summary>
        /// <param name="item"></param>
        public static void SendQueueUpdate(SendQueue item)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                if (string.IsNullOrEmpty(item.RetryDelay))
                {
                    List<KeyValuePair<string, object>> parms = new List<KeyValuePair<string, object>>();
                    parms.Add(new KeyValuePair<string, object>("$State", item.StateValue));
                    parms.Add(new KeyValuePair<string, object>("$AttemptCount", item.AttemptCount));
                    parms.Add(new KeyValuePair<string, object>("$RetryAfter", item.RetryAfter));
                    parms.Add(new KeyValuePair<string, object>("$SendQueueID", item.SendQueueID));
                    RunNonQuery(s, SQLiteStrings.SendQueue_Update_By_ID, parms);
                }
                else
                {
                    List<KeyValuePair<string, object>> parms = new List<KeyValuePair<string, object>>();
                    parms.Add(new KeyValuePair<string, object>("$State", item.StateValue));
                    parms.Add(new KeyValuePair<string, object>("$AttemptCount", item.AttemptCount));
                    parms.Add(new KeyValuePair<string, object>("$delay", item.RetryDelay));
                    parms.Add(new KeyValuePair<string, object>("$SendQueueID", item.SendQueueID));
                    RunNonQuery(s, SQLiteStrings.SendQueue_Update_By_ID_With_Delay, parms);
                }
            }
        }

        /// <summary>
        /// Removes a SendQueue item that has been completed (or has permanently failed)
        /// </summary>
        /// <param name="item"></param>
        public static void SendQueueDeleteByID(long id)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                List<KeyValuePair<string, object>> parms = new List<KeyValuePair<string, object>>();
                parms.Add(new KeyValuePair<string, object>("$SendQueueID", id));
                RunNonQuery(s, SQLiteStrings.SendQueue_Delete_By_ID, parms);
            }
        }

        /// <summary>
        /// Gets a list of chunks for the specified envelope, and verifies that each chunk contains something.
        /// Returns a count of how many of them are good and contiguous. If chunk 5 is null, the count will be 4, even if there are later
        /// chunks that were usable.
        /// </summary>
        /// <param name="envelopeID"></param>
        public static long MailChunkGetCountForEnvelope(long envelopeID)
        {
            long result = 0;
            using (var conn = new SQLiteConnection(DatabaseConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = SQLiteStrings.MailChunk_Get_ChunkCount_For_Envelope;
                cmd.Parameters.AddWithValue("$EnvelopeID", envelopeID);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (result != reader.GetInt64(0))
                        {
                            return result;
                        }
                        result++;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves an envelope by the ID provided
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Envelope GetEnvelopeByID(long id)
        {
            Envelope result = null;
            using (var conn = new SQLiteConnection(DatabaseConnectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = SQLiteStrings.Envelope_Get_By_ID;
                cmd.Parameters.AddWithValue("$EnvelopeID", id);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new Envelope();
                        result.EnvelopeID = reader.GetInt64(0);
                        result.WhenReceived = reader.GetDateTime(1);
                        result.Sender = reader.GetString(2);
                        result.Recipients = ParseRecipients(reader.GetString(3));
                        result.ChunkCount = reader.GetInt64(4);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Createsa new log entry in SendLog with the specified values
        /// </summary>
        /// <param name="envelopeID"></param>
        /// <param name="recipient"></param>
        /// <param name="whenSent"></param>
        /// <param name="results"></param>
        /// <param name="attemptCount"></param>
        public static void SendLogAddLogEntry(long envelopeID, string recipient, string results, long attemptCount)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                List<KeyValuePair<string, object>> parms = new List<KeyValuePair<string, object>>();
                parms.Add(new KeyValuePair<string, object>("$EnvelopeID", envelopeID));
                parms.Add(new KeyValuePair<string, object>("$Recipient", recipient));
                parms.Add(new KeyValuePair<string, object>("$Results", results));
                parms.Add(new KeyValuePair<string, object>("$AttemptCount", attemptCount));
                RunNonQuery(s, SQLiteStrings.SendLog_Insert, parms);
            }
        }

        private static string RunValueQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, object>> parms)
        {
            string result = null;
            var command = conn.CreateCommand();
            command.CommandText = query;
            foreach (var kv in parms)
            {
                command.Parameters.AddWithValue(kv.Key, kv.Value);
            }
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result = reader.GetString(0);
                }
            }
            return result;
        }

        private static void RunNonQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, object>> parms)
        {
            var command = conn.CreateCommand();
            command.CommandText = query;
            foreach (var kv in parms)
            {
                command.Parameters.AddWithValue(kv.Key, kv.Value);
            }
            command.ExecuteNonQuery();
        }

        private static string[] ParseRecipients(string value)
        {
            return value.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
