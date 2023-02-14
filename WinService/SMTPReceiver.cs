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
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SMTPRelay.WinService
{
    public class SMTPReceiver
    {
        private readonly int MAX_RCPTTO_FAIL_COUNT = 10;
        private BackgroundWorker Worker;        
        public bool Running;        
        public ConcurrentQueue<WorkerReport> WorkerReports;

        public SMTPReceiver(TcpClient client, tblIPEndpoint endpoint)
        {
            WorkerReports = new ConcurrentQueue<WorkerReport>();
            Running = true;
            Worker = new BackgroundWorker();
            Worker.WorkerSupportsCancellation = true;
            Worker.WorkerReportsProgress = true;
            Worker.DoWork += Worker_DoWork;
            Worker.ProgressChanged += Worker_ProgressChanged;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            WorkerArgs args = new WorkerArgs(client, endpoint);
            Worker.RunWorkerAsync(args);
        }

        private class WorkerArgs
        {
            public WorkerArgs(TcpClient client, tblIPEndpoint endpoint)
            {
                Client = client;
                EndPoint = endpoint;
            }
            public TcpClient Client { get; private set; }

            public tblIPEndpoint EndPoint { get; private set; }
        }

        public void Cancel()
        {
            Worker.CancelAsync();
        }

        private enum SMTPStates
        {
            SendHello,                            // Send the 220 welcome message, proceed to WaitClientHello,
            WaitClientHello,                      // Wait for client to send the EHLO/HELO command. If get EHLO proceed to SendServerExtensions. Else, proceed to SendSuccessAck
            SendServerExtensions,                 // Client has send EHLO/HELO, transmit our extnesions. proceed to SendWelcomeClient
            SendWelcomeClient,                    // Sends the 250 please to meet you. Proceed to WaitForClientVerb.
            SendSuccessAck,                       // Send 250 Ok. Procedd to WaitForClientVerb.
            WaitForClientVerb,                    // Wait for the client to send something. Depending on command... 
                                                  // RSET, clear mail object, proceed to SendSuccessAck. This is probably an antivirus interception.
                                                  // NOOP, proceed to SendSuccessAck
                                                  // STARTTLS, proceed to ProcessStartTLS
                                                  // AUTH, proceed to SendAUTHLOGINUsernameChallenge. If TLS Enforced but not enabled, proceed to SendStartTLSReq
                                                  // MAIL FROM, proceed to ProcessMAILFROM. If TLS Enforced but not enabled, proceed to SendStartTLSReq
                                                  // RCPT TO, proceed to ProcessRCPTTOMessage
                                                  // QUIT, proceed to ProcessQUITMessage.
                                                  // DATA, proceed to ProcessDATAMessage.
                                                  // For all other messages, proceed to GenericNotImplemented.
            ProcessStartTLS,                      // If we can support TLS, proceed to SendStartTLSProceed, else proceed to SendStartTLSNotAvail
            SendStartTLSProceed,                  // Send 220 Ready to start TLS and procced to SwitchToTLS
            SwitchToTLS,                          // Change over to TLS Stream, and proceded to WaitClientHello
            SendStartTLSNotAvail,                 // Send 454 TLS not available. proceed to WaitForClientVerb.
            SendStartTLSReq,                      // Command received that requires StartTLS first. Send 530 StartTLS Required. Proceed to WaitForClientVerb
            SendAuthReq,                          // Authentication required. Send 550 Authentication Required. Proceed to WaitForClientVerb
            SendBadCommandSeq,                    // Got a RCPT TO with no MAIL object yet. Send 503 Bad sequence of commands. Proceed to WaitForClientVerb.
            ProcessAUTHLOGINMessage,              // Prepare to handle auth requests. If acceptable, proceed to SendAUTHLOGINUsernameChallenge.
                                                  // If we require STARTTLS, proceed to SendStartTLSReq. This is currently not implemented.
            SendAUTHLOGINUsernameChallenge,       // Client sent AUTH LOGIN, Clear logged in user, send 334 VXNlcm5hbWU6. Proceed to WaitForClientUsernameResonse
            WaitForClientUsernameResonse,         // Receive BASE64 encoded username. Proceed to SendAUTHLOGINPasswordChallenge
            SendAUTHLOGINPasswordChallenge,       // Send 334 UGFzc3dvcmQ6. Proceed to WaitForClientPasswordResponse
            WaitForClientPasswordResponse,        // Receive BASE64 encoded password. Proceed to ProcessAUTHLOGINCredentials
            ProcessAUTHLOGINCredentials,          // Query database with the username/password they provided.
                                                  // If successful, proceed to SendAUTHLOGINSuccess
                                                  // If unsuccessful, proceed to SendAUTHLOGINFailure
            SendAUTHLOGINSuccess,                 // Send 235 Authentication successful. Proceed to WaitForClientVerb
            SendAUTHLOGINFailure,                 // Send 535 SMTP Authentication unsuccessful/Bad username or password. proceed to WaitForClientVerb
            ProcessMAILFROM,                      // We got a MAIL FROM message from the user. Create a MAIL object to hold the client data
                                                  // If not logged in, proceed to SendAuthReq
                                                  // If logged in and sender is OK, proceed to SendMAILFROMSuccess.
                                                  // If we can't accept from that sender, proceed to SendMAILFROMFailure.
            SendMAILFROMSuccess,                  // Send 250 Ok in response to the MAIL FROM command. Proceed to WaitForClientVerb
            SendMAILFROMFailure,                  // Send 550 Relay Not Permitted error. proceed to WaitForClientVerb
            ProcessRCPTTOMessage,                 // Got a RCPT TO message. Should be authenticated, and have a MAIL object ready already.
                                                  // If Not in MAIL mode, proceed to SendBadCommandSeq
                                                  // If recipient acceptable, proceed to SendRCPTTOOk
                                                  // If recipient unacceptable, proceed to SendRCPTTOBadAddress
                                                  // If max recipient count exceeded, proceed to SendRCPTTOTooMany.
            SendRCPTTOBadAddress,                 // Got a RCPT TO with invalid email address. Send 550 No such user here. Proceed to WaitForClientVerb.
            SentSCPTTONotOurUser,                 // Got a RCPT TO while in unauth mode, but user is not local. Send 553 Sorry, that user isn't in my list of allowed rcpthosts
            SendRCPTTOTooMany,                    // Send 452 Too Many recipients. Proceed to WaitForClientVerb
            SendRCPTTOOk,                         // Got a RCPT TO that is valid. Send 250 Ok, proceed to WaitForClientVerb.
            ProcessDATAMessage,                   // We got a DATA command. Should be authenticated, in MAIL mode, with at least one recipient. Create necessary database objects.
                                                  // If we are not in MAIL mode, proceed to SendBadCommandSeq
                                                  // If we have no valid recipients, proceed to SendDATANoRcpt
                                                  // If we are ready to begin receiving data, proceed to SendDATAOk.
                                                  // If we have a failure creating the database objects, proceed to 
            SendDATAServerError,                  // Send 451 Local server error, try again later. Proceed to WaitForClientVerb.
            SendDATANoRcpt,                       // Send 554 No valid recipients, proceed to WaitForClientVerb
            SendDATAOk,                           // We have valid records in the database and are ready to receive. Send 354 End data with <CR><LF>.<CR><LF>. Proceed to ReceiveDATABlock
            ReceiveDATABlock,                     // We are receiving the message. When we get a \r\n.\r\n, proceed to FinalizeDATABlock.
                                                  // If the message data exceeds our mesage limit, proceed to FailDATABlock.
            FailDATABlock,                        // Send 552 Requested mail actions aborted – Exceeded storage allocation.
                                                  // Delete all message data, mark Envelope as failed. Proceed to BadCommandDisconnect.
            FinalizeDATABlock,                    // We have finished receiving the message. Finish processing, and add item to the outbound queue. Proceed to SendDATAAck.
            SendDATAAck,                          // Send the 250 Ok and tracking ID to client. Clear MAIL object so another can be started. Proceed to WaitForClientVerb.
            ProcessQUITMessage,                   // Client sent QUIT. Send 221 Bye and close the connection.
            BadCommandDisconnect,                 // Send 421 Too Many errors. Close the connection.
            GenericNotImplemented,                // Generic error that command isn't implemented. 550 Command not Implemented.
                                                  // If not exceeds error limit, Go to WaitForClientVerb
                                                  // if exceeds error limit, go to BadCommandDisconnect.
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Random rnd = new Random();
            // conversation state tracking 
            SMTPStates state = SMTPStates.SendHello;
            // turns on ESMTP extensions for this connection. Enabled if client sends EHLO.
            bool eSMTP = false;
            bool EncryptedChannel = false;
            // If authentication is successful, this is the connected user loaded from the database.
            tblUser connectedUser = null;
            tblDevice connectedDevice = null;
            string ClientUsername = "";
            string ClientPassword = "";
            int RCPTTOFailCount = 0;
            int ExpectedMessageBytesCount = -1;
            TextWriter debugWriter = null;
            string outLine = "";

            try
            {
                System.Diagnostics.Stopwatch swTimeout = new System.Diagnostics.Stopwatch();
                swTimeout.Start();
                WorkerArgs args = e.Argument as WorkerArgs;
                if (args == null || args.Client == null || args.EndPoint == null)
                {
                    throw new Exception("Invalid TcpClient or EndPoint passed to SMTPReceiver.");
                }

                TcpClient client = args.Client;

                NetworkStream stream = client.GetStream();
                ISMTPStream lineStream = new SMTPStreamHandler(stream);
                EndPoint clientEP = client.Client.RemoteEndPoint;
                EndPoint localEP = client.Client.LocalEndPoint;
                string ClientIPAddress = ((IPEndPoint)clientEP).Address.ToString();
                string LocalIPAddress = ((IPEndPoint)localEP).Address.ToString();
                X509Certificate2 serverCert = null;

                tblEnvelope ActiveEnvelope = null;
                List<tblEnvelopeRcpt> ActiveEnvelopeRcpts = null;

                try
                {
                    // get configuration
                    int MaxMessageLength = int.Parse(SQLiteDB.System_GetValue("Message", "MaxLength"));
                    int MaxRecipients = int.Parse(SQLiteDB.System_GetValue("Message", "MaxRecipients"));
                    int MaxChunkSize = int.Parse(SQLiteDB.System_GetValue("Message", "ChunkSize"));
                    string ServerHostName = args.EndPoint.Hostname;
                    int ConnectionTimeoutMS = int.Parse(SQLiteDB.System_GetValue("SMTPServer", "ConnectionTimeoutMS"));
                    int CommandTimeoutMS = int.Parse(SQLiteDB.System_GetValue("SMTPServer", "CommandTimeoutMS"));
                    int BadCommandLimit = int.Parse(SQLiteDB.System_GetValue("SMTPServer", "BadCommandLimit"));
                    bool UnauthMaildrop = args.EndPoint.Maildrop;
                    bool UnauthModeEnabled = false;

                    // these get flagged true to enable debugging based on the current settings in the database.
                    bool Verbose = false;
                    bool VerboseBodyStarted = false;
                    bool VerboseBody = false;
                    
                    string VerboseEnabledString = SQLiteDB.System_GetValue("SMTPServer", "VerboseDebuggingEnabled");
                    if (string.IsNullOrEmpty(VerboseEnabledString) || (VerboseEnabledString != "1" && VerboseEnabledString != "0"))
                    {
                        Worker.ReportProgress(0, new WorkerReport()
                        {
                            LogWarning = "Resetting default Verbose Debugging settings for SMTPServer."
                        });
                        SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingEnabled", "0");
                        SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingPath", "C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Receive\\");
                        SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingIncludeBody", "0");
                    }
                    else if (VerboseEnabledString != "1")
                    {
                        string VerbosePathString = SQLiteDB.System_GetValue("SMTPServer", "VerboseDebuggingPath");
                        string debugpath = SQLiteDB.System_GetValue("SMTPServer", "VerboseDebuggingPath");
                        string debugbody = SQLiteDB.System_GetValue("SMTPServer", "VerboseDebuggingIncludeBody");
                        if (string.IsNullOrEmpty(debugpath))
                        {
                            Worker.ReportProgress(0, new WorkerReport()
                            {
                                LogWarning = "No SMTPServer verbose debug path specified. Using C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Receive\\"
                            });
                            debugpath = "C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Receive\\";
                            SQLiteDB.System_AddUpdateValue("SMTPServer", "VerboseDebuggingPath", "C:\\ProgramData\\SMTPRelay\\VerboseDebugging\\Receive\\");
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
                            string fname = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff") + ClientIPAddress;
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

                    string ClientHostName = "";
                    MailObject mailObject = null;

                    //flag True to close
                    bool CloseConnection = false;

                    // in the beginning, state = SMTPStates.SendHello
                    outLine = string.Format("220 {0} ESMTP MAIL relay service ready {1}", ServerHostName, DateTime.UtcNow);
                    WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                    state = SMTPStates.WaitClientHello;

                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    string line = null;

                    sw.Restart();
                    while (!CloseConnection)
                    {
                        if (Worker.CancellationPending)
                        {
                            outLine = "554 SMTP Server is shutting down";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            CloseConnection = true;
                            if (ActiveEnvelope != null)
                            {
                                CleanupFailedMessageData(ActiveEnvelope);
                            }
                            ActiveEnvelope = null;
                            ActiveEnvelopeRcpts = null;
                            mailObject = null;
                        }
                        else if ((state == SMTPStates.WaitClientHello && sw.ElapsedMilliseconds > ConnectionTimeoutMS) ||
                                 (state != SMTPStates.WaitClientHello && state != SMTPStates.ReceiveDATABlock && sw.ElapsedMilliseconds > CommandTimeoutMS))
                        {
                            outLine = "421 Timout, closing connection";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            CloseConnection = true;
                        }
                        else if (BadCommandLimit <= 0)
                        {
                            outLine = "421 Too many bad commands, closing connection";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            CloseConnection = true;
                        }
                        else if (state == SMTPStates.WaitClientHello)
                        {
                            line = lineStream.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                if (Verbose)
                                {
                                    try
                                    {
                                        debugWriter.WriteLine(string.Format("C->S: {0}", line));
                                    }
                                    catch (Exception ex)
                                    {
                                        Worker.ReportProgress(0, new WorkerReport()
                                        {
                                            LogError = string.Format("Verbose Logging failed for {0}. Exception: {1}", ClientIPAddress, ex.Message)
                                        });
                                        Verbose = false;
                                        VerboseBody = false;
                                    }
                                }
                                // we got something.
                                if (line.ToUpper().StartsWith("EHLO ") && line.Length > 6 && line.Length < 134)
                                {
                                    eSMTP = true;
                                    ClientHostName = line.Substring(5).Trim();
                                    state = SMTPStates.SendServerExtensions;
                                    sw.Restart();
                                }
                                else if (line.ToUpper().StartsWith("HELO ") && line.Length > 6 && line.Length < 134)
                                {
                                    eSMTP = false;
                                    ClientHostName = line.Substring(5).Trim();
                                    state = SMTPStates.SendWelcomeClient;
                                    sw.Restart();
                                }
                                else
                                {
                                    outLine = "550 Command not Implemented";
                                    WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                                    BadCommandLimit--;
                                }
                                if (state != SMTPStates.WaitClientHello)
                                {
                                    if (connectedDevice == null)
                                    {
                                        List<tblDevice> devices = SQLiteDB.Device_GetByAddress(ClientIPAddress);
                                        // try to find an exact ClientHostname match.
                                        for (int i = 0; i < devices.Count; i++)
                                        {
                                            if (devices[i].Hostname == ClientHostName)
                                            {
                                                connectedDevice = devices[i];
                                                break;
                                            }
                                        }
                                        if (connectedDevice == null)
                                        {
                                            // try to find a device with an empty hostname, and match to that.
                                            for (int i = 0; i < devices.Count; i++)
                                            {
                                                if (string.IsNullOrEmpty(devices[i].Hostname))
                                                {
                                                    connectedDevice = devices[i];
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (EncryptedChannel)
                                    {
                                        Worker.ReportProgress(0, new WorkerReport()
                                        {
                                            LogMessage = string.Format("Upgraded encryption for {0} connection from {1} [{2}] to {3}.", 
                                            eSMTP ? "ESMTP" : "SMTP", ClientHostName, ClientIPAddress, ((SMTPTLSStreamHandler)lineStream).EncryptionNegotiated)
                                        }) ;
                                    }
                                    else
                                    {
                                        Worker.ReportProgress(0, new WorkerReport()
                                        {
                                            LogMessage = string.Format("Accepted {0} connection from {1} [{2}].", eSMTP ? "ESMTP" : "SMTP", ClientHostName, ClientIPAddress)
                                        });
                                    }
                                }
                            }
                        }
                        else if (state == SMTPStates.SendServerExtensions)
                        {
                            outLine = string.Format("250-{0}", ServerHostName);
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            outLine = string.Format("250-SIZE {0}", MaxMessageLength);
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            if (EncryptedChannel)
                            {
                                outLine = "250-AUTH LOGIN";
                                WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            }
                            else
                            {
                                if (args.EndPoint.TLSMode != tblIPEndpoint.IPEndpointTLSModes.Disabled)
                                {
                                    outLine = "250-STARTTLS";
                                    WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                                }
                                if (args.EndPoint.TLSMode != tblIPEndpoint.IPEndpointTLSModes.Enforced)
                                {
                                    outLine = "250-AUTH LOGIN";
                                    WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                                }
                            }
                            state = SMTPStates.SendWelcomeClient;
                        }
                        else if (state == SMTPStates.SendWelcomeClient)
                        {
                            outLine = string.Format("250 {0} Hello {1} [{2}], pleased to meet you", ServerHostName, ClientHostName, ClientIPAddress);
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SendSuccessAck)
                        {
                            outLine = "250 Ok";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.WaitForClientVerb)
                        {
                            line = lineStream.ReadLine(10);
                            if (!string.IsNullOrEmpty(line))
                            {
                                if (Verbose)
                                {
                                    try
                                    {
                                        debugWriter.WriteLine(string.Format("C->S: {0}", line));
                                    }
                                    catch (Exception ex)
                                    {
                                        Worker.ReportProgress(0, new WorkerReport()
                                        {
                                            LogError = string.Format("Verbose Logging failed for {0}. Exception: {1}", ClientIPAddress, ex.Message)
                                        });
                                        Verbose = false;
                                        VerboseBody = false;
                                    }
                                }
                                sw.Restart();
                                // figure out what they said.
                                if (line.ToUpper() == "RSET")
                                {
                                    mailObject = null;
                                    UnauthModeEnabled = false;
                                    ExpectedMessageBytesCount = -1;
                                    RCPTTOFailCount = 0;
                                    state = SMTPStates.SendSuccessAck;
                                }
                                else if (line.ToUpper() == "NOOP")
                                {
                                    state = SMTPStates.SendSuccessAck;
                                }
                                else if (line.ToUpper() == "STARTTLS")
                                {
                                    state = SMTPStates.ProcessStartTLS;
                                }
                                else if (line.ToUpper().StartsWith("AUTH"))
                                {
                                    state = SMTPStates.ProcessAUTHLOGINMessage;
                                }
                                else if (line.ToUpper().StartsWith("MAIL FROM:"))
                                {
                                    state = SMTPStates.ProcessMAILFROM;
                                }
                                else if (line.ToUpper().StartsWith("RCPT TO:"))
                                {
                                    state = SMTPStates.ProcessRCPTTOMessage;
                                }
                                else if (line.ToUpper() == "DATA")
                                {
                                    state = SMTPStates.ProcessDATAMessage;
                                }
                                else if (line.ToUpper() == "QUIT")
                                {
                                    state = SMTPStates.ProcessQUITMessage;
                                }
                                else
                                {
                                    outLine = "550 Command not Implemented";
                                    WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                                    BadCommandLimit--;
                                }
                            }
                        }
                        else if (state == SMTPStates.ProcessStartTLS)
                        {
                            if (args.EndPoint.TLSMode == tblIPEndpoint.IPEndpointTLSModes.Disabled ||
                                string.IsNullOrEmpty(args.EndPoint.CertFriendlyName))
                            {
                                state = SMTPStates.SendStartTLSNotAvail;
                            }
                            if (args.EndPoint.TLSMode == tblIPEndpoint.IPEndpointTLSModes.Enabled ||
                                args.EndPoint.TLSMode == tblIPEndpoint.IPEndpointTLSModes.Enforced)
                            {
                                state = SMTPStates.SendStartTLSProceed;
                            }
                        }
                        else if (state == SMTPStates.SendStartTLSProceed)
                        {
                            // see if we can actually get the cert specified.
                            serverCert = FindSystemCert(args.EndPoint.CertFriendlyName);
                            if (serverCert != null)
                            {
                                outLine = "220 Go ahead";
                                WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                                state = SMTPStates.SwitchToTLS;
                            }
                            else
                            {
                                state = SMTPStates.SendStartTLSNotAvail;
                            }
                        }
                        else if (state == SMTPStates.SwitchToTLS)
                        {
                            lineStream.Release();
                            lineStream = new SMTPTLSStreamHandler(stream, SMTPTLSStreamHandler.Mode.Server, null, serverCert);
                            state = SMTPStates.WaitClientHello;
                            EncryptedChannel = true;
                        }
                        else if (state == SMTPStates.SendStartTLSNotAvail)
                        {
                            outLine = "454 TLS not available";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SendStartTLSReq)
                        {
                            outLine = "530 StartTLS Required";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SendAuthReq)
                        {
                            outLine = "550 Authentication Required";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SendBadCommandSeq)
                        {
                            outLine = "503 Bad sequence of commands";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.ProcessAUTHLOGINMessage)
                        {
                            connectedUser = null;
                            ClientUsername = "";
                            ClientPassword = "";
                            // Encrypted channels aren't suported yet, so just proceed.
                            if (line.ToUpper().StartsWith("AUTH LOGIN") && line.Length > 10)
                            {
                                if (args.EndPoint.TLSMode == tblIPEndpoint.IPEndpointTLSModes.Enforced && !EncryptedChannel)
                                {
                                    state = SMTPStates.SendStartTLSReq;
                                }
                                else
                                {
                                    try
                                    {
                                        ClientUsername = BASE64Decode(line.Substring(10).Trim());
                                    }
                                    catch
                                    {
                                        ClientUsername = null;
                                    }
                                    if (string.IsNullOrEmpty(ClientUsername))
                                    {
                                        state = SMTPStates.SendAUTHLOGINUsernameChallenge;
                                    }
                                    else
                                    {
                                        state = SMTPStates.SendAUTHLOGINPasswordChallenge;
                                    }
                                }
                            }
                            else
                            {
                                state = SMTPStates.SendAUTHLOGINUsernameChallenge;
                            }
                        }
                        else if (state == SMTPStates.SendAUTHLOGINUsernameChallenge)
                        {
                            outLine = "334 VXNlcm5hbWU6";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientUsernameResonse;
                        }
                        else if (state == SMTPStates.WaitForClientUsernameResonse)
                        {
                            line = lineStream.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                if (Verbose)
                                {
                                    try
                                    {
                                        debugWriter.WriteLine(string.Format("C->S: {0}", line));
                                    }
                                    catch (Exception ex)
                                    {
                                        Worker.ReportProgress(0, new WorkerReport()
                                        {
                                            LogError = string.Format("Verbose Logging failed for {0}. Exception: {1}", ClientIPAddress, ex.Message)
                                        });
                                        Verbose = false;
                                        VerboseBody = false;
                                    }
                                }
                                sw.Restart();
                                ClientUsername = BASE64Decode(line);
                                state = SMTPStates.SendAUTHLOGINPasswordChallenge;
                            }
                        }
                        else if (state == SMTPStates.SendAUTHLOGINPasswordChallenge)
                        {
                            outLine = "334 UGFzc3dvcmQ6";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientPasswordResponse;
                        }
                        else if (state == SMTPStates.WaitForClientPasswordResponse)
                        {
                            line = lineStream.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                if (Verbose)
                                {
                                    try
                                    {
                                        debugWriter.WriteLine(string.Format("C->S: {0}", line));
                                    }
                                    catch (Exception ex)
                                    {
                                        Worker.ReportProgress(0, new WorkerReport()
                                        {
                                            LogError = string.Format("Verbose Logging failed for {0}. Exception: {1}", ClientIPAddress, ex.Message)
                                        });
                                        Verbose = false;
                                        VerboseBody = false;
                                    }
                                }
                                sw.Restart();
                                ClientPassword = BASE64Decode(line);
                                state = SMTPStates.ProcessAUTHLOGINCredentials;
                            }
                        }
                        else if (state == SMTPStates.ProcessAUTHLOGINCredentials)
                        {
                            System.Threading.Thread.Sleep(900 + rnd.Next(150));
                            if (string.IsNullOrEmpty(ClientUsername) || string.IsNullOrEmpty(ClientPassword))
                            {
                                BadCommandLimit--;
                                state = SMTPStates.SendAUTHLOGINFailure;
                            }
                            else
                            {
                                connectedUser = SQLiteDB.User_GetByEmailPassword(ClientUsername, ClientPassword);
                                if (connectedUser == null)
                                {
                                    BadCommandLimit--;
                                    state = SMTPStates.SendAUTHLOGINFailure;
                                }
                                else
                                {
                                    state = SMTPStates.SendAUTHLOGINSuccess;
                                }
                            }
                        }
                        else if (state == SMTPStates.SendAUTHLOGINSuccess)
                        {
                            outLine = "235 Authentication successful";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                            UnauthModeEnabled = false;
                        }
                        else if (state == SMTPStates.ProcessMAILFROM)
                        {
                            try
                            {
                                bool FailedParse = false;
                                // trim the address string
                                string addrString = line.Substring(10).Trim(); // remove  "MAIL FROM:"
                                if (addrString.StartsWith("\""))    // remove any quotes
                                {
                                    addrString = addrString.Replace('\"', ' ').Trim();
                                }
                                // see if the server has sent a "size" extension
                                int rcptSizeParam = -1;
                                if (addrString.Contains("SIZE="))
                                {
                                    string sz = addrString.Substring(addrString.IndexOf("SIZE=") + 5);
                                    addrString = addrString.Substring(0, addrString.IndexOf("SIZE=")).Trim();
                                    if (!Int32.TryParse(sz, out rcptSizeParam))
                                    {
                                        FailedParse = true;
                                    }
                                    ExpectedMessageBytesCount = rcptSizeParam;
                                }
                                else
                                {
                                    ExpectedMessageBytesCount = -1;
                                }

                                if (FailedParse)
                                {
                                    state = SMTPStates.SendMAILFROMFailure;
                                    BadCommandLimit--;
                                }
                                else
                                {
                                    if (UnauthMaildrop)
                                    {
                                        // we are allowing unauthenticated users to send mail if it's to a local maildrop enabled user
                                        try
                                        {
                                            System.Net.Mail.MailAddress test = new System.Net.Mail.MailAddress(addrString);
                                            mailObject = new MailObject(test.Address);
                                            state = SMTPStates.SendMAILFROMSuccess;
                                            UnauthModeEnabled = true;
                                            Worker.ReportProgress(0, new WorkerReport()
                                            {
                                                LogMessage = string.Format("Changing connection mode to Maildrop {0} from {1} [{2}].", eSMTP ? "ESMTP" : "SMTP", ClientHostName, ClientIPAddress)
                                            });
                                        }
                                        catch (Exception ex)
                                        {
                                            Worker.ReportProgress(0, new WorkerReport()
                                            {
                                                LogError = string.Format("Error processing MAIL FROM: \"{0}\": {1}.", addrString, ex.Message)
                                            });
                                            line = addrString;
                                            state = SMTPStates.SendMAILFROMFailure;
                                        }
                                    }
                                    else
                                    {
                                        // if we are NOT a maildrop, we require either an approved device, or an authenticated user
                                        if (connectedUser == null && connectedDevice == null)
                                        {
                                            state = SMTPStates.SendAuthReq;
                                        }
                                        else
                                        {
                                            try
                                            {
                                                System.Net.Mail.MailAddress test = new System.Net.Mail.MailAddress(addrString);
                                                mailObject = new MailObject(test.Address);
                                                state = SMTPStates.SendMAILFROMSuccess;
                                            }
                                            catch (Exception ex)
                                            {
                                                Worker.ReportProgress(0, new WorkerReport()
                                                {
                                                    LogError = string.Format("Error processing MAIL FROM: \"{0}\": {1}.", addrString, ex.Message)
                                                });
                                                line = addrString;
                                                state = SMTPStates.SendMAILFROMFailure;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                state = SMTPStates.SendMAILFROMFailure;
                                BadCommandLimit--;
                            }
                        }
                        else if (state == SMTPStates.SendMAILFROMFailure)
                        {
                            outLine = "513 Bad email address";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SendMAILFROMSuccess)
                        {
                            outLine = string.Format("250 Originator {0} Ok.", mailObject.Sender);
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.ProcessRCPTTOMessage)
                        {
                            if (mailObject == null)
                            {
                                state = SMTPStates.SendBadCommandSeq;
                                BadCommandLimit--;
                            }
                            else if (mailObject.Recipients.Count == MaxRecipients)
                            {
                                state = SMTPStates.SendRCPTTOTooMany;
                            }
                            else
                            {
                                // parse the recipient email address
                                string addrString = line.Substring(8).Trim();
                                try
                                {
                                    System.Net.Mail.MailAddress test = new System.Net.Mail.MailAddress(addrString);
                                    bool userok = true;
                                    if (UnauthModeEnabled)
                                    {
                                        // if we are in unauth mode, we check each recipient to make sure they are a local user,
                                        // enabled, and maildrop enabled.
                                        tblUser localUser = SQLiteDB.User_GetByEmail(test.Address);
                                        if (localUser == null || !localUser.Enabled || !localUser.Maildrop)
                                        {
                                            state = SMTPStates.SentSCPTTONotOurUser;
                                            userok = false;
                                        }
                                    }
                                    if (userok)
                                    {
                                        mailObject.Recipients.Add(test.Address);
                                        line = test.Address;
                                        state = SMTPStates.SendRCPTTOOk;
                                        Worker.ReportProgress(0, new WorkerReport()
                                        {
                                            LogMessage = string.Format("Maildrop {0} from {1} [{2}] accepted RCPT TO {3}.", eSMTP ? "ESMTP" : "SMTP", ClientHostName, ClientIPAddress, test.Address)
                                        });
                                    }
                                    else
                                    {
                                        RCPTTOFailCount++;
                                        Worker.ReportProgress(0, new WorkerReport()
                                        {
                                            LogWarning = string.Format("Maildrop {0} from {1} [{2}] REJECTED RCPT TO {3}.", eSMTP ? "ESMTP" : "SMTP", ClientHostName, ClientIPAddress, addrString)
                                        });
                                        if (RCPTTOFailCount >= MAX_RCPTTO_FAIL_COUNT)
                                        {
                                            outLine = "421 Too many bad commands, closing connection";
                                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                                            CloseConnection = true;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Worker.ReportProgress(0, new WorkerReport()
                                    {
                                        LogError = string.Format("Error processing RCPT TO: \"{0}\": {1}.", addrString, ex.Message)
                                    });
                                    line = addrString;
                                    state = SMTPStates.SendRCPTTOBadAddress;
                                }
                            }
                        }
                        else if (state == SMTPStates.SendRCPTTOTooMany)
                        {
                            outLine = "452 Too Many recipients";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SendRCPTTOBadAddress)
                        {
                            outLine = "513 Bad email address";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SentSCPTTONotOurUser)
                        {
                            outLine = "553 Sorry, that user isn't in my list of allowed rcpthosts";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SendRCPTTOOk)
                        {
                            if (ExpectedMessageBytesCount <= -1)
                            {
                                outLine = string.Format("250 Recipient {0} Ok", line);
                                WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            }
                            else
                            {
                                outLine = string.Format("250 Recipient {0} Ok; can accomodate {1} byte message.", line, ExpectedMessageBytesCount);
                                WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            }
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.ProcessDATAMessage)
                        {
                            if (mailObject == null)
                            {
                                state = SMTPStates.SendBadCommandSeq;
                                BadCommandLimit--;
                            }
                            else if (mailObject.Recipients.Count == 0)
                            {
                                state = SMTPStates.SendDATANoRcpt;
                            }
                            else
                            {
                                try
                                {
                                    // create entries in the database
                                    long? userid = connectedUser?.UserID;
                                    long? deviceid = connectedDevice?.DeviceID;

                                    ActiveEnvelope = new tblEnvelope(userid, deviceid, DateTime.Now, mailObject.Sender, FormatRecipients(mailObject.Recipients), 0, SQLiteDB.GenerateNonce(16));
                                    SQLiteDB.Envelope_Add(ActiveEnvelope);
                                    ActiveEnvelopeRcpts = new List<tblEnvelopeRcpt>();
                                    foreach (var rcp in mailObject.Recipients)
                                    {
                                        tblEnvelopeRcpt envrcp = new tblEnvelopeRcpt(ActiveEnvelope.EnvelopeID.Value, rcp);
                                        SQLiteDB.EnvelopeRcpt_Insert(envrcp);
                                        ActiveEnvelopeRcpts.Add(envrcp);
                                    }
                                    mailObject.ChunkData.AppendLine(
                                        string.Format("Received: from {0} ({1})\r\n by {2} ({3}) with {4} id {5};\r\n {6}",
                                        ClientHostName, ClientIPAddress, ServerHostName, LocalIPAddress, eSMTP ? "ESMTP" : "SMTP",
                                        ActiveEnvelope.MsgID, DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH\\:mm\\:ss +0000")));
                                    state = SMTPStates.SendDATAOk;
                                }
                                catch (Exception ex)
                                {
                                    Worker.ReportProgress(0, new WorkerReport()
                                    {
                                        LogError = ex.Message
                                    });
                                    state = SMTPStates.SendDATAServerError;
                                }
                            }
                        }
                        else if (state == SMTPStates.SendDATANoRcpt)
                        {
                            outLine = "554 No valid recipients";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SendDATAServerError)
                        {
                            outLine = "451 Local server error";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            CleanupFailedMessageData(ActiveEnvelope);
                            mailObject = null;
                            ActiveEnvelope = null;
                            ActiveEnvelopeRcpts = null;
                            UnauthModeEnabled = false;
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.SendDATAOk)
                        {
                            outLine = "354 End data with <CR><LF>.<CR><LF>";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            sw.Restart();
                            state = SMTPStates.ReceiveDATABlock;
                        }
                        else if (state == SMTPStates.ReceiveDATABlock)
                        {
                            line = lineStream.ReadLine();
                            // Can't use string.IsNullOrEmpty() because empty strings are valid in a message. Only null indicates that no data was received.
                            if (line != null)
                            {
                                if (Verbose)
                                {
                                    if (!VerboseBody && !VerboseBodyStarted)
                                    {
                                        VerboseBodyStarted = true;
                                        try
                                        {
                                            debugWriter.WriteLine(string.Format("C->S: <message body redacted>", line));
                                        }
                                        catch (Exception ex)
                                        {
                                            Worker.ReportProgress(0, new WorkerReport()
                                            {
                                                LogError = string.Format("Verbose Logging failed for {0}. Exception: {1}", ClientIPAddress, ex.Message)
                                            });
                                            Verbose = false;
                                            VerboseBody = false;
                                        }
                                    }
                                    else if (VerboseBody)
                                    {
                                        try
                                        {
                                            debugWriter.WriteLine(string.Format("C->S: {0}", line));
                                        }
                                        catch (Exception ex)
                                        {
                                            Worker.ReportProgress(0, new WorkerReport()
                                            {
                                                LogError = string.Format("Verbose Logging failed for {0}. Exception: {1}", ClientIPAddress, ex.Message)
                                            });
                                            Verbose = false;
                                            VerboseBody = false;
                                        }
                                    }
                                }
                                
                                sw.Restart();
                                bool Handled = false;
                                if (line == ".")
                                {
                                    // end of message.
                                    state = SMTPStates.FinalizeDATABlock;
                                    Handled = true;
                                }
                                if (line.StartsWith(".."))
                                {
                                    line = line.Substring(1);
                                }
                                if (mailObject.ChunkData.Length + line.Length + 4 > MaxChunkSize)
                                {
                                    //insert a chunk for the data we have now.
                                    try
                                    {
                                        byte[] buff = ASCIIEncoding.ASCII.GetBytes(mailObject.ChunkData.ToString());
                                        mailObject.ChunkData.Clear();
                                        SQLiteDB.MailChunk_AddChunk(ActiveEnvelope.EnvelopeID.Value, ActiveEnvelope.ChunkCount, buff);
                                        ActiveEnvelope.ChunkCount++;
                                        SQLiteDB.Envelope_UpdateChunkCount(ActiveEnvelope.EnvelopeID.Value, ActiveEnvelope.ChunkCount);
                                    }
                                    catch (Exception ex)
                                    {
                                        Worker.ReportProgress(0, new WorkerReport()
                                        {
                                            LogError = ex.Message
                                        });
                                        state = SMTPStates.SendDATAServerError;
                                    }
                                }
                                if (!Handled)
                                {
                                    mailObject.MessageSize += line.Length;
                                    if (mailObject.MessageSize > MaxMessageLength)
                                    {
                                        state = SMTPStates.FailDATABlock;
                                    }
                                    else
                                    {
                                        mailObject.ChunkData.AppendLine(line);
                                    }
                                }
                            }
                            else if (sw.ElapsedMilliseconds > CommandTimeoutMS)
                            {
                                outLine = "421 Timout, Closing Connection";
                                WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                                CloseConnection = true;
                                Worker.ReportProgress(0, new WorkerReport()
                                {
                                    LogMessage = string.Format("Timeout receiving mail from {0} [{1}].", ClientHostName, ClientIPAddress)
                                });
                                CleanupFailedMessageData(ActiveEnvelope);
                                mailObject = null;
                                ActiveEnvelope = null;
                                ActiveEnvelopeRcpts = null;
                            }
                        }
                        else if (state == SMTPStates.FailDATABlock)
                        {
                            outLine = "552 Requested mail actions aborted – Exceeded storage allocation";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            Worker.ReportProgress(0, new WorkerReport()
                            {
                                LogMessage = string.Format("Mail length exceeds max message length, for client {0} [{1}].", ClientHostName, ClientIPAddress)
                            });
                            CleanupFailedMessageData(ActiveEnvelope);
                            mailObject = null;
                            ActiveEnvelope = null;
                            ActiveEnvelopeRcpts = null;
                            state = SMTPStates.WaitForClientVerb;
                        }
                        else if (state == SMTPStates.FinalizeDATABlock)
                        {
                            // insert any remaining data that wasn't already handled.
                            if (mailObject.ChunkData.Length > 0)
                            {
                                try
                                {
                                    byte[] buff = ASCIIEncoding.ASCII.GetBytes(mailObject.ChunkData.ToString());
                                    mailObject.ChunkData.Clear();
                                    SQLiteDB.MailChunk_AddChunk(ActiveEnvelope.EnvelopeID.Value, ActiveEnvelope.ChunkCount, buff);
                                    ActiveEnvelope.ChunkCount++;
                                    SQLiteDB.Envelope_UpdateChunkCount(ActiveEnvelope.EnvelopeID.Value, ActiveEnvelope.ChunkCount);
                                }
                                catch (Exception ex)
                                {
                                    Worker.ReportProgress(0, new WorkerReport()
                                    {
                                        LogError = ex.Message
                                    });
                                    state = SMTPStates.SendDATAServerError;
                                }
                            }

                            try
                            {
                                if (UnauthModeEnabled)
                                {
                                    // Add a new MailItem for a newly received item in a user's inbox.
                                    bool AddMaildropSuccess = true;
                                    foreach (var rcp in mailObject.Recipients)
                                    {
                                        tblUser localUser = SQLiteDB.User_GetByEmail(rcp);
                                        if (localUser == null || !localUser.Enabled || !localUser.Maildrop)
                                        {
                                            AddMaildropSuccess = false;
                                        }
                                        else
                                        {
                                            SQLiteDB.MailItem_Insert(localUser.UserID.Value, ActiveEnvelope.EnvelopeID.Value, true);
                                        }
                                    }
                                    if (AddMaildropSuccess)
                                    {
                                        state = SMTPStates.SendDATAAck;
                                    }
                                    else
                                    {
                                        state = SMTPStates.SendDATAServerError;
                                    }
                                }
                                else
                                {
                                    // add a process queue for a new item that will be relayed.
                                    foreach (var envrcp in ActiveEnvelopeRcpts)
                                    {
                                        tblProcessQueue snd = new tblProcessQueue(ActiveEnvelope.EnvelopeID.Value, envrcp.EnvelopeRcptID.Value, 0, 0, DateTime.Now);
                                        SQLiteDB.ProcessQueue_AddUpdate(snd);
                                    }
                                    state = SMTPStates.SendDATAAck;
                                }
                            }
                            catch (Exception ex)
                            {
                                Worker.ReportProgress(0, new WorkerReport()
                                {
                                    LogError = ex.Message
                                });
                                state = SMTPStates.SendDATAServerError;
                            }
                        }
                        else if (state == SMTPStates.SendDATAAck)
                        {
                            outLine = "250 Ok, Message accepted";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            state = SMTPStates.WaitForClientVerb;
                            UnauthModeEnabled = false;
                            mailObject = null;
                            ActiveEnvelope = null;
                            ActiveEnvelopeRcpts = null;
                            ExpectedMessageBytesCount = -1;
                        }
                        else if (state == SMTPStates.ProcessQUITMessage)
                        {
                            outLine = "221 Bye";
                            WriteLineWithDebugOptions(lineStream, outLine, ref Verbose, debugWriter, ClientIPAddress);
                            CloseConnection = true;
                        }
                    }
                }
                finally
                {
                    if (debugWriter != null)
                    {
                        try
                        {
                            debugWriter.Dispose();
                            debugWriter = null;
                        }
                        catch { }
                    }
                    if (stream != null)
                    {
                        try
                        {
                            stream.Close();
                            stream = null;
                        }
                        catch { }
                    }
                    if (client != null)
                    {
                        try
                        {
                            client.Close();
                            client.Dispose();
                            client = null;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                e.Result = new WorkerReport()
                {
                    LogError = ex.Message
                };
                if (debugWriter != null)
                {
                    try
                    {
                        debugWriter.Dispose();
                        debugWriter = null;
                    }
                    catch { }
                }
            }
        }

        private void WriteLineWithDebugOptions(ISMTPStream lineStream, string line, ref bool debug, TextWriter debugWriter, string clientIPAddress)
        {
            if (debug)
            {
                try
                {
                    debugWriter.WriteLine(String.Format("S->C: {0}", line));
                }
                catch (Exception ex)
                {
                    Worker.ReportProgress(0, new WorkerReport()
                    {
                        LogError = string.Format("Verbose Logging failed for {0}. Exception: {1}", clientIPAddress, ex.Message)
                    });
                    debug = false;
                }
            }
            lineStream.WriteLine(line);
        }

        private X509Certificate2 FindSystemCert(string friendlyName)
        {
            X509Certificate2 serverCert = null;
            var store = GetLocalCerts();
            foreach (var cert in store.Cast<X509Certificate2>())
            {
                if (cert.FriendlyName == friendlyName)
                {
                    serverCert = cert;
                }
            }
            return serverCert;
        }

        private X509Certificate2Collection GetLocalCerts()
        {
            var localMachineStore = new X509Store(StoreLocation.LocalMachine);
            localMachineStore.Open(OpenFlags.ReadOnly);
            var certs = localMachineStore.Certificates;
            localMachineStore.Close();
            return certs;
        }
        private void CleanupFailedMessageData(tblEnvelope activeEnvelope)
        {
            if (activeEnvelope == null)
            {
                return;
            }
            try
            {
                // message too long. delete the message.
                SQLiteDB.MailChunk_DeleteMailData(activeEnvelope.EnvelopeID.Value);
            }
            catch (TimeoutException)
            {
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogWarning = "Timeout exceeded. Terminating connection."
                });
            }
            catch (Exception ex)
            {
                Worker.ReportProgress(0, new WorkerReport()
                {
                    LogError = ex.Message
                });
            }
        }

        private string BASE64Decode(string code)
        {
            try
            {
                return ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(code));
            }
            catch
            {
                return null;
            }
        }

        private string FormatRecipients(List<string> rcpts)
        {
            StringBuilder sb = new StringBuilder();
            bool first = true;
            foreach (var rcpt in rcpts)
            {
                if (!first)
                {
                    sb.Append("; ");
                }
                else
                {
                    first = false;
                }
                sb.Append(rcpt);
            }
            return sb.ToString();
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

    }
}
