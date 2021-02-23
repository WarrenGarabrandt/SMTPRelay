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
                var parms = new List<KeyValuePair<string, string>>();
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    foreach (string cmdstr in SQLiteStrings.Format_Database)
                    {
                        RunNonQuery(s, cmdstr, parms);
                    }
                    parms.Add(new KeyValuePair<string, string>("$Category", "System"));
                    parms.Add(new KeyValuePair<string, string>("$Setting", "Version"));
                    parms.Add(new KeyValuePair<string, string>("$Value", "1.0"));
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
                    List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                    parms.Add(new KeyValuePair<string, string>("$Category", "System"));
                    parms.Add(new KeyValuePair<string, string>("$Setting", "Version"));
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
        /// Resets any SendQueue items that are checked out for processing.
        /// </summary>
        public static void ResetSendQueueItems()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets SendQueue items that are ready to check out
        /// </summary>
        /// <returns></returns>
        public static SendQueue[] GetReadySendQueueItems(int max)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks out a SendQueue item so it can be processed
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static SendQueue CheckoutSendQueueItem(SendQueue item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks in a SendQueue item
        /// </summary>
        /// <param name="item"></param>
        public static void CheckInSendQueueItem(SendQueue item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes a SendQueue item that has been completed (or has permanently failed)
        /// </summary>
        /// <param name="item"></param>
        public static void RemoveSendQueueItem(SendQueue item)
        {
            throw new NotImplementedException();
        }

        private static string RunValueQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, string>> parms)
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

        private static void RunNonQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, string>> parms)
        {
            var command = conn.CreateCommand();
            command.CommandText = query;
            foreach (var kv in parms)
            {
                command.Parameters.AddWithValue(kv.Key, kv.Value);
            }
            command.ExecuteNonQuery();
        }

    }
}
