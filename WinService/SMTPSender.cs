using SMTPRelay.Database;
using SMTPRelay.Model;
using SMTPRelay.Model.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.WinService
{
    public class SMTPSender
    {
        // hard programmed limit of 20 minutes
        private const int CONNECTIONTIMEOUT = 1200000;
        private BackgroundWorker Worker;
        public bool Running;
        public ConcurrentQueue<WorkerReport> WorkerReports;
        public long? GatewayIdInUse = null;

        public SMTPSender(tblProcessQueue sendQueueItem, long? trackGateway)
        {
            GatewayIdInUse = trackGateway;
            WorkerReports = new ConcurrentQueue<WorkerReport>();
            Running = true;
            Worker = new BackgroundWorker();
            Worker.WorkerSupportsCancellation = true;
            Worker.WorkerReportsProgress = true;
            Worker.DoWork += Worker_DoWork;
            Worker.ProgressChanged += Worker_ProgressChanged;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            Worker.RunWorkerAsync(sendQueueItem);
        }

        public void Cancel()
        {
            Worker.CancelAsync();
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is WorkerReport)
            {
                WorkerReports.Enqueue(e.UserState as WorkerReport);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is WorkerReport)
            {
                WorkerReports.Enqueue(e.Result as WorkerReport);
            }
            Running = false;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Diagnostics.Stopwatch HardTimeLimit = new System.Diagnostics.Stopwatch();
            HardTimeLimit.Start();
            System.Diagnostics.Stopwatch swTimeout = new System.Diagnostics.Stopwatch();
            swTimeout.Start();
            tblProcessQueue sendQueueItem = e.Argument as tblProcessQueue;
            if (sendQueueItem == null)
            {
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogError = "No ProcessQueue Item specified to SMTPSender."
                });
                return;
            }
            sendQueueItem.AttemptCount++;

            TcpClient client = null;
            NetworkStream stream = null;
            ISMTPStream smtpStream = null;
            string finalResults = null;
            string MsgIdentifier = string.Empty;
            bool PermanentFailure = false;

            TextWriter debugWriter = null;

            // these get flagged true to enable debugging based on the current settings in the database.
            bool Verbose = false;
            bool VerboseBody = false;

            try
            {
                // queue item is already marked as dispatched before SMTPSender is instantiated to prevent duplicate pickups.
                // retrieve settings
                string localHostname = SQLiteDB.System_GetValue("SMTPServer", "Hostname");

                // retrieve necessary message info (sender, recipient, gateway, message size)
                tblEnvelope envelope = SQLiteDB.Envelope_GetByID(sendQueueItem.EnvelopeID);
                tblEnvelopeRcpt envelopeRcpt = SQLiteDB.EnvelopeRcpt_GetByID(sendQueueItem.EnvelopeRcptID);
                tblUser user = null;
                tblDevice device = null;
                if (envelope.UserID.HasValue)
                {
                    user = SQLiteDB.User_GetByID(envelope.UserID.Value);
                }
                if (envelope.DeviceID.HasValue)
                {
                    device = SQLiteDB.Device_GetByID(envelope.DeviceID.Value);
                }
                tblMailGateway gateway = null;
                if (user != null && user.MailGateway.HasValue)
                {
                    gateway = SQLiteDB.MailGateway_GetByID(user.MailGateway.Value);
                }
                if (gateway == null && device != null && device.MailGateway.HasValue)
                {
                    gateway = SQLiteDB.MailGateway_GetByID(device.MailGateway.Value);
                }

                // Sender
                string MailFromAddress = envelope.Sender;
                // Recipient
                string RcptToAddress = envelopeRcpt.Recipient;
                bool HeaderReplaceSender = false;
                List<string> SMTPExtensions = new List<string>();

                MsgIdentifier = string.Format("Msg [{0}] from <{1}> to <{2}>. ", envelope.MsgID, envelope.Sender, envelopeRcpt.Recipient);
                
                string VerboseEnabledString = SQLiteDB.System_GetValue("SMTPSender", "VerboseDebuggingEnabled");
                if (string.IsNullOrEmpty(VerboseEnabledString) || (VerboseEnabledString != "1" && VerboseEnabledString != "0"))
                {
                    Worker.ReportProgress(0, new WorkerReport()
                    {
                        LogWarning = "Resetting default Verbose Debugging settings for SMTPSender."
                    });
                    SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingEnabled", "0");
                    SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingPath", "C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Sent\\");
                    SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingIncludeBody", "0");
                }
                else if (VerboseEnabledString == "1")
                {
                    string VerbosePathString = SQLiteDB.System_GetValue("SMTPSender", "VerboseDebuggingPath");
                    string debugpath = SQLiteDB.System_GetValue("SMTPSender", "VerboseDebuggingPath");
                    string debugbody = SQLiteDB.System_GetValue("SMTPSender", "VerboseDebuggingIncludeBody");
                    if (string.IsNullOrEmpty(debugpath))
                    {
                        Worker.ReportProgress(0, new WorkerReport()
                        {
                            LogWarning = "No SMTPSender verbose debug path specified. Using C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Sent\\"
                        });
                        debugpath = "C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Sent\\";
                        SQLiteDB.System_AddUpdateValue("SMTPSender", "VerboseDebuggingPath", "C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Sent\\");
                    }

                    bool pathGood = true;
                    if (!Directory.Exists(debugpath))
                    {
                        try
                        {
                            Directory.CreateDirectory(debugpath);
                        }
                        catch (Exception ex)
                        {
                            pathGood = false;
                            Worker.ReportProgress(0, new WorkerReport()
                            {
                                LogWarning = string.Format("Could not create the verbose debug path {0}. Exception: {1}" + debugpath, ex.Message)
                            });
                        }
                    }
                    if (pathGood)
                    {
                        string safeIdent = SanitizeName(envelope.MsgID);
                        string fname = string.Format("{0} msg {1} th {2}", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff"), safeIdent, System.Threading.Thread.CurrentThread.ManagedThreadId);
                        string filePath = Path.Combine(debugpath, fname + ".log");
                        if (File.Exists(filePath))
                        {
                            for (int collisionCount = 1; collisionCount < 1000; collisionCount++)
                            {
                                filePath = Path.Combine(debugpath, fname + " (" + collisionCount + ").log");
                                if (!File.Exists(filePath))
                                {
                                    break;
                                }
                                else
                                {
                                    filePath = null;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(filePath))
                        {
                            Worker.ReportProgress(0, new WorkerReport()
                            {
                                LogWarning = "Could not verbose debug the connection because the file exists."
                            });
                        }
                        else
                        {
                            try
                            {
                                debugWriter = new StreamWriter(filePath, false, Encoding.ASCII);
                            }
                            catch (Exception ex)
                            {
                                debugWriter = null;
                                Worker.ReportProgress(0, new WorkerReport()
                                {
                                    LogWarning = string.Format("Could not verbose debug the connection because the file couldn't be opened. Exception: {0}", ex.Message)
                                });
                            }
                        }

                        if (debugWriter != null)
                        {
                            Verbose = true;
                            if (debugbody == "1")
                            {
                                VerboseBody = true;
                            }
                        }
                    }
                }

                if (Verbose)
                {
                    debugWriter.WriteLine(string.Format("# Send email initiated for MessageID {0} from {1} to {2}. ", envelope.MsgID, envelope.Sender, envelopeRcpt.Recipient));
                    debugWriter.Flush();
                }

                // figure out how to contact the server.
                // if there's a gateway specified, look that up and connect.
                // if there's no gateway, we need to look up the MX record for the recipient's domain.
                List<IPEndPoint> serverEPs = new List<IPEndPoint>();
                string serverHostname;
                int serverPort = 25;
                bool useTLS = false;
                bool login = false;
                string username = null;
                string password = null;
                if (gateway != null)
                {
                    serverHostname = gateway.SMTPServer;
                    serverPort = gateway.Port;
                    login = gateway.Authenticate;
                    username = gateway.Username;
                    password = gateway.Password;
                    useTLS = gateway.EnableSSL;
                    if (Verbose)
                    {
                        if (login)
                        {
                            debugWriter.WriteLine("# Gateway host {0} port {1} authenticate as {2}", serverHostname, serverPort, username);
                        }
                        else
                        {
                            debugWriter.WriteLine("# Gateway host {0} port {1}", serverHostname, serverPort);
                        }
                        debugWriter.Flush();
                    }
                }
                else
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# No gateway specified, and MX Lookup not implemented. Failing item.");
                        debugWriter.Flush();
                    }
                    PermanentFailure = true;
                    throw new NotImplementedException("Looking up domain MX recoreds is not implemented.");
                }

                if (!string.IsNullOrEmpty(gateway.SenderOverride))
                {
                    HeaderReplaceSender = true;
                    MailFromAddress = gateway.SenderOverride;
                    if (Verbose)
                    {
                        debugWriter.WriteLine(string.Format("# Sender override enabled. Sending as {0}", MailFromAddress));
                        debugWriter.Flush();
                    }
                }

                // at this point, we should have either a domain name or an IP address in serverAddress.
                // first, try parsing it as an IP address. If that fails, then do a DNS lookup on it.
                IPAddress serverAddress;
                if (IPAddress.TryParse(serverHostname, out serverAddress))
                {
                    serverEPs.Add(new IPEndPoint(serverAddress, serverPort));
                }
                else
                {
                    // try a DNS lookup.
                    try
                    {
                        IPHostEntry entry = Dns.GetHostEntry(serverHostname);
                        foreach (IPAddress addr in entry.AddressList)
                        {
                            serverEPs.Add(new IPEndPoint(addr, serverPort));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine(string.Format("# Failed querying DNS for {0}. Exception: {1}", serverHostname, ex.Message));
                            debugWriter.Flush();
                        }
                        throw new Exception(string.Format("Querying DNS failed for [{0}]. {1}", serverHostname, ex.Message));
                    }
                }

                // attempt to connect to the server
                if (serverEPs.Count == 0)
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Unable to connect to SMTP server. No End Point could be resolved.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Unable to connect to SMTP server. No End Point could be resolved.");
                }

                client = null;
                foreach (var ep in serverEPs)
                {
                    try
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine(string.Format("# Initiating TCP connection to {0}:{1}", ep.Address.ToString(), ep.Port));
                            debugWriter.Flush();
                        }
                        client = new TcpClient();
                        client.Connect(ep);
                        break;
                    }
                    catch (Exception exconnect)
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine(string.Format("# Failed to connect. Exception: {0}", exconnect.Message.ToString()));
                            debugWriter.Flush();
                        }
                        client = null;
                    }
                }

                if (client == null)
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Unable to connect to SMTP Server. All server end points have been exhausted.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Unable to connect to the SMTP Server.");
                }

                stream = client.GetStream();
                smtpStream = new SMTPStreamHandler(stream);

                // negotiate the connection and switch to TLS if the gateway specifies requirements to do so.
                string line = smtpStream.ReadLine(30000, Worker);

                if (string.IsNullOrEmpty(line))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("Timeout waiting for SMTP server welcome message.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Timeout initiating connection to SMTP server.");
                }
                if (Verbose)
                {
                    debugWriter.WriteLine(String.Format("S->C: {0}", line));
                    debugWriter.Flush();
                }

                if (!line.StartsWith("220 "))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Unexpected welcome message. Aborting SMTP connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception(string.Format("Unexpected welcome message: {0}", line));
                }
                // we got a 220 hello message.
                // send the HELO message
                line = string.Format("EHLO {0}", localHostname);
                WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);

                // Get the server extensions (250-) and the OK message (250)
                bool LoopDone = false;
                while (!LoopDone)
                {
                    line = smtpStream.ReadLine(30000, Worker);
                    if (string.IsNullOrEmpty(line))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Timeout waiting for EHLO acknowledgment and server extensions response.");
                            debugWriter.Flush();
                        }
                        // we got a timeout waiting for a line back from the server
                        throw new Exception("Timeout initiating connection to SMTP server.");
                    }
                    if (Verbose)
                    {
                        debugWriter.WriteLine(String.Format("S->C: {0}", line));
                        debugWriter.Flush();
                    }
                    if (Worker.CancellationPending)
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Operation Canceled.");
                            debugWriter.Flush();
                        }
                        throw new OperationCanceledException();
                    }
                    if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Connection Timeout Expired. Aborting connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Total Process Timeout Exceeded for the connection.");
                    }
                    if (line.StartsWith("250-"))
                    {
                        SMTPExtensions.Add(line);
                    }
                    else
                    {
                        LoopDone = true;
                    }
                }

                SMTPExtensions.Add(line);  // sometimes the last message is Ok, continue, or some nice message. Often, it's the last item in the extensions list

                if (!line.StartsWith("250 "))
                {
                    // didn't get expected 250 message
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Unexpected message from SMTP server. Aborting connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception(string.Format("Server didn't reply as expected to EHLO. Got: {0}", line));
                }
                if (useTLS)
                {
                    line = "STARTTLS";
                    WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                    // Possible responses
                    // 220 Ready to start TLS
                    // 501 Syntax error (no parameters allowed)
                    // 454 TLS not available due to temporary reason
                    line = smtpStream.ReadLine(30000, Worker);
                    if (string.IsNullOrEmpty(line))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Timeout initiating TLS connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Timeout enabling TLS.");
                    }
                    if (Verbose)
                    {
                        debugWriter.WriteLine(String.Format("S->C: {0}", line));
                        debugWriter.Flush();
                    }
                    if (line.StartsWith("501 "))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Server doesn't suport TLS. Aborting connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Server doesn't support TLS.");
                    }
                    else if (line.StartsWith("454 "))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Server indicated TLS not available at this time. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("TLS not available at this time.");
                    }
                    else if (!line.StartsWith("220 "))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Expected server response to TLS request. Aborting connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception(string.Format("Unexpected server response to STARTTLS: {0}", line));
                    }
                    // switch to TLS now
                    smtpStream.Release();
                    smtpStream = null;
                    smtpStream = new SMTPTLSStreamHandler(stream, SMTPTLSStreamHandler.Mode.Client, serverHostname, null);
                    // send the HELO message. This essentially starts the conversation over.
                    line = string.Format("EHLO {0}", localHostname);
                    WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                    SMTPExtensions.Clear();  // we might get different extensions now that we're in TLS mode.

                    // Get the server extensions (250-) and the OK message (250)
                    LoopDone = false;
                    while (!LoopDone)
                    {
                        line = smtpStream.ReadLine(30000, Worker);
                        if (string.IsNullOrEmpty(line))
                        {
                            if (Verbose)
                            {
                                debugWriter.WriteLine("# Timeout waiting for EHLO acknowledgment and server extensions response.");
                                debugWriter.Flush();
                            }
                            // we got a timeout waiting for a line back from the server
                            throw new Exception("Timeout initiating connection to SMTP server.");
                        }
                        if (Verbose)
                        {
                            debugWriter.WriteLine(String.Format("S->C: {0}", line));
                            debugWriter.Flush();
                        }
                        if (Worker.CancellationPending)
                        {
                            if (Verbose)
                            {
                                debugWriter.WriteLine("# Operation Canceled.");
                                debugWriter.Flush();
                            }
                            throw new OperationCanceledException();
                        }
                        if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                        {
                            if (Verbose)
                            {
                                debugWriter.WriteLine("# Connection Timeout Expired. Aborting connection.");
                                debugWriter.Flush();
                            }
                            throw new Exception("Total Process Timeout Exceeded for the connection.");
                        }
                        if (line.StartsWith("250-"))
                        {
                            SMTPExtensions.Add(line);
                        }
                        else
                        {
                            LoopDone = true;
                        }
                    }

                    SMTPExtensions.Add(line);  // sometimes the last message is Ok, continue, or some nice message. Often, it's the last item in the extensions list

                    if (!line.StartsWith("250 "))
                    {
                        // didn't get expected 250 message
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Unexpected message from SMTP server. Aborting connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception(string.Format("Server didn't reply as expected to EHLO. Got: {0}", line));
                    }

                    // this should leave the connection TLS encrypted, and ready to either start AUTH or to start a mail item.
                }

                // handle authentication
                if (login)
                {
                    line = "AUTH LOGIN";
                    WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                    line = smtpStream.ReadLine(30000, Worker);
                    if (Verbose)
                    {
                        debugWriter.WriteLine(String.Format("S->C: {0}", line));
                        debugWriter.Flush();
                    }
                    if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Total Process Timeout Exceeded for the connection.");
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Timeout authenticating to the SMTP server. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Timeout authenticating to SMTP server.");
                    }
                    else if (line.StartsWith("530 "))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Server requires STARTTLS before authenticating. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Must use STARTTLS before authenticating.");
                    }
                    else if (line != "334 VXNlcm5hbWU6")
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# AUTH LOGIN apparently not supported by SMTP server. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception(string.Format("Authentication not supported. {0}", line));
                    }
                    line = BASE64Encode(gateway.Username);
                    WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                    line = smtpStream.ReadLine(30000, Worker);
                    if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Total Process Timeout Exceeded for the connection.");
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Timeout authenticating to the SMTP server. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Timeout authenticating to SMTP server.");
                    }
                    if (Verbose)
                    {
                        debugWriter.WriteLine(String.Format("S->C: {0}", line));
                        debugWriter.Flush();
                    }
                    if (line.StartsWith("530 "))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Server requires STARTTLS before authenticating. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Must use STARTTLS before authenticating.");
                    }
                    else if (line != "334 UGFzc3dvcmQ6")
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# AUTH LOGIN apparently not supported by SMTP server. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception(string.Format("Authentication not supported. {0}", line));
                    }
                    line = BASE64Encode(gateway.Password);
                    WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                    line = smtpStream.ReadLine(30000, Worker);
                    if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Total Process Timeout Exceeded for the connection.");
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Timeout authenticating to the SMTP server. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Timeout authenticating to SMTP server.");
                    }
                    if (Verbose)
                    {
                        debugWriter.WriteLine(String.Format("S->C: {0}", line));
                        debugWriter.Flush();
                    }
                    if (line.StartsWith("530 "))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Server requires STARTTLS before authenticating. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception("Must use STARTTLS before authenticating.");
                    }
                    else if (line.StartsWith("535 "))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Authentication Failed for user. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception(string.Format("Authentication not successful for user {0} on gateway {1}. Message: {2}", gateway.SMTPServer, gateway.Username, line));
                    }
                    else if (!line.StartsWith("235 "))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Authentication Failed for user. Aborting Connection.");
                            debugWriter.Flush();
                        }
                        throw new Exception(string.Format("Authentication not successful for user {0} on gateway {1}. Message: {2}", gateway.SMTPServer, gateway.Username, line));
                    }
                }

                // negotiate the MAIL FROM
                line = string.Format("MAIL FROM: {0}", EscapeEmailAddress(MailFromAddress));
                WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                line = smtpStream.ReadLine(30000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Total Process Timeout Exceeded for the connection.");
                }
                if (string.IsNullOrEmpty(line))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Timeout negotiating MAIL FROM. Aborting Connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Timeout negotiating MAIL FROM.");
                }
                if (Verbose)
                {
                    debugWriter.WriteLine(String.Format("S->C: {0}", line));
                    debugWriter.Flush();
                }
                if (line.StartsWith("550 "))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Authentication required or relay not permitted. Aborting connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception(string.Format("Authentication required or relay not permitted for {0}", MailFromAddress));
                }
                else if (!line.StartsWith("250 "))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine(string.Format("# Can't send as {0}. Aborting connection.", MailFromAddress));
                        debugWriter.Flush();
                    }
                    throw new Exception(string.Format("Can't send as {0}. {1}", MailFromAddress, line));
                }

                // and now the RCPT TO
                line = string.Format("RCPT TO: {0}", EscapeEmailAddress(RcptToAddress));
                WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                line = smtpStream.ReadLine(30000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Total Process Timeout Exceeded for the connection.");
                }
                if (string.IsNullOrEmpty(line))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Timeout negotiating RCPT TO. Aborting Connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Timeout negotiating RCPT TO.");
                }
                if (Verbose)
                {
                    debugWriter.WriteLine(String.Format("S->C: {0}", line));
                    debugWriter.Flush();
                }
                if (!line.StartsWith("250 ") && !line.StartsWith("251 "))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine(string.Format("# Can't send to {0}. Aborting connection.", RcptToAddress));
                        debugWriter.Flush();
                    }
                    throw new Exception(string.Format("Can't send to {0}. {1}", RcptToAddress, line));
                }

                // Signal that we are done with the envelope and ready to send the message
                line = "DATA";
                WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                line = smtpStream.ReadLine(30000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Total Process Timeout Exceeded for the connection.");
                }
                if (string.IsNullOrEmpty(line))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Timeout negotiating DATA stream. Aborting Connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Timeout negotiating DATA stream.");
                }
                if (Verbose)
                {
                    debugWriter.WriteLine(String.Format("S->C: {0}", line));
                    debugWriter.Flush();
                }
                if (!line.StartsWith("250 ") && !line.StartsWith("354 "))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine(string.Format("# Can't send to {0}. Aborting connection.", RcptToAddress));
                        debugWriter.Flush();
                    }
                    throw new Exception(string.Format("Can't send email to {0}. {1}", RcptToAddress, line));
                }

                // send the data
                bool FromDone = false;
                //bool MIMEFormat = false;
                if (Verbose && !VerboseBody)
                {
                    debugWriter.WriteLine(string.Format("C->S: <message body redacted>", line));
                    debugWriter.Flush();
                }
                string lastLine = null;
                bool LineSent = false;
                for (int i = 0; i < envelope.ChunkCount; i++)
                {
                    if (Worker.CancellationPending)
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Operation Canceled.");
                            debugWriter.Flush();
                        }
                        throw new OperationCanceledException();
                    }
                    byte[] datablock = SQLiteDB.MailChunk_GetChunk(envelope.EnvelopeID.Value, i);
                    string datastring = ASCIIEncoding.ASCII.GetString(datablock);
                    List<string> datalines = datastring.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
                    for (int lineno = 0; lineno < datalines.Count; lineno++)
                    //foreach (string dl in datalines)
                    {
                        string outputLine = datalines[lineno];
                        if (HeaderReplaceSender && !FromDone && outputLine.ToUpper().StartsWith("FROM:"))
                        {
                            FromDone = true;
                            outputLine = string.Format("FROM: {0}", EscapeEmailAddress(MailFromAddress));
                        }
                        else if (outputLine.StartsWith("."))
                        {
                            outputLine = string.Format(".{0}", outputLine);
                        }
                        // the last line in the  message might be empty because of how receiving works, so skip that if so.
                        if (string.IsNullOrEmpty(outputLine) && i == envelope.ChunkCount - 1 && lineno == datalines.Count - 1)
                        {
                            // skip this line
                            if (Verbose && VerboseBody)
                            {
                                debugWriter.WriteLine("# Cropping the last empty line from the message.");
                                debugWriter.Flush();
                            }
                        }
                        else
                        {
                            lastLine = outputLine;
                            WriteLineWithDebugOptions(smtpStream, outputLine, ref VerboseBody, debugWriter, MsgIdentifier);
                            LineSent = true;
                        }
                        if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                        {
                            if (Verbose)
                            {
                                debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                                debugWriter.Flush();
                            }
                            throw new Exception("Total Process Timeout Exceeded for the connection.");
                        }
                        if (swTimeout.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                        {
                            if (Verbose)
                            {
                                debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                                debugWriter.Flush();
                            }
                            throw new Exception("Timeout Exceeded for the connection.");
                        }
                    }
                }
                // end the message with /r/n./r/n
                if (!LineSent || !string.IsNullOrEmpty(lastLine))
                {
                    // if the last line sent had anythign on it, we need to emit an empty line before the .
                    // otherwise we can sometimes hang the receiving SMTP server.
                    line = "";
                    WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                }
                line = ".";
                WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);

                // get a final ack
                line = smtpStream.ReadLine(30000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Total Process Timeout Exceeded for the connection.");
                }
                if (string.IsNullOrEmpty(line))
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Connection timeout. Aborting connection.");
                        debugWriter.Flush();
                    }
                    throw new Exception("Timeout finalizing message. No ACK received.");
                }
                if (Verbose)
                {
                    debugWriter.WriteLine(String.Format("S->C: {0}", line));
                    debugWriter.Flush();
                }
                if (!line.StartsWith("250 "))
                {
                    if (line.StartsWith("554"))
                    {
                        // SendAsDenied
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Send message denied by SMTP server.");
                            debugWriter.Flush();
                        }
                        PermanentFailure = true;
                    }
                    throw new Exception(string.Format("Can't send email to {0}. {1}", RcptToAddress, line));
                }
                finalResults = string.Format("{0}{1}", MsgIdentifier, line);

                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogMessage = string.Format("{0}Email sent. {1}", MsgIdentifier, line)
                });

                // close the tcp connection
                line = "QUIT";
                WriteLineWithDebugOptions(smtpStream, line, ref Verbose, debugWriter, MsgIdentifier);
                line = smtpStream.ReadLine(10000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    if (Verbose)
                    {
                        debugWriter.WriteLine("# Timeout waiting for response to QUIT.");
                        debugWriter.Flush();
                    }
                }
                if (!string.IsNullOrEmpty(line))
                {
                    //we don't actually need to wait for a reply to a QUIT. We can hang up.
                    if (Verbose)
                    {
                        debugWriter.WriteLine(String.Format("S->C: {0}", line));
                        debugWriter.Flush();
                    }
                    if (!line.StartsWith("221 "))
                    {
                        if (Verbose)
                        {
                            debugWriter.WriteLine("# Unexpected response to QUIT message.");
                            debugWriter.Flush();
                        }
                        Worker.ReportProgress(0, new WorkerReport()
                        {
                            LogMessage = string.Format("{0}Unexpected response to QUIT. {1}", MsgIdentifier, line)
                        });
                    }
                }
                // store the ack as a SendLog and update the ProcessQueue.
                tblSendLog log = new tblSendLog(envelope.EnvelopeID.Value, envelopeRcpt.EnvelopeRcptID.Value, DateTime.Now, finalResults, sendQueueItem.AttemptCount, true);
                SQLiteDB.SendLog_Insert(log);
                sendQueueItem.State = QueueState.Done;
                sendQueueItem.RetryAfter = DateTime.Now;
                SQLiteDB.ProcessQueue_AddUpdate(sendQueueItem);
            }
            catch (OperationCanceledException opex)
            {
                // this might be because of a cancel, or a global timeout, or any number of underlying exceptions.
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogError = string.Format("Operation Cancelled: {0}. Will retry after a minimum of 10 minutes.", MsgIdentifier)
                });
                tblSendLog log = new tblSendLog(sendQueueItem.EnvelopeID, sendQueueItem.EnvelopeRcptID, DateTime.Now, "Operation cancelled", sendQueueItem.AttemptCount, false);
                SQLiteDB.SendLog_Insert(log);
                // roll back this aborted attempt.
                sendQueueItem.AttemptCount--;
                sendQueueItem.State = QueueState.Ready;
                sendQueueItem.RetryAfter = DateTime.Now.AddMinutes(10);
                SQLiteDB.ProcessQueue_AddUpdate(sendQueueItem);
            }
            catch (Exception ex)
            {
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogError = string.Format("{0}{1}{2}", PermanentFailure ? "Permanent Failure: " : "", MsgIdentifier, ex.Message)
                });
                try
                {
                    tblSendLog log = new tblSendLog(sendQueueItem.EnvelopeID, sendQueueItem.EnvelopeRcptID, DateTime.Now, ex.Message, sendQueueItem.AttemptCount, false);
                    SQLiteDB.SendLog_Insert(log);
                    if (PermanentFailure)
                    {
                        sendQueueItem.State = QueueState.SendAdminFailure;
                    }
                    else
                    {
                        sendQueueItem.State = QueueState.Ready;
                        if (sendQueueItem.AttemptCount < 4)
                        {
                            sendQueueItem.RetryAfter = DateTime.Now.AddMinutes(10);
                        }
                        else if (sendQueueItem.AttemptCount < 25)
                        {
                            sendQueueItem.RetryAfter = DateTime.Now.AddHours(6);
                        }
                        else
                        {
                            sendQueueItem.State = QueueState.SendAdminFailure;
                            sendQueueItem.RetryAfter = DateTime.Now;
                        }
                    }
                    SQLiteDB.ProcessQueue_AddUpdate(sendQueueItem);
                }
                catch { }
            }
            finally
            {
                if (debugWriter != null)
                {
                    try
                    {
                        debugWriter.Flush();
                    }
                    catch { }
                    try
                    {
                        debugWriter.Close();
                    }
                    catch { }
                    try
                    {
                        debugWriter.Dispose();
                    }
                    catch { }
                    debugWriter = null;
                }
                if (smtpStream != null)
                {
                    try
                    {
                        smtpStream.Release();
                    }
                    catch { }
                    smtpStream = null;
                }
                if (stream != null)
                {
                    try
                    {
                        stream.Dispose();
                    }
                    catch { }
                    stream = null;
                }
                if (client != null)
                {
                    try
                    {
                        client.Dispose();
                    }
                    catch { }
                    client = null;
                }
            }
        }

        private void WriteLineWithDebugOptions(ISMTPStream lineStream, string line, ref bool debug, TextWriter debugWriter, string messageID)
        {
            if (debug)
            {
                try
                {
                    debugWriter.WriteLine(String.Format("C->S: {0}", line));
                    debugWriter.Flush();
                }
                catch (Exception ex)
                {
                    Worker.ReportProgress(0, new WorkerReport()
                    {
                        LogError = string.Format("Verbose Logging failed for {0}. Exception: {1}", messageID, ex.Message)
                    });
                    debug = false;
                }
            }
            lineStream.WriteLine(line);
        }

        private string SanitizeName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }

        private string BASE64Encode(string cleartext)
        {
            try
            {
                return Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(cleartext));
            }
            catch
            {
                return null;
            }
        }

        private string EscapeEmailAddress(string email)
        {
            if (!email.StartsWith("<"))
            {
                email = string.Format("<{0}", email);
            }
            if (!email.EndsWith(">"))
            {
                email = string.Format("{0}>", email);
            }
            return email;
        }

    }
}
