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
        public static bool ConnectionInitialized = false;

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
            long queryCount = 0;
            try
            {
                while (!Worker.CancellationPending)
                {
                    DatabaseQuery query;
                    if (queue.TryTake(out query, 2000))
                    {
                        try
                        {
                            if (query is DatabaseInit)
                            {
                                _initDatabase(ref conn, query as DatabaseInit);
                            }
                            else
                            {
                                if (!_verifyConnection(ref conn))
                                {
                                    query.Abort();
                                }
                                queryCount++;
                                System.Diagnostics.Debug.WriteLine(string.Format("Query Count: {0}", queryCount));
                                switch (query)
                                {
                                    case qryGetAllConfigValues q:
                                        _system_GetAll(conn, q);
                                        break;
                                    case qryGetConfigValue q:
                                        _system_GetValue(conn, q);
                                        break;
                                    case qrySetConfigValue q:
                                        _system_AddUpdateValue(conn, q);
                                        break;
                                    case qryGetAllUsers q:
                                        _user_GetAll(conn, q);
                                        break;
                                    case qryGetUserByID q:
                                        _user_GetByID(conn, q);
                                        break;
                                    case qryGetUserByEmail q:
                                        _user_GetByEmail(conn, q);
                                        break;
                                    case qryGetUserByEmailPassword q:
                                        _user_GetByEmailPassword(conn, q);
                                        break;
                                    case qrySetUser q:
                                        _user_AddUpdate(conn, q);
                                        break;
                                    case qryClearUserGatewayByID q:
                                        _user_ClearGatewayByID(conn, q);
                                        break;
                                    case qryDeleteUserByID q:
                                        _user_DeleteByID(conn, q);
                                        break;
                                    case qryGetAllEnvelopes q:
                                        _envelope_GetAll(conn, q);
                                        break;
                                    case qryGetEnvelopeByID q:
                                        _envelope_GetByID(conn, q);
                                        break;
                                    case qrySetEnvelope q:
                                        _envelope_Add(conn, q);
                                        break;
                                    case qrySetEnvelopeChunkCount q:
                                        _envelope_UpdateChunkCount(conn, q);
                                        break;
                                    case qryGetAllMailGateways q:
                                        _mailGateway_GetAll(conn, q);
                                        break;
                                    case qryGetMailGatewayByID q:
                                        _mailGateway_GetByID(conn, q);
                                        break;
                                    case qrySetMailGateway q:
                                        _mailGateway_AddUpdate(conn, q);
                                        break;
                                    case qryDeleteMailGatwayByID q:
                                        _mailGateway_DeleteByID(conn, q);
                                        break;
                                    case qryViewMailQueue q:
                                        _mailQueue_QueryView(conn, q);
                                        break;
                                    case qryGetMailChunkData q:
                                        _mailChunks_GetChunk(conn, q);
                                        break;
                                    case qrySetMailChunk q:
                                        _mailChunk_AddChunk(conn, q);
                                        break;
                                    case qryDeleteMailChunkData q:
                                        _mailChunk_DeleteMailData(conn, q);
                                        break;
                                    case qryGetMailDataSize q:
                                        _mailChunk_GetMailSize(conn, q);
                                        break;
                                    case qryGetAllSendQueue q:
                                        _sendQueue_GetAll(conn, q);
                                        break;
                                    case qryGetReadySendQueue q:
                                        _sendQueue_GetReady(conn, q);
                                        break;
                                    case qrySetSendQueue q:
                                        _sendQueue_AddUpdate(conn, q);
                                        break;
                                    case qryDeleteSendQueueByID q:
                                        _sendQueue_DeleteByID(conn, q);
                                        break;
                                    case qryGetSendLogPage q:
                                        _sendLog_GetPage(conn, q);
                                        break;
                                    case qrySetSendLog q:
                                        _sendLog_Insert(conn, q);
                                        break;
                                    case qryGetEnvelopeRcptByID q:
                                        _envelopeRcpt_GetByID(conn, q);
                                        break;
                                    case qryGetEnvelopeRcptByEnvelopeID q:
                                        _envelopeRcpt_GetByEnvelopeID(conn, q);
                                        break;
                                    case qrySetEnvelopeRcpt q:
                                        _envelopeRcpt_Insert(conn, q);
                                        break;
                                    case qryGetAllIPEndpoints q:
                                        _ipendpoint_GetAll(conn, q);
                                        break;
                                    case qryGetIPEndpointByID q:
                                        _ipendpoint_GetByID(conn, q);
                                        break;
                                    case qrySetIPEndpoint q:
                                        _ipendpoint_AddUpdate(conn, q);
                                        break;
                                    case qryDeleteIPEndpointByID q:
                                        _ipendpoint_DeleteByID(conn, q);
                                        break;
                                    default:
                                        throw new Exception(string.Format("Unsupported object type: {0}", query.GetType().ToString()));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(string.Format("Error processing query: {0}", ex.Message));
                            query.Abort();
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

        private const string COMPATIBLE_DATABASE_VERSION = "1.1";

        private static string DBPathOverride = null;

        private static string DatabasePath
        {
            get
            {
                string progdata;
                if (string.IsNullOrEmpty(DBPathOverride))
                {
                    progdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    progdata = System.IO.Path.Combine(progdata, "SMTPRelay");
                }
                else
                {
                    progdata = System.IO.Path.GetDirectoryName(DBPathOverride);
                }
                return progdata;
            }
        }

        private static string DatabaseFile
        {
            get
            {
                string filePath;
                if (string.IsNullOrEmpty(DBPathOverride))
                {
                    filePath = System.IO.Path.Combine(DatabasePath, "config.db");
                }
                else
                {
                    filePath = DBPathOverride;
                }
                return filePath;
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
        /// Generates and sets the salt and password hash on a user for a given password.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPassword"></param>
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

        /// <summary>
        /// Compute a password hash for a provided password and verify that it matches the expected value
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates a random string of letters and numbers.
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets up the connection to the database. Will create a new database if one doesn't exist already.
        /// </summary>
        /// <returns></returns>
        public static WorkerReport InitDatabase(string dbPath = null)
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
            DatabaseInit q = new DatabaseInit(dbPath);
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
        
        /// <summary>
        /// Clear the gateway ID for all users that have the specified gateway ID assigned.
        /// </summary>
        /// <param name="gatewayID"></param>
        /// <returns></returns>
        public static bool User_ClearGatewayByID(long gatewayID)
        {
            qryClearUserGatewayByID q = new qryClearUserGatewayByID(gatewayID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Deletes a user from the database specified by UserID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static bool User_DeleteByID(long userID)
        {
            qryDeleteUserByID q = new qryDeleteUserByID(userID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a full list all envelopes.
        /// </summary>
        /// <returns></returns>
        public static List<tblEnvelope> Envelope_GetAll()
        {
            qryGetAllEnvelopes q = new qryGetAllEnvelopes();
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets an envelope by ID.
        /// </summary>
        /// <param name="envelopeID"></param>
        /// <returns></returns>
        public static tblEnvelope Envelope_GetByID(long envelopeID)
        {
            qryGetEnvelopeByID q = new qryGetEnvelopeByID(envelopeID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Adds an envelope to the database
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static bool Envelope_Add(tblEnvelope envelope)
        {
            qrySetEnvelope q = new qrySetEnvelope(envelope);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Update the chunk count for an existing envelope.
        /// </summary>
        /// <param name="envelopeID"></param>
        /// <param name="chunkCount"></param>
        public static bool Envelope_UpdateChunkCount(long envelopeID, int chunkCount)
        {
            qrySetEnvelopeChunkCount q = new qrySetEnvelopeChunkCount(envelopeID, chunkCount);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a list of all the mail gateways
        /// </summary>
        /// <returns></returns>
        public static List<tblMailGateway> MailGateway_GetAll()
        {
            qryGetAllMailGateways q = new qryGetAllMailGateways();
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a mail gateway by ID.
        /// </summary>
        /// <param name="mailGatewayID"></param>
        /// <returns></returns>
        public static tblMailGateway MailGateway_GetByID(long mailGatewayID)
        {
            qryGetMailGatewayByID q = new qryGetMailGatewayByID(mailGatewayID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Adds or updates an existing Mail Gateway
        /// </summary>
        /// <param name="mailGateway"></param>
        /// <returns></returns>
        public static bool MailGateway_AddUpdate(tblMailGateway mailGateway)
        {
            qrySetMailGateway q = new qrySetMailGateway(mailGateway);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Deletes a specified Mail Gateway
        /// </summary>
        /// <param name="mailGatewayID"></param>
        /// <returns></returns>
        public static bool MailGateway_DeleteByID(long mailGatewayID)
        {
            qryDeleteMailGatwayByID q = new qryDeleteMailGatwayByID(mailGatewayID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Retreive the view of mail queue items.
        /// </summary>
        /// <returns></returns>
        public static List<vwMailQueue> MailQueue_QueryView()
        {
            qryViewMailQueue q = new qryViewMailQueue();
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Get the byte data for a mail chunk
        /// </summary>
        /// <param name="envelopeID"></param>
        /// <param name="chunkID"></param>
        /// <returns></returns>
        public static byte[] MailChunk_GetChunk(long envelopeID, long chunkID)
        {
            qryGetMailChunkData q = new qryGetMailChunkData(envelopeID, chunkID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Add a chunk of mail data
        /// </summary>
        /// <param name="envelopeID"></param>
        /// <param name="chunkID"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool MailChunk_AddChunk(long envelopeID, int chunkID, byte[] buffer)
        {
            qrySetMailChunk q = new qrySetMailChunk(envelopeID, chunkID, buffer);
            QueryQueue.Add(q);
            return q.GetResult();
        }
        
        /// <summary>
        /// Delete all mail chunk data for a specified envelope
        /// </summary>
        /// <param name="envelopeID"></param>
        /// <returns></returns>
        public static bool MailChunk_DeleteMailData(long envelopeID)
        {
            qryDeleteMailChunkData q = new qryDeleteMailChunkData(envelopeID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets total size of mail data for the envelope
        /// </summary>
        /// <param name="envelopeID"></param>
        /// <returns></returns>
        public static long MailChunk_GetMailSize(long envelopeID)
        {
            qryGetMailDataSize q = new qryGetMailDataSize(envelopeID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a list of all queued mail items.
        /// </summary>
        /// <returns></returns>
        public static List<tblSendQueue> SendQueue_GetAll()
        {
            qryGetAllSendQueue q = new qryGetAllSendQueue();
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a list of all ready queued mail items.
        /// </summary>
        /// <returns></returns>
        public static List<tblSendQueue> SendQueue_GetReady()
        {
            qryGetReadySendQueue q = new qryGetReadySendQueue();
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Adds an item to the send queue.
        /// </summary>
        /// <param name="sendqueue"></param>
        public static bool SendQueue_AddUpdate(tblSendQueue sendqueue)
        {
            qrySetSendQueue q = new qrySetSendQueue(sendqueue);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Deletes a send queue by ID
        /// </summary>
        /// <param name="sendQueueID"></param>
        public static bool SendQueue_DeleteByID(long sendQueueID)
        {
            qryDeleteSendQueueByID q = new qryDeleteSendQueueByID(sendQueueID);
            QueryQueue.Add(q);
            return q.GetResult();
        }
        
        /// <summary>
        /// Get a page of SendLog.
        /// </summary>
        /// <param name="count">How many rows to return</param>
        /// <param name="offset">zero-based index for first row to return</param>
        /// <returns></returns>
        public static List<tblSendLog> SendLog_GetPage(long count, long offset)
        {
            qryGetSendLogPage q = new qryGetSendLogPage(count, offset);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Adds a record to the send log.
        /// The ID of the inserted row is not returned.
        /// </summary>
        /// <param name="sendlog"></param>
        /// <returns></returns>
        public static bool SendLog_Insert(tblSendLog sendlog)
        {
            qrySetSendLog q = new qrySetSendLog(sendlog);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Inserts an Envelope Recipient
        /// </summary>
        /// <param name="envrcpt"></param>
        /// <returns></returns>
        public static bool EnvelopeRcpt_Insert(tblEnvelopeRcpt envrcpt)
        {
            qrySetEnvelopeRcpt q = new qrySetEnvelopeRcpt(envrcpt);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        /// <summary>
        /// Gets a list of Envelope Recipients by Envelope ID
        /// </summary>
        /// <param name="envelopeID"></param>
        /// <returns></returns>
        public static List<tblEnvelopeRcpt> EnvelopeRcpt_GetByEnvelopeID(long envelopeID)
        {
            qryGetEnvelopeRcptByEnvelopeID q = new qryGetEnvelopeRcptByEnvelopeID(envelopeID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        public static tblEnvelopeRcpt EnvelopeRcpt_GetByID(long envelopeRcptID)
        {
            qryGetEnvelopeRcptByID q = new qryGetEnvelopeRcptByID(envelopeRcptID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        public static List<tblIPEndpoint> IPEndpoint_GetAll()
        {
            qryGetAllIPEndpoints q = new qryGetAllIPEndpoints();
            QueryQueue.Add(q);
            return q.GetResult();
        }

        public static tblIPEndpoint IPEndpoint_GetByID(long ipendpointID)
        {
            qryGetIPEndpointByID q = new qryGetIPEndpointByID(ipendpointID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        public static bool IPEndpoint_AddUpdate(tblIPEndpoint ipendpoint)
        {
            qrySetIPEndpoint q = new qrySetIPEndpoint(ipendpoint);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        public static bool IPEndpoint_DeleteByID(long ipendpointID)
        {
            qryDeleteIPEndpointByID q = new qryDeleteIPEndpointByID(ipendpointID);
            QueryQueue.Add(q);
            return q.GetResult();
        }

        #endregion

        #region Private Methods
        private static bool _verifyConnection(ref SQLiteConnection conn)
        {
            if (!ConnectionInitialized)
            {
                return false;
            }
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
                    ConnectionInitialized = false;
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
            ConnectionInitialized = false;
            DBPathOverride = query.DBPath;
            _cached_DatabaseConnectionString = null;
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
                ConnectionInitialized = true;
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
                qrySetUser q = new qrySetUser(newAdminUser);
                _user_AddUpdate(s, q);
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

        private static void _system_GetAll(SQLiteConnection conn, qryGetAllConfigValues query)
        {
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
        
        private static void _system_GetValue(SQLiteConnection conn, qryGetConfigValue query)
        {
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("$Category", query.Category));
            parms.Add(new KeyValuePair<string, string>("$Setting", query.Setting));
            query.SetResult(_runValueQuery(conn, SQLiteStrings.System_Select, parms));
        }
        
        private static void _system_AddUpdateValue(SQLiteConnection conn, qrySetConfigValue query)
        {
            var parms = new List<KeyValuePair<string, string>>();
            parms.Add(new KeyValuePair<string, string>("$Category", query.Category));
            parms.Add(new KeyValuePair<string, string>("$Setting", query.Setting));
            parms.Add(new KeyValuePair<string, string>("$Value", query.Value));
            _runNonQuery(conn, SQLiteStrings.System_Insert, parms);
            query.SetResult(true);
        }

        private static void _user_GetAll(SQLiteConnection conn, qryGetAllUsers query)
        {
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

        private static void _user_GetByID(SQLiteConnection conn, qryGetUserByID query)
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

        private static tblUser _user_GetByEmail(SQLiteConnection conn, string email)
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

        private static void _user_GetByEmail(SQLiteConnection conn, qryGetUserByEmail query)
        {
            query.SetResult(_user_GetByEmail(conn, query.Email));
        }

        private static void _user_GetByEmailPassword(SQLiteConnection conn, qryGetUserByEmailPassword query)
        {
            tblUser user = _user_GetByEmail(conn, query.Email);
            if (user == null)
            {
                query.SetResult(null);
                return;
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

        private static void _user_AddUpdate(SQLiteConnection conn, qrySetUser query)
        {
            try
            {
                // if the UserID is populated, then we are going to try to update first. 
                // the update might fail, in which case we insert below
                if (query.User.UserID.HasValue)
                {
                    // update
                    tblUser dbUser = null;
                    using (var command = conn.CreateCommand())
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
                        using (var command = conn.CreateCommand())
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

                // if there is no UserID, then we insert a new record and select the ID back.
                if (!query.User.UserID.HasValue)
                {
                    // insert new record and read back the ID
                    using (var command = conn.CreateCommand())
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
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.Table_LastRowID;
                        query.User.UserID = (long)command.ExecuteScalar();
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

        private static void _user_ClearGatewayByID(SQLiteConnection conn, qryClearUserGatewayByID query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.User_ClearGatewayByID;
                command.Parameters.AddWithValue("$MailGatewayID", query.GatewayID);
                command.ExecuteNonQuery();
            }
            query.SetResult(true);
        }

        private static void _user_DeleteByID(SQLiteConnection conn, qryDeleteUserByID query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.User_DeleteByID;
                command.Parameters.AddWithValue("$UserID", query.UserID);
                command.ExecuteNonQuery();
            }
            query.SetResult(true);
        }

        private static void _envelope_GetAll(SQLiteConnection conn, qryGetAllEnvelopes query)
        {
            List<tblEnvelope> results = new List<tblEnvelope>();
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.Envelope_GetAll;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new tblEnvelope(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5), reader.GetString(6)));
                    }
                }
            }
            query.SetResult(results);
        }

        private static void _envelope_GetByID(SQLiteConnection conn, qryGetEnvelopeByID query)
        {
            tblEnvelope result = null;
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.Envelope_GetByID;
                command.Parameters.AddWithValue("$EnvelopeID", query.EnvelopeID);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new tblEnvelope(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetInt32(5), reader.GetString(6));
                    }
                }
            }
            query.SetResult(result);
        }

        private static void _envelope_Add(SQLiteConnection conn, qrySetEnvelope query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.Envelope_Insert;
                command.Parameters.AddWithValue("$UserID", query.Envelope.UserID);
                command.Parameters.AddWithValue("$WhenReceived", query.Envelope.WhenReceivedString);
                command.Parameters.AddWithValue("$Sender", query.Envelope.Sender);
                command.Parameters.AddWithValue("$Recipients", query.Envelope.Recipients);
                command.Parameters.AddWithValue("$ChunkCount", query.Envelope.ChunkCount);
                command.Parameters.AddWithValue("$MsgID", query.Envelope.MsgID);
                command.ExecuteNonQuery();
            }
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.Table_LastRowID;
                query.Envelope.EnvelopeID = (long)command.ExecuteScalar();
            }
            query.SetResult(true);
        }

        private static void _envelope_UpdateChunkCount(SQLiteConnection conn, qrySetEnvelopeChunkCount query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.Envelope_UpdateChunkCount;
                command.Parameters.AddWithValue("$EnvelopeID", query.EnvelopeID);
                command.Parameters.AddWithValue("$ChunkCount", query.ChunkCount);
                command.ExecuteNonQuery();
            }
            query.SetResult(true);
        }

        private static void _mailGateway_GetAll(SQLiteConnection conn, qryGetAllMailGateways query)
        {
            List<tblMailGateway> results = new List<tblMailGateway>();
            using (var command = conn.CreateCommand())
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
            query.SetResult(results);
        }

        private static void _mailGateway_GetByID(SQLiteConnection conn, qryGetMailGatewayByID query)
        {
            tblMailGateway results = null;
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.MailGateway_GetByID;
                command.Parameters.AddWithValue("$MailGatewayID", query.MailGatewayID);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        results = new tblMailGateway(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetString(5), reader.GetString(6), reader.GetString(7));
                    }
                }
            }
            query.SetResult(results);
        }

        private static void _mailGateway_AddUpdate(SQLiteConnection conn, qrySetMailGateway query)
        {
            // if the MailGatewayID is populated, then we are going to try to update first. 
            // the update might fail, in which case we insert below
            if (query.MailGateway.MailGatewayID.HasValue)
            {
                // update. First, read the existing record by ID to make sure it exists. 
                tblMailGateway dbMailGateway = null;

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.MailGateway_GetByID;
                    command.Parameters.AddWithValue("$MailGatewayID", query.MailGateway.MailGatewayID);
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
                    query.MailGateway.MailGatewayID = null;
                }
                else
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.MailGateway_Update;
                        command.Parameters.AddWithValue("$SMTPServer", query.MailGateway.SMTPServer);
                        command.Parameters.AddWithValue("$Port", query.MailGateway.Port);
                        command.Parameters.AddWithValue("$EnableSSL", query.MailGateway.EnableSSLInt);
                        command.Parameters.AddWithValue("$Authenticate", query.MailGateway.AuthenticateInt);
                        command.Parameters.AddWithValue("$Username", query.MailGateway.Username);
                        command.Parameters.AddWithValue("$Password", query.MailGateway.Password);
                        command.Parameters.AddWithValue("$SenderOverride", query.MailGateway.SenderOverride);
                        command.Parameters.AddWithValue("$MailGatewayID", query.MailGateway.MailGatewayID);
                        command.ExecuteNonQuery();
                    }
                }
                
            }

            // if there is no MailGatewayID, then we insert a new record and select the ID back.
            if (!query.MailGateway.MailGatewayID.HasValue)
            {
                // insert new record and read back the ID
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.MailGateway_Insert;
                    command.Parameters.AddWithValue("$SMTPServer", query.MailGateway.SMTPServer);
                    command.Parameters.AddWithValue("$Port", query.MailGateway.Port);
                    command.Parameters.AddWithValue("$EnableSSL", query.MailGateway.EnableSSLInt);
                    command.Parameters.AddWithValue("$Authenticate", query.MailGateway.AuthenticateInt);
                    command.Parameters.AddWithValue("$Username", query.MailGateway.Username);
                    command.Parameters.AddWithValue("$Password", query.MailGateway.Password);
                    command.Parameters.AddWithValue("$SenderOverride", query.MailGateway.SenderOverride);
                    command.ExecuteNonQuery();
                }
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.Table_LastRowID;
                    query.MailGateway.MailGatewayID = (long)command.ExecuteScalar();
                }
            }
            query.SetResult(true);
        }

        private static void _mailGateway_DeleteByID(SQLiteConnection conn, qryDeleteMailGatwayByID query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.MailGateway_DeleteByID;
                command.Parameters.AddWithValue("$MailGatewayID", query.MailGatewayID);
                command.ExecuteNonQuery();
            }
            query.SetResult(true);
        }

        private static void _mailQueue_QueryView(SQLiteConnection conn, qryViewMailQueue query)
        {
            List<vwMailQueue> results = new List<vwMailQueue>();
            using (var command = conn.CreateCommand())
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
            query.SetResult(results);
        }

        private static void _mailChunks_GetChunk(SQLiteConnection conn, qryGetMailChunkData query)
        {
            byte[] result = null;
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.MailChunk_GetChunk;
                command.Parameters.AddWithValue("$EnvelopeID", query.EnvelopeID);
                command.Parameters.AddWithValue("$ChunkID", query.ChunkID);
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
            query.SetResult(result);
        }

        private static void _mailChunk_AddChunk(SQLiteConnection conn, qrySetMailChunk query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.MailChunk_AddChunk;
                command.Parameters.AddWithValue("$EnvelopeID", query.EnvelopeID);
                command.Parameters.AddWithValue("$ChunkID", query.ChunkID);
                command.Parameters.Add("$Chunk", System.Data.DbType.Binary, query.Buffer.Length).Value = query.Buffer;
                command.ExecuteNonQuery();
            }
            query.SetResult(true);
        }

        private static void _mailChunk_DeleteMailData(SQLiteConnection conn, qryDeleteMailChunkData query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.MailChunk_DeleteMailData;
                command.Parameters.AddWithValue("$EnvelopeID", query.EnvelopeID);
                command.ExecuteNonQuery();
            }
            query.SetResult(true);
        }
        
        private static void _mailChunk_GetMailSize(SQLiteConnection conn, qryGetMailDataSize query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.MailChunk_GetMailSize;
                command.Parameters.AddWithValue("$EnvelopeID", query.EnvelopeID);
                query.SetResult((long)command.ExecuteScalar());
            }
        }

        private static void _sendQueue_GetAll(SQLiteConnection conn, qryGetAllSendQueue query)
        {
            List<tblSendQueue> results = new List<tblSendQueue>();
            using (var command = conn.CreateCommand())
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
            query.SetResult(results);
        }

        private static void _sendQueue_GetReady(SQLiteConnection conn, qryGetReadySendQueue query)
        {
            List<tblSendQueue> results = new List<tblSendQueue>();
            using (var command = conn.CreateCommand())
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
            query.SetResult(results);
        }

        private static void _sendQueue_AddUpdate(SQLiteConnection conn, qrySetSendQueue query)
        {
            // if the SendQueuID is populated, then we are going to try to update first. 
            // the update might fail, in which case we insert below
            if (query.SendQueue.SendQueueID.HasValue)
            {
                // update. First, read the existing record by ID to make sure it exists. 
                tblSendQueue dbsendqueue = null;
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.SendQueue_GetByID;
                    command.Parameters.AddWithValue("$SendQueueID", query.SendQueue.SendQueueID);
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
                    query.SendQueue.SendQueueID = null;
                }
                else
                {
                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = SQLiteStrings.SendQueue_Update;
                        command.Parameters.AddWithValue("$State", query.SendQueue.StateInt);
                        command.Parameters.AddWithValue("$AttemptCount", query.SendQueue.AttemptCount);
                        command.Parameters.AddWithValue("$RetryAfter", query.SendQueue.RetryAfterStr);
                        command.Parameters.AddWithValue("$SendQueueID", query.SendQueue.SendQueueID);
                        command.ExecuteNonQuery();
                    }
                }
                
            }

            // if there is no SendQueueID, then we insert a new record and select the ID back.
            if (!query.SendQueue.SendQueueID.HasValue)
            {
                // insert new record and read back the ID
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.SendQueue_Insert;
                    command.Parameters.AddWithValue("$EnvelopeID", query.SendQueue.EnvelopeID);
                    command.Parameters.AddWithValue("$EnvelopeRcptID", query.SendQueue.EnvelopeRcptID);
                    command.Parameters.AddWithValue("$State", query.SendQueue.StateInt);
                    command.Parameters.AddWithValue("$AttemptCount", query.SendQueue.AttemptCount);
                    command.Parameters.AddWithValue("$RetryAfter", query.SendQueue.RetryAfterStr);
                    command.ExecuteNonQuery();
                }
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.Table_LastRowID;
                    query.SendQueue.SendQueueID = (long)command.ExecuteScalar();
                }
            }
            query.SetResult(true);
        }

        private static void _sendQueue_DeleteByID(SQLiteConnection conn, qryDeleteSendQueueByID query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.SendQueue_DeleteByID;
                command.Parameters.AddWithValue("$SendQueueID", query.SendQueueID);
                command.ExecuteNonQuery();
            }
            query.SetResult(true);
        }

        private static void _sendLog_GetPage(SQLiteConnection conn, qryGetSendLogPage query)
        {
            List<tblSendLog> results = new List<tblSendLog>();

            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.SendLog_GetPage;
                command.Parameters.AddWithValue("$RowCount", query.Count);
                command.Parameters.AddWithValue("$RowStart", query.Offset);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new tblSendLog(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2), reader.GetString(3), reader.GetInt32(4)));
                    }
                }
            }
            query.SetResult(results);
        }

        private static void _sendLog_Insert(SQLiteConnection conn, qrySetSendLog query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.SendLog_Insert;
                command.Parameters.AddWithValue("$EnvelopeID", query.SendLog.EnvelopeID);
                command.Parameters.AddWithValue("$EnvelopeRcptID", query.SendLog.EnvelopeRcptID);
                command.Parameters.AddWithValue("$WhenAttempted", query.SendLog.WhenAttemptedStr);
                command.Parameters.AddWithValue("$Results", query.SendLog.Results);
                command.Parameters.AddWithValue("$AttemptCount", query.SendLog.AttemptCount);
                command.ExecuteNonQuery();
            }
            query.SetResult(true);
        }

        private static void _envelopeRcpt_GetByEnvelopeID(SQLiteConnection conn, qryGetEnvelopeRcptByEnvelopeID query)
        {
            List<tblEnvelopeRcpt> results = new List<tblEnvelopeRcpt>();
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.EnvelopeRcpt_GetByEnvelopeID;
                command.Parameters.AddWithValue("$EnvelopeID", query.EnvelopeID);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //EnvelopeRcptID, EnvelopeID, Recipient
                        results.Add(new tblEnvelopeRcpt(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2)));
                    }
                }
            }
            query.SetResult(results);
        }

        private static void _envelopeRcpt_GetByID(SQLiteConnection conn, qryGetEnvelopeRcptByID query)
        {
            tblEnvelopeRcpt result = null;
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.EnvelopeRcpt_GetByID;
                command.Parameters.AddWithValue("$EnvelopeRcptID", query.EnvelopeRcptID);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        //EnvelopeRcptID, EnvelopeID, Recipient
                        result = new tblEnvelopeRcpt(reader.GetInt64(0), reader.GetInt64(1), reader.GetString(2));
                    }
                }
            }
            query.SetResult(result);
        }

        private static void _envelopeRcpt_Insert(SQLiteConnection conn, qrySetEnvelopeRcpt query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.EnvelopeRcpt_Insert;
                command.Parameters.AddWithValue("$EnvelopeID", query.EnvelopeRcpt.EnvelopeID);
                command.Parameters.AddWithValue("$Recipient", query.EnvelopeRcpt.Recipient);
                command.ExecuteNonQuery();
            }
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.Table_LastRowID;
                query.EnvelopeRcpt.EnvelopeRcptID = (long)command.ExecuteScalar();
            }
            query.SetResult(true);
        }

        private static void _ipendpoint_GetAll(SQLiteConnection conn, qryGetAllIPEndpoints query)
        {
            List<tblIPEndpoint> results = new List<tblIPEndpoint>();
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.IPEndpoint_GetAll;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new tblIPEndpoint(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3), reader.GetInt32(4), reader.GetString(5)));
                    }
                }
            }
            query.SetResult(results);
        }

        private static void _ipendpoint_GetByID(SQLiteConnection conn, qryGetIPEndpointByID query)
        {
            tblIPEndpoint result = null;
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.IPEndpoint_GetByID;
                command.Parameters.AddWithValue("$IPEndpointID", query.IPEndpointID);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        //EnvelopeRcptID, EnvelopeID, Recipient
                        result = new tblIPEndpoint(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3), reader.GetInt32(4), reader.GetString(5));
                    }
                }
            }
            query.SetResult(result);
        }

        private static void _ipendpoint_AddUpdate(SQLiteConnection conn, qrySetIPEndpoint query)
        {
            // if the IPEndpointID is populated, then we are going to try to update first. 
            // the update might fail, in which case we insert below
            if (query.IPEndpoint.IPEndpointID.HasValue)
            {
                // update. First, read the existing record by ID to make sure it exists. 
                tblIPEndpoint dbipendpoint = null;
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.IPEndpoint_GetByID;
                    command.Parameters.AddWithValue("$IPEndpointID", query.IPEndpoint.IPEndpointID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dbipendpoint = new tblIPEndpoint(reader.GetInt64(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3), reader.GetInt32(4), reader.GetString(5));
                        }
                    }
                }
                if (dbipendpoint == null)
                {
                    // IPEndpoint doesn't exit, so below we will insert it.
                    query.IPEndpoint.IPEndpointID = null;
                }
                else
                {
                    using (var command = conn.CreateCommand())
                    {
                        //$Address, $Port, $Protocol, $TLSMode, $CertFriendlyName
                        command.CommandText = SQLiteStrings.IPEndpoint_Update;
                        command.Parameters.AddWithValue("$Address", query.IPEndpoint.Address);
                        command.Parameters.AddWithValue("$Port", query.IPEndpoint.Port);
                        command.Parameters.AddWithValue("$Protocol", query.IPEndpoint.ProtocolString);
                        command.Parameters.AddWithValue("$TLSMode", query.IPEndpoint.TLSModeInt);
                        command.Parameters.AddWithValue("$CertFriendlyName", query.IPEndpoint.CertFriendlyName);
                        command.Parameters.AddWithValue("$IPEndpointID", query.IPEndpoint.IPEndpointID);
                        command.ExecuteNonQuery();
                    }
                }

            }

            // if there is no IPEndpointID, then we insert a new record and select the ID back.
            if (!query.IPEndpoint.IPEndpointID.HasValue)
            {
                // insert new record and read back the ID
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.IPEndpoint_Insert;
                    command.Parameters.AddWithValue("$Address", query.IPEndpoint.Address);
                    command.Parameters.AddWithValue("$Port", query.IPEndpoint.Port);
                    command.Parameters.AddWithValue("$Protocol", query.IPEndpoint.ProtocolString);
                    command.Parameters.AddWithValue("$TLSMode", query.IPEndpoint.TLSModeInt);
                    command.Parameters.AddWithValue("$CertFriendlyName", query.IPEndpoint.CertFriendlyName);
                    command.ExecuteNonQuery();
                }
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQLiteStrings.Table_LastRowID;
                    query.IPEndpoint.IPEndpointID = (long)command.ExecuteScalar();
                }
            }
            query.SetResult(true);
        }

        private static void _ipendpoint_DeleteByID(SQLiteConnection conn, qryDeleteIPEndpointByID query)
        {
            using (var command = conn.CreateCommand())
            {
                command.CommandText = SQLiteStrings.IPEndpoint_DeleteByID;
                command.Parameters.AddWithValue("$IPEndpointID", query.IPEndpointID);
                command.ExecuteNonQuery();
            }
            query.SetResult(true);
        }

        #endregion

    }
}
