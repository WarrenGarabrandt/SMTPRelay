using SMTPRelay.Model;
using SMTPRelay.Model.DB;
using SMTPRelay.Model.Query;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace SMTPRelay.Database
{
    public static class SQLiteDB
    {
        private static BackgroundWorker Worker = null;

        private static BlockingCollection<DatabaseQuery> QueryQueue = new BlockingCollection<DatabaseQuery>();

        private static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BlockingCollection<DatabaseQuery> queue = e.Argument as BlockingCollection<DatabaseQuery>;
            if (queue == null)
            {
                e.Result = new Exception("No Work Queue Provided.");
                return;
            }
            SQLiteConnection conn = null;
            try
            {
                while (!Worker.CancellationPending)
                {
                    try
                    {
                        DatabaseQuery query;
                        if (queue.TryTake(out query, 2000))
                        {
                            switch (query)
                            {
                                case DatabaseInit q:
                                    _initDatabase(ref conn, q);
                                    break;
                                case qryGetAllConfigValues q:
                                    _system_GetAll(ref conn, q);
                                    break;
                                case qryGetConfigValue q:
                                    _system_GetValue(ref conn, q);
                                    break;
                                case qrySetConfigValue q:
                                    _system_AddUpdateValue(ref conn, q);
                                    break;
                                case qryGetAllUsers q:
                                    _user_GetAll(ref conn, q);
                                    break;
                                case qryGetUserByID q:
                                    _user_GetByID(ref conn, q);
                                    break;
                                case qryGetUserByEmail q:
                                    _user_GetByEmail(ref conn, q);
                                    break;
                                case qryGetUserByEmailPassword q:
                                    _user_GetByEmailPassword(ref conn, q);
                                    break;
                                case qrySetUser q:
                                    _user_AddUpdate(ref conn, q);
                                    break;
                                default:
                                    throw new Exception(string.Format("Don't understand object type: {0}", query.GetType().ToString()));
                            }
                        }
                        else
                        {
                            if (conn != null)
                            {
                                try
                                {
                                    conn.Dispose();
                                }
                                catch { }
                                conn = null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            finally
            {
                if (conn != null)
                {
                    try
                    {
                        conn.Dispose();
                        conn = null;
                    }
                    catch { }
                }
            }
            throw new NotImplementedException();
        }

        private static void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                System.Diagnostics.Debug.WriteLine(((Exception)e.Result).Message);
            }
            DatabaseQuery q;
            while (QueryQueue.TryTake(out q, 100))
            {
                q.Abort();
            }
        }

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

        #region Public Methods
        /// <summary>
        /// Sets up the connection to the database. Will create a new database if one doesn't exist already.
        /// </summary>
        /// <returns></returns>
        public static WorkerReport InitDatabase()
        {
            if (Worker != null && !Worker.IsBusy)
            {
                Worker = null;
            }
            if (Worker == null)
            {
                Worker = new BackgroundWorker();
                Worker.WorkerReportsProgress = false;
                Worker.WorkerSupportsCancellation = true;
                Worker.DoWork += Worker_DoWork;
                Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                Worker.RunWorkerAsync(QueryQueue);
            }
            DatabaseInit q = new DatabaseInit();
            QueryQueue.Add(q);
            return q.GetResult();
        }
        /// <summary>
        /// Gets all system configuration values.
        /// </summary>
        /// <returns>All config values in the System table</returns>
        public static List<tblSystem> System_GetAll()
        {
            qryGetAllConfigValues q = new qryGetAllConfigValues();
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Returns a setting value from the System table for a given Category and Setting.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static string System_GetValue(string category, string setting)
        {
            qryGetConfigValue q = new qryGetConfigValue(category, setting);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Creates or updates a setting to a specified value.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool System_AddUpdateValue(string category, string setting, string value)
        {
            qrySetConfigValue q = new qrySetConfigValue(category, setting, value);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a list of all user accounts
        /// </summary>
        /// <returns></returns>
        public static List<tblUser> User_GetAll()
        {
            qryGetAllUsers q = new qryGetAllUsers();
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a user by UserID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static tblUser User_GetByID(long userID)
        {
            qryGetUserByID q = new qryGetUserByID(userID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a user by email address
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static tblUser User_GetByEmail(string email)
        {
            qryGetUserByEmail q = new qryGetUserByEmail(email);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a user by email address and password.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static tblUser User_GetByEmailPassword(string email, string password)
        {
            qryGetUserByEmailPassword q = new qryGetUserByEmailPassword(email, password);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Adds or updates a user in the database.
        /// </summary>
        /// <param name="user"></param>
        public static bool User_AddUpdate(tblUser user)
        {
            qrySetUser q = new qrySetUser(user);
            QueryQueue.Add(q);
            return q.GetResult();
        }
        #endregion

        #region Private Methods
        private static bool _verifyConnection(ref SQLiteConnection conn)
        {
            if (conn != null)
            {
                if (conn.State == System.Data.ConnectionState.Broken || conn.State == System.Data.ConnectionState.Closed)
                {
                    try
                    {
                        conn.Dispose();
                        conn = null;
                    }
                    catch { }
                }
            }
            if (conn == null)
            {
                try
                {
                    conn = new SQLiteConnection(DatabaseConnectionString);
                    conn.Open();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Unable to connect to the database. {0}", ex.Message));
                    return false;
                }
            }
            return true;
        }

        private static void _initDatabase(ref SQLiteConnection conn, DatabaseInit query)
        {
            if (conn != null)
            {
                try
                {
                    conn.Dispose();
                }
                catch { }
                conn = null;
            }
            try
            {
                if (!System.IO.File.Exists(DatabaseFile))
                {
                    try
                    {
                        _formatNewDatabase();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error formattting database: {0}", ex.Message));
                    }
                }
                conn = new SQLiteConnection(DatabaseConnectionString);
                conn.Open();
                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                parms.Add(new KeyValuePair<string, string>("$Category", "System"));
                parms.Add(new KeyValuePair<string, string>("$Setting", "Version"));
                string value = _runValueQuery(conn, SQLiteStrings.System_Select, parms);
                if (value != COMPATIBLE_DATABASE_VERSION)
                {
                    throw new Exception("Incompatible database version.");
                }
                query.SetResult(null);
            }
            catch (Exception ex)
            {
                query.SetResult(new WorkerReport()
                {
                    LogError = string.Format("Unable to start the database. {0}", ex.Message),
                });
                return;
            }
        }

        private static void _formatNewDatabase()
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
                // create all tables
                foreach (string cmdstr in SQLiteStrings.Format_Database)
                {
                    _runNonQuery(s, cmdstr, parms);
                }

                // create all default values
                foreach (var setting in SQLiteStrings.DatabaseDefaults)
                {
                    parms.Add(new KeyValuePair<string, string>("$Category", setting.Item1));
                    parms.Add(new KeyValuePair<string, string>("$Setting", setting.Item2));
                    parms.Add(new KeyValuePair<string, string>("$Value", setting.Item3));
                    _runNonQuery(s, SQLiteStrings.System_Insert, parms);
                }

                // Create the admin user
                tblUser newAdminUser = new tblUser("Administrator", "admin@local", "", "", true, true, null);
                GeneratePasswordHash(newAdminUser, "password");
                User_AddUpdate(newAdminUser);
            }
        }

        /// <summary>
        /// Gets a value from a table. if not found, returns null;
        /// </summary>
        /// <returns></returns>
        private static string _runValueQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, string>> parms)
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

        private static void _runNonQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, string>> parms)
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

        private static void _system_GetAll(ref SQLiteConnection conn, qryGetAllConfigValues query)
        {
            if (!_verifyConnection(ref conn))
            {
                query.Abort();
                return;
            }
            List<tblSystem> results = new List<tblSystem>();
            {
                using (var command = conn.CreateCommand())
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
            query.SetResult(results);
        }
        
        private static void _system_GetValue(ref SQLiteConnection conn, qryGetConfigValue query)
        {
            if (!_verifyConnection(ref conn))
            {
                query.Abort();
                return;
            }
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("$Category", query.Category));
            parms.Add(new KeyValuePair<string, string>("$Setting", query.Setting));
            query.SetResult(_runValueQuery(conn, SQLiteStrings.System_Select, parms));
        }
        
        private static void _system_AddUpdateValue(ref SQLiteConnection conn, qrySetConfigValue query)
        {
            if (!_verifyConnection(ref conn))
            {
                query.Abort();
                return;
            }
            var parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("$Category", query.Category));
            parms.Add(new KeyValuePair<string, string>("$Setting", query.Setting));
            parms.Add(new KeyValuePair<string, string>("$Value", query.Value));
            _runNonQuery(conn, SQLiteStrings.System_Insert, parms);
            query.SetResult(true);
        }

        private static void _user_GetAll(ref SQLiteConnection conn, qryGetAllUsers query)
        {
            if (!_verifyConnection(ref conn))
            {
                query.Abort();
                return;
            }
            List<tblUser> results = new List<tblUser>();
            using (var command = conn.CreateCommand())
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
            query.SetResult(results);
        }

        private static void _user_GetByID(ref SQLiteConnection conn, qryGetUserByID query)
        {
            tblUser dbUser = null;
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.User_GetByID;
                command.Parameters.AddWithValue("$UserID", query.UserID);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dbUser = new tblUser(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7));
                    }
                }
            }
            query.SetResult(dbUser);
        }

        private static tblUser _user_GetByEmail(ref SQLiteConnection conn, string email)
        {
            tblUser result = null;
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.User_GetByEmail;
                command.Parameters.AddWithValue("$Email", email);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new tblUser(reader.GetInt64(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6), reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7));
                    }
                }
            }
            return result;
        }

        private static void _user_GetByEmail(ref SQLiteConnection conn, qryGetUserByEmail query)
        {
            query.SetResult(_user_GetByEmail(ref conn, query.Email));
        }

        private static void _user_GetByEmailPassword(ref SQLiteConnection conn, qryGetUserByEmailPassword query)
        {
            tblUser user = _user_GetByEmail(ref conn, query.Email);
            if (user == null)
            {
                query.SetResult(null);
            }
            if (user.Enabled && ValidatePasswordHash(user, query.Password))
            {
                query.SetResult(user);
            }
            else
            {
                query.SetResult(null);
            }
        }

        private static void _user_AddUpdate(ref SQLiteConnection conn, qrySetUser query)
        {
            try
            {
                // if the UserID is populated, then we are going to try to update first. 
                // the update might fail, in which case we insert below
                if (query.User.UserID.HasValue)
                {
                    // update
                    tblUser dbUser = null;
                    using (var s = new SQLiteConnection(DatabaseConnectionString))
                    {
                        s.Open();
                        using (var command = s.CreateCommand())
                        {
                            command.CommandText = SQLiteStrings.User_GetByID;
                            command.Parameters.AddWithValue("$UserID", query.User.UserID);
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
                            query.User.UserID = null;
                        }
                        else
                        {
                            using (var command = s.CreateCommand())
                            {
                                command.CommandText = SQLiteStrings.User_Update;
                                //@"UPDATE User SET DisplayName = $DisplayName, Email = $Email, Salt = $Salt, PassHash = $PassHash, Enabled = $Enabled, Admin = $Admin WHERE UserID = $UserID;"
                                command.Parameters.AddWithValue("$DisplayName", query.User.DisplayName);
                                command.Parameters.AddWithValue("$Email", query.User.Email);
                                command.Parameters.AddWithValue("$Salt", query.User.Salt);
                                command.Parameters.AddWithValue("$PassHash", query.User.PassHash);
                                command.Parameters.AddWithValue("$Enabled", query.User.EnabledInt);
                                command.Parameters.AddWithValue("$Admin", query.User.AdminInt);
                                if (query.User.MailGateway.HasValue)
                                {
                                    command.Parameters.AddWithValue("$MailGatewayID", query.User.MailGateway);
                                }
                                else
                                {
                                    command.Parameters.AddWithValue("$MailGatewayID", DBNull.Value);
                                }
                                command.Parameters.AddWithValue("$UserID", query.User.UserID);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // if there is no UserID, then we insert a new record and select the ID back.
                if (!query.User.UserID.HasValue)
                {
                    // insert new record and read back the ID
                    using (var s = new SQLiteConnection(DatabaseConnectionString))
                    {
                        s.Open();
                        using (var command = s.CreateCommand())
                        {
                            command.CommandText = SQLiteStrings.User_Insert;
                            //@"INSERT INTO User(DisplayName, Email, Salt, PassHash, Enabled, Admin) VALUES ($DisplayName, $Email, $Salt, $PassHash, $Enabled, $Admin);"
                            command.Parameters.AddWithValue("$DisplayName", query.User.DisplayName);
                            command.Parameters.AddWithValue("$Email", query.User.Email);
                            command.Parameters.AddWithValue("$Salt", query.User.Salt);
                            command.Parameters.AddWithValue("$PassHash", query.User.PassHash);
                            command.Parameters.AddWithValue("$Enabled", query.User.EnabledInt);
                            command.Parameters.AddWithValue("$Admin", query.User.AdminInt);
                            if (query.User.MailGateway.HasValue)
                            {
                                command.Parameters.AddWithValue("$MailGatewayID", query.User.MailGateway);
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
                            query.User.UserID = (long)command.ExecuteScalar();
                        }
                    }
                }
                query.SetResult(true);
            }
            catch (Exception ex)
            {
                query.SetResult(false);
                throw ex;
            }
        }

        #endregion

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

        public static void EnvelopeRcpt_Insert(tblEnvelopeRcpt envrcpt)
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.EnvelopeRcpt_Insert;
                    command.Parameters.AddWithValue("$EnvelopeID", envrcpt.EnvelopeID);
                    command.Parameters.AddWithValue("$Recipient", envrcpt.Recipient);
                    command.ExecuteNonQuery();
                }
                using (var command = s.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.Table_LastRowID;
                    envrcpt.EnvelopeRcptID = (long)command.ExecuteScalar();
                }
            }
        }
    }
}
