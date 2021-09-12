using SMTPRelay.Model;
using SMTPRelay.Model.DB;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace SMTPRelay.Database
{
    public static class SQLiteDB
    {
        private const string COMPATIBLE_DATABASE_VERSION = "1.0";

        private static string DatabasePath
        {
            get
            {
                string progdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                return System.IO.Path.Combine(progdata, "SMTPRelay");
            }
        }

        private static string DatabaseFile
        {
            get
            {
                return System.IO.Path.Combine(DatabasePath, "config.db");
            }
        }

        private static string _cached_DatabaseConnectionString = null;
        private static string DatabaseConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_cached_DatabaseConnectionString))
                {
                    _cached_DatabaseConnectionString = string.Format(string.Format("Data Source={0}", DatabaseFile));
                }
                return _cached_DatabaseConnectionString;
            }
        }
        
        public static WorkerReport FormatNewDatabase()
        {
            try
            {
                if (!System.IO.Directory.Exists(DatabasePath))
                {
                    System.IO.Directory.CreateDirectory(DatabasePath);
                }
                SQLiteConnection.CreateFile(DatabaseFile);
                var parms = new List<KeyValuePair<string, string>>();
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    foreach (string cmdstr in SQLiteStrings.Format_Database)
                    {
                        RunNonQuery(s, cmdstr, parms);
                    }

                    // Insert the database version
                    parms.Add(new KeyValuePair<string, string>("$Category", "System"));
                    parms.Add(new KeyValuePair<string, string>("$Setting", "Version"));
                    parms.Add(new KeyValuePair<string, string>("$Value", COMPATIBLE_DATABASE_VERSION));
                    RunNonQuery(s, SQLiteStrings.System_Insert, parms);
                    parms.Clear();

                    // Create the admin user
                    tblUser newAdminUser = new tblUser("Administrator", "admin@local", "", "", true, true, null);
                    GeneratePasswordHash(newAdminUser, "password");
                    User_AddUpdate(newAdminUser);

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
                if (!System.IO.File.Exists(DatabaseFile))
                {
                    WorkerReport rep = FormatNewDatabase();
                    if (rep != null)
                    {
                        return rep;
                    }
                }
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                    parms.Add(new KeyValuePair<string, string>("$Category", "System"));
                    parms.Add(new KeyValuePair<string, string>("$Setting", "Version"));
                    string value = RunValueQuery(s, SQLiteStrings.System_Select, parms);

                }
            }
            catch (Exception ex)
            {
                return new WorkerReport()
                {
                    LogError = string.Format("Unable to start the database. {0}", ex.Message),
                };
            }
            return null;
        }

        /// <summary>
        /// Gets a value from a table. if not found, returns null;
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="query"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        private static string RunValueQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, string>> parms)
        {
            string result = null;
            using (var command = conn.CreateCommand())
            {
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
            }
            return result;
        }

        private static void RunNonQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, string>> parms)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = query;
                foreach (var kv in parms)
                {
                    command.Parameters.AddWithValue(kv.Key, kv.Value);
                }
                command.ExecuteNonQuery();
            }
        }

        public static List<tblSystem> System_GetAll()
        {
            List<tblSystem> results = new List<tblSystem>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.System_GetAll;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new tblSystem(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
                        }
                    }
                }
            }
            return results;
        }

        public static string System_GetValue(string category, string setting)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                parms.Add(new KeyValuePair<string, string>("$Category", category));
                parms.Add(new KeyValuePair<string, string>("$Setting", setting));
                return RunValueQuery(s, SQLiteStrings.System_Select, parms);
            }
        }

        public static void System_AddUpdateValue(string category, string setting, string value)
        {
            var parms = new List<KeyValuePair<string, string>>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                parms.Add(new KeyValuePair<string, string>("$Category", category));
                parms.Add(new KeyValuePair<string, string>("$Setting", setting));
                parms.Add(new KeyValuePair<string, string>("$Value", value));
                RunNonQuery(s, SQLiteStrings.System_Insert, parms);
            }
        }

        public static List<tblUser> User_GetAll()
        {
            List<tblUser> results = new List<tblUser>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.User_GetAll;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new tblUser(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7)));
                        }
                    }
                }
            }
            return results;
        }

        public static tblUser User_GetByID(long? userID)
        {
            tblUser dbUser = null;
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.User_GetByID;
                    command.Parameters.AddWithValue("$UserID", userID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dbUser = new tblUser(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7));
                        }
                    }
                }
            }
            return dbUser;
        }

        public static tblUser User_GetByEmail(string email)
        {
            tblUser results = null;
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.User_GetByEmail;
                    command.Parameters.AddWithValue("$Email", email);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            results = new tblUser(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7));
                        }
                    }
                }
            }
            return results;
        }

        public static tblUser User_GetByEmailPassword(string email, string password)
        {
            tblUser user = User_GetByEmail(email);
            if (user == null)
            {
                return null;
            }
            if (user.Enabled && ValidatePasswordHash(user, password))
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        public static void User_AddUpdate(tblUser user)
        {
            // if the UserID is populated, then we are going to try to update first. 
            // the update might fail, in which case we insert below
            if (user.UserID.HasValue)
            {
                // update
                tblUser dbUser = null;
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    using (var command = s.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.User_GetByID;
                        command.Parameters.AddWithValue("$UserID", user.UserID);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                dbUser = new tblUser(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7));
                            }
                        }
                    }
                    if (dbUser == null)
                    {
                        // user doesn't exit afterall
                        user.UserID = null;
                    }
                    else
                    {
                        using (var command = s.CreateCommand())
                        {
                            command.CommandText = SQLiteStrings.User_Update;
                            //@"UPDATE User SET DisplayName = $DisplayName, Email = $Email, Salt = $Salt, PassHash = $PassHash, Enabled = $Enabled, Admin = $Admin WHERE UserID = $UserID;"
                            command.Parameters.AddWithValue("$DisplayName", user.DisplayName);
                            command.Parameters.AddWithValue("$Email", user.Email);
                            command.Parameters.AddWithValue("$Salt", user.Salt);
                            command.Parameters.AddWithValue("$PassHash", user.PassHash);
                            command.Parameters.AddWithValue("$Enabled", user.EnabledInt);
                            command.Parameters.AddWithValue("$Admin", user.AdminInt);
                            if (user.MailGateway.HasValue)
                            {
                                command.Parameters.AddWithValue("$MailGatewayID", user.MailGateway);
                            }
                            else
                            {
                                command.Parameters.AddWithValue("$MailGatewayID", DBNull.Value);
                            }
                            command.Parameters.AddWithValue("$UserID", user.UserID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            // if there is no UserID, then we insert a new record and select the ID back.
            if (!user.UserID.HasValue)
            {
                // insert new record and read back the ID
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    using (var command = s.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.User_Insert;
                        //@"INSERT INTO User(DisplayName, Email, Salt, PassHash, Enabled, Admin) VALUES ($DisplayName, $Email, $Salt, $PassHash, $Enabled, $Admin);"
                        command.Parameters.AddWithValue("$DisplayName", user.DisplayName);
                        command.Parameters.AddWithValue("$Email", user.Email);
                        command.Parameters.AddWithValue("$Salt", user.Salt);
                        command.Parameters.AddWithValue("$PassHash", user.PassHash);
                        command.Parameters.AddWithValue("$Enabled", user.EnabledInt);
                        command.Parameters.AddWithValue("$Admin", user.AdminInt);
                        if (user.MailGateway.HasValue)
                        {
                            command.Parameters.AddWithValue("$MailGatewayID", user.MailGateway);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("$MailGatewayID", DBNull.Value);
                        }
                        command.ExecuteNonQuery();
                    }
                    using (var command = s.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.Table_LastRowID;
                        user.UserID = (long)command.ExecuteScalar();
                    }
                }
            }
        }

        public static void User_ClearGatewayByID(long gatewayID)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.User_ClearGatewayByID;
                    command.Parameters.AddWithValue("$MailGatewayID", gatewayID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void User_DeleteByID(long userID)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.User_DeleteByID;
                    command.Parameters.AddWithValue("$UserID", userID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void GeneratePasswordHash(tblUser user, string newPassword)
        {
            user.Salt = GenerateNonce(16);
            string Password = string.Format("{0}:{1}", user.Salt, newPassword);
            byte[] passbytes = UTF8Encoding.UTF8.GetBytes(Password);
            using (SHA256 sha = SHA256.Create())
            {
                passbytes = sha.ComputeHash(passbytes);
            }
            user.PassHash = Convert.ToBase64String(passbytes);
        }

        public static bool ValidatePasswordHash(tblUser user, string password)
        {
            string Password = string.Format("{0}:{1}", user.Salt, password);
            byte[] passbytes = UTF8Encoding.UTF8.GetBytes(Password);
            using (SHA256 sha = SHA256.Create())
            {
                passbytes = sha.ComputeHash(passbytes);
            }
            if (user.PassHash == Convert.ToBase64String(passbytes))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GenerateNonce(int len)
        {
            Random rnd = new Random();
            string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                sb.Append(chars[rnd.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        public static List<tblEnvelope> Envelope_GetAll()
        {
            List<tblEnvelope> results = new List<tblEnvelope>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.Envelope_GetAll;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new tblEnvelope(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4)));
                        }
                    }
                }
            }
            return results;
        }

        public static tblEnvelope Envelope_GetByID(long envelopeID)
        {
            tblEnvelope result = null;
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.Envelope_GetByID;
                    command.Parameters.AddWithValue("$EnvelopeID", envelopeID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            result = new tblEnvelope(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4));
                        }
                    }
                }
            }
            return result;
        }

        public static void Envelope_Add(tblEnvelope envelope)
        {
            // insert new record and read back the ID
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.Envelope_Insert;
                    command.Parameters.AddWithValue("$WhenReceived", envelope.WhenReceivedString);
                    command.Parameters.AddWithValue("$Sender", envelope.Sender);
                    command.Parameters.AddWithValue("$Recipients", envelope.Recipients);
                    command.Parameters.AddWithValue("$ChunkCount", envelope.ChunkCount);
                    command.ExecuteNonQuery();
                }
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.Table_LastRowID;
                    envelope.EnvelopeID = (long)command.ExecuteScalar();
                }
            }
        }

        public static void Envelope_UpdateChunkCount(long envelopeID, int chunkCount)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.Envelope_UpdateChunkCount;
                    command.Parameters.AddWithValue("$EnvelopeID", envelopeID);
                    command.Parameters.AddWithValue("$ChunkCount", chunkCount);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<tblMailGateway> MailGateway_GetAll()
        {
            List<tblMailGateway> results = new List<tblMailGateway>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.MailGateway_GetAll;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new tblMailGateway(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetString(5), reader.GetString(6), reader.GetString(7)));
                        }
                    }
                }
            }
            return results;
        }

        public static tblMailGateway MailGateway_GetByID(long mailGatewayID)
        {
            tblMailGateway results = null;
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.MailGateway_GetByID;
                    command.Parameters.AddWithValue("$MailGatewayID", mailGatewayID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            results = new tblMailGateway(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetString(5), reader.GetString(6), reader.GetString(7));
                        }
                    }
                }
            }
            return results;
        }

        public static void MailGateway_AddUpdate(tblMailGateway mailGateway)
        {
            // if the MailGatewayID is populated, then we are going to try to update first. 
            // the update might fail, in which case we insert below
            if (mailGateway.MailGatewayID.HasValue)
            {
                // update. First, read the existing record by ID to make sure it exists. 
                tblMailGateway dbMailGateway = null;
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    using (var command = s.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.MailGateway_GetByID;
                        command.Parameters.AddWithValue("$MailGatewayID", mailGateway.MailGatewayID);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                dbMailGateway = new tblMailGateway(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetString(5), reader.GetString(6), reader.GetString(7));
                            }
                        }
                    }
                    if (dbMailGateway == null)
                    {
                        // MailGateway doesn't exit, so below we will insert it.
                        mailGateway.MailGatewayID = null;
                    }
                    else
                    {
                        using (var command = s.CreateCommand())
                        {
                            command.CommandText = SQLiteStrings.MailGateway_Update;
                            command.Parameters.AddWithValue("$SMTPServer", mailGateway.SMTPServer);
                            command.Parameters.AddWithValue("$Port", mailGateway.Port);
                            command.Parameters.AddWithValue("$EnableSSL", mailGateway.EnableSSLInt);
                            command.Parameters.AddWithValue("$Authenticate", mailGateway.AuthenticateInt);
                            command.Parameters.AddWithValue("$Username", mailGateway.Username);
                            command.Parameters.AddWithValue("$Password", mailGateway.Password);
                            command.Parameters.AddWithValue("$SenderOverride", mailGateway.SenderOverride);
                            command.Parameters.AddWithValue("$MailGatewayID", mailGateway.MailGatewayID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            // if there is no MailGatewayID, then we insert a new record and select the ID back.
            if (!mailGateway.MailGatewayID.HasValue)
            {
                // insert new record and read back the ID
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    using (var command = s.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.MailGateway_Insert;
                        command.Parameters.AddWithValue("$SMTPServer", mailGateway.SMTPServer);
                        command.Parameters.AddWithValue("$Port", mailGateway.Port);
                        command.Parameters.AddWithValue("$EnableSSL", mailGateway.EnableSSLInt);
                        command.Parameters.AddWithValue("$Authenticate", mailGateway.AuthenticateInt);
                        command.Parameters.AddWithValue("$Username", mailGateway.Username);
                        command.Parameters.AddWithValue("$Password", mailGateway.Password);
                        command.Parameters.AddWithValue("$SenderOverride", mailGateway.SenderOverride);
                        command.ExecuteNonQuery();
                    }
                    using (var command = s.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.Table_LastRowID;
                        mailGateway.MailGatewayID = (long)command.ExecuteScalar();
                    }
                }
            }
        }

        public static void MailGateway_DeleteByID(long gatewayID)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.MailGateway_DeleteByID;
                    command.Parameters.AddWithValue("$MailGatewayID", gatewayID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<vwMailQueue> MailQueue_QueryView()
        {
            List<vwMailQueue> results = new List<vwMailQueue>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.vwMailQueue_GetQueue;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new vwMailQueue(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetInt64(4)));
                        }
                    }
                }
            }
            return results;
        }

        public static byte[] MailChunk_GetChunk(long envelopeID, long chunkID)
        {
            byte[] result = null;
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.MailChunk_GetChunk;
                    command.Parameters.AddWithValue("$EnvelopeID", envelopeID);
                    command.Parameters.AddWithValue("$ChunkID", chunkID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read() && !reader.IsDBNull(1))
                        {
                            long len = reader.GetInt32(0);
                            result = new byte[len];
                            len = reader.GetBytes(1, 0, result, 0, (int)len);
                        }
                    }
                }
            }
            return result;
        }

        public static void MailChunk_AddChunk(long envelopeID, long chunkID, byte[] buffer)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.MailChunk_AddChunk;
                    command.Parameters.AddWithValue("$EnvelopeID", envelopeID);
                    command.Parameters.AddWithValue("$ChunkID", chunkID);
                    command.Parameters.Add("$Chunk", System.Data.DbType.Binary, buffer.Length).Value = buffer;
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void MailChunk_DeleteMailData(long envelopeID)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.MailChunk_DeleteMailData;
                    command.Parameters.AddWithValue("$EnvelopeID", envelopeID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static long MailChunk_GetMailSize(long envelopeID)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.MailChunk_GetMailSize;
                    command.Parameters.AddWithValue("$EnvelopeID", envelopeID);
                    return (long)command.ExecuteScalar();
                }
            }
        }

        public static List<tblSendQueue> SendQueue_GetAll()
        {
            List<tblSendQueue> results = new List<tblSendQueue>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.SendQueue_GetAll;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new tblSendQueue(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt32(3), reader.GetInt32(4), reader.IsDBNull(5) ? null : reader.GetString(5)));
                        }
                    }
                }
            }
            return results;
        }

        public static List<tblSendQueue> SendQueue_GetReady()
        {
            List<tblSendQueue> results = new List<tblSendQueue>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.SendQueue_GetReady;
                    command.Parameters.AddWithValue("$RetryAfter", DateTime.UtcNow.ToString("O"));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //SELECT SendQueueID, EnvelopeID, EnvelopeRcptID, State, AttemptCount, RetryAfter FROM SendQueue;
                            results.Add(new tblSendQueue(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt32(3), reader.GetInt32(4), reader.IsDBNull(5) ? null : reader.GetString(5)));
                        }
                    }
                }
            }
            return results;
        }

        public static void SendQueue_AddUpdate(tblSendQueue sendqueue)
        {
            // if the SendQueuID is populated, then we are going to try to update first. 
            // the update might fail, in which case we insert below
            if (sendqueue.SendQueueID.HasValue)
            {
                // update. First, read the existing record by ID to make sure it exists. 
                tblSendQueue dbsendqueue = null;
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    using (var command = s.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.SendQueue_GetByID;
                        command.Parameters.AddWithValue("$SendQueueID", sendqueue.SendQueueID);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                dbsendqueue = new tblSendQueue(reader.GetInt64(0), reader.GetInt64(1), reader.GetInt64(2), reader.GetInt32(3), reader.GetInt32(4), reader.IsDBNull(5) ? null : reader.GetString(5));
                            }
                        }
                    }
                    if (dbsendqueue == null)
                    {
                        // SendQueue doesn't exit, so below we will insert it.
                        sendqueue.SendQueueID = null;
                    }
                    else
                    {
                        using (var command = s.CreateCommand())
                        {
                            command.CommandText = SQLiteStrings.SendQueue_Update;
                            command.Parameters.AddWithValue("$State", sendqueue.StateInt);
                            command.Parameters.AddWithValue("$AttemptCount", sendqueue.AttemptCount);
                            command.Parameters.AddWithValue("$RetryAfter", sendqueue.RetryAfterStr);
                            command.Parameters.AddWithValue("$SendQueueID", sendqueue.SendQueueID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }

            // if there is no SendQueueID, then we insert a new record and select the ID back.
            if (!sendqueue.SendQueueID.HasValue)
            {
                // insert new record and read back the ID
                using (var s = new SQLiteConnection(DatabaseConnectionString))
                {
                    s.Open();
                    using (var command = s.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.SendQueue_Insert;
                        command.Parameters.AddWithValue("$EnvelopeID", sendqueue.EnvelopeID);
                        command.Parameters.AddWithValue("$EnvelopeRcptID", sendqueue.EnvelopeRcptID);
                        command.Parameters.AddWithValue("$State", sendqueue.StateInt);
                        command.Parameters.AddWithValue("$AttemptCount", sendqueue.AttemptCount);
                        command.Parameters.AddWithValue("$RetryAfter", sendqueue.RetryAfterStr);
                        command.ExecuteNonQuery();
                    }
                    using (var command = s.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.Table_LastRowID;
                        sendqueue.SendQueueID = (long)command.ExecuteScalar();
                    }
                }
            }
        }

        public static void SendQueue_DeleteByID(long sendQueueID)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.SendQueue_DeleteByID;
                    command.Parameters.AddWithValue("$SendQueueID", sendQueueID);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Get a page of SendLog.
        /// </summary>
        /// <param name="count">How many rows to return</param>
        /// <param name="offset">zero-based index for first row to return</param>
        /// <returns></returns>
        public static List<tblSendLog> SendLog_GetPage(long count, long offset)
        {
            List<tblSendLog> results = new List<tblSendLog>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.SendLog_GetPage;
                    command.Parameters.AddWithValue("$RowCount", count);
                    command.Parameters.AddWithValue("$RowStart", offset);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(new tblSendLog(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4)));
                        }
                    }
                }
            }
            return results;
        }

        public static void SendLog_Insert(tblSendLog sendlog)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.SendLog_Insert;
                    command.Parameters.AddWithValue("$EnvelopeID", sendlog.EnvelopeID);
                    command.Parameters.AddWithValue("$EnvelopeRcptID", sendlog.EnvelopeRcptID);
                    command.Parameters.AddWithValue("$WhenAttempted", sendlog.WhenAttemptedStr);
                    command.Parameters.AddWithValue("$Results", sendlog.Results);
                    command.Parameters.AddWithValue("$AttemptCount", sendlog.AttemptCount);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<tblEnvelopeRcpt> EnvelopeRcpt_GetByEnvelopeID (long envelopeID)
        {
            List<tblEnvelopeRcpt> results = new List<tblEnvelopeRcpt>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.EnvelopeRcpt_GetByEnvelopeID;
                    command.Parameters.AddWithValue("$EnvelopeID", envelopeID);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //EnvelopeRcptID, EnvelopeID, Recipient
                            results.Add(new tblEnvelopeRcpt(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2)));
                        }
                    }
                }
            }
            return results;
        }

        public static void EnvelopeRcpt_Insert(long envelopeID, string recipient)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.EnvelopeRcpt_Insert;
                    command.Parameters.AddWithValue("$EnvelopeID", envelopeID);
                    command.Parameters.AddWithValue("$Recipient", recipient);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
