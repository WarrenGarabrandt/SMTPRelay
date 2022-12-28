using SMTPRelay.Database;
using SMTPRelay.Model;
using SMTPRelay.Model.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
            sendQueueItem.AttemptCount ++;

            TcpClient client = null;
            NetworkStream stream = null;
            ISMTPStream smtpStream = null;
            string finalResults = null;
            string MsgIdentifier = string.Empty;
            bool PermanentFailure = false;


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
                }
                else
                {
                    PermanentFailure = true;
                    throw new NotImplementedException("Looking up domain MX recoreds is not implemented.");
                }

                if (!string.IsNullOrEmpty(gateway.SenderOverride))
                {
                    HeaderReplaceSender = true;
                    MailFromAddress = gateway.SenderOverride;
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
                        throw new Exception(string.Format("Querying DNS failed for [{0}]. {1}", serverHostname, ex.Message));
                    }
                }

                // attempt to connect to the server
                if (serverEPs.Count == 0)
                {
                    throw new Exception("Unable to connect to SMTP server. No End Point could be resolved.");
                }

                client = null;
                foreach (var ep in serverEPs)
                {
                    try
                    {
                        client = new TcpClient();
                        client.Connect(ep);
                        break;
                    }
                    catch
                    {
                        client = null;
                    }
                }

                if (client == null)
                {
                    throw new Exception("Unable to connect to the SMTP Server.");
                }

                stream = client.GetStream();
                smtpStream = new SMTPStreamHandler(stream);

                // negotiate the connection and switch to TLS if the gateway specifies requirements to do so.
                string line = smtpStream.ReadLine(30000, Worker);
                if (string.IsNullOrEmpty(line))
                {
                    throw new Exception("Timeout initiating connection to SMTP server.");
                }

                if (!line.StartsWith("220 "))
                {
                    throw new Exception(string.Format("Unexpected welcome message: {0}", line));
                }
                // we got a 220 hello message.
                // send the HELO message
                smtpStream.WriteLine(string.Format("EHLO {0}", localHostname));
                

                // Get the server extensions (250-) and the OK message (250)
                while ((line = smtpStream.ReadLine(30000, Worker)).StartsWith("250-"))
                {
                    if (Worker.CancellationPending)
                    {
                        throw new OperationCanceledException();
                    }
                    if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                    {
                        throw new Exception("Total Process Timeout Exceeded for the connection.");
                    }
                    SMTPExtensions.Add(line);
                    if (swTimeout.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                    {
                        throw new Exception("Timeout Exceeded for the connection.");
                    }
                }
                if (string.IsNullOrEmpty(line))
                {
                    throw new Exception("Timeout initiating connection to SMTP server.");
                }
                SMTPExtensions.Add(line);  // sometimes the last message is Ok, continue, or some nice message. Often, it's the last item in the extensions list

                if (!line.StartsWith("250 "))
                {
                    // didn't get expected 250 message
                    throw new Exception(string.Format("Server didn't reply as expected to EHLO. Got: {0}", line));
                }
                if (useTLS)
                {
                    smtpStream.WriteLine("STARTTLS");
                    // Possible responses
                    // 220 Ready to start TLS
                    // 501 Syntax error (no parameters allowed)
                    // 454 TLS not available due to temporary reason
                    line = smtpStream.ReadLine(30000, Worker);
                    if (string.IsNullOrEmpty(line))
                    {
                        throw new Exception("Timeout enabling TLS.");
                    }
                    else if (line.StartsWith("501 "))
                    {
                        throw new Exception("Server doesn't support TLS.");
                    }
                    else if (line.StartsWith("454 "))
                    {
                        throw new Exception("TLS not available at this time.");
                    }
                    else if (!line.StartsWith("220 "))
                    {
                        throw new Exception(string.Format("Unexpected server response to STARTTLS: {0}", line));
                    }
                    // switch to TLS now
                    smtpStream.Release();
                    smtpStream = null;
                    smtpStream = new SMTPTLSStreamHandler(stream, SMTPTLSStreamHandler.Mode.Client, serverHostname, null);
                    // send the HELO message. This essentially starts the conversation over.
                    smtpStream.WriteLine(string.Format("EHLO {0}", localHostname));
                    SMTPExtensions.Clear();  // we might get different extensions now that we're in TLS mode.
                    while ((line = smtpStream.ReadLine(30000, Worker)).StartsWith("250-"))
                    {
                        if (Worker.CancellationPending)
                        {
                            throw new OperationCanceledException();
                        }
                        if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                        {
                            throw new Exception("Total Process Timeout Exceeded for the connection.");
                        }
                        SMTPExtensions.Add(line);
                        if (swTimeout.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                        {
                            throw new Exception("Timeout Exceeded for the connection.");
                        }
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        throw new Exception("Timeout sending EHLO after STARTTLS.");
                    }
                    SMTPExtensions.Add(line);  // sometimes the last message is Ok, continue, or some nice message. Often, it's the last item in the extensions list

                    if (!line.StartsWith("250 "))
                    {
                        // didn't get expected 250 message
                        throw new Exception(string.Format("Server didn't reply as expected to EHLO. Got: {0}", line));
                    }
                }
                //else
                // Server may send "530 Must issue a STARTTLS command first" to anything other than NOOP, EHLO, STARTTLS, or QUIT.

                // handle authentication
                if (login)
                {
                    smtpStream.WriteLine("AUTH LOGIN");
                    line = smtpStream.ReadLine(30000, Worker);
                    if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                    {
                        throw new Exception("Total Process Timeout Exceeded for the connection.");
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        throw new Exception("Timeout authenticating to SMTP server.");
                    }
                    else if (line.StartsWith("530 "))
                    {
                        throw new Exception("Must use STARTTLS before authenticating.");
                    }
                    else if (line != "334 VXNlcm5hbWU6")
                    {
                        throw new Exception(string.Format("Authentication not supported. {0}", line));
                    }
                    smtpStream.WriteLine(BASE64Encode(gateway.Username));
                    line = smtpStream.ReadLine(30000, Worker);
                    if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                    {
                        throw new Exception("Total Process Timeout Exceeded for the connection.");
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        throw new Exception("Timeout authenticating to SMTP server.");
                    }
                    else if (line.StartsWith("530 "))
                    {
                        throw new Exception("Must use STARTTLS before authenticating.");
                    }
                    else if (line != "334 UGFzc3dvcmQ6")
                    {
                        throw new Exception(string.Format("Authentication not supported. {0}", line));
                    }
                    smtpStream.WriteLine(BASE64Encode(gateway.Password));
                    line = smtpStream.ReadLine(30000, Worker);
                    if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                    {
                        throw new Exception("Total Process Timeout Exceeded for the connection.");
                    }
                    if (string.IsNullOrEmpty(line))
                    {
                        throw new Exception("Timeout authenticating to SMTP server.");
                    }
                    else if (line.StartsWith("530 "))
                    {
                        throw new Exception("Must use STARTTLS before authenticating.");
                    }
                    else if (line.StartsWith("535 "))
                    {
                        throw new Exception(string.Format("Authentication not successful for user {0} on gateway {1}. Message: {2}", gateway.SMTPServer, gateway.Username, line));
                    }
                    else if (!line.StartsWith("235 "))
                    {
                        throw new Exception(string.Format("Authentication not successful for user {0} on gateway {1}. Message: {2}", gateway.SMTPServer, gateway.Username, line));
                    }
                }

                // negotiate the MAIL FROM
                smtpStream.WriteLine(string.Format("MAIL FROM: {0}", EscapeEmailAddress(MailFromAddress)));
                line = smtpStream.ReadLine(30000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    throw new Exception("Total Process Timeout Exceeded for the connection.");
                }
                if (string.IsNullOrEmpty(line))
                {
                    throw new Exception("Timeout negotiating MAIL FROM.");
                }
                else if (line.StartsWith("550 "))
                {
                    throw new Exception(string.Format("Authentication required or relay not permitted for {0}", MailFromAddress));
                }
                else if (!line.StartsWith("250 "))
                {
                    throw new Exception(string.Format("Can't send as {0}. {1}", MailFromAddress, line));
                }

                // and now the RCPT TO
                smtpStream.WriteLine(string.Format("RCPT TO: {0}", EscapeEmailAddress(RcptToAddress)));
                line = smtpStream.ReadLine(30000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    throw new Exception("Total Process Timeout Exceeded for the connection.");
                }
                if (string.IsNullOrEmpty(line))
                {
                    throw new Exception("Timeout negotiating RCPT TO.");
                }
                else if (!line.StartsWith("250 ") && !line.StartsWith("251 "))
                {
                    throw new Exception(string.Format("Can't send as {0}. {1}", RcptToAddress, line));
                }

                // Signal that we are done with the envelope and ready to send the message
                smtpStream.WriteLine("DATA");
                line = smtpStream.ReadLine(30000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    throw new Exception("Total Process Timeout Exceeded for the connection.");
                }
                if (string.IsNullOrEmpty(line))
                {
                    throw new Exception("Timeout negotiating DATA stream.");
                }
                else if (!line.StartsWith("250 ") && !line.StartsWith("354 "))
                {
                    throw new Exception(string.Format("Can't send email to {0}. {1}", RcptToAddress, line));
                }

                // send the data
                bool FromDone = false;
                //bool MIMEFormat = false;
                for (int i = 0; i < envelope.ChunkCount; i++)
                {
                    if (Worker.CancellationPending)
                    {
                        throw new OperationCanceledException();
                    }
                    byte[] datablock = SQLiteDB.MailChunk_GetChunk(envelope.EnvelopeID.Value, i);
                    string datastring = ASCIIEncoding.ASCII.GetString(datablock);
                    List<string> datalines = datastring.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
                    foreach (string dl in datalines)
                    {
                        string outputLine = dl;
                        if (HeaderReplaceSender && !FromDone && outputLine.ToUpper().StartsWith("FROM:"))
                        {
                            FromDone = true;
                            outputLine = string.Format("FROM: {0}", EscapeEmailAddress(MailFromAddress));
                        }
                        else if (outputLine.StartsWith("."))
                        {
                            outputLine = string.Format(".{0}", outputLine);
                        }
                        smtpStream.WriteLine(outputLine);
                        if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                        {
                            throw new Exception("Total Process Timeout Exceeded for the connection.");
                        }
                        if (swTimeout.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                        {
                            throw new Exception("Timeout Exceeded for the connection.");
                        }
                    }
                }
                // end the message with /r/n./r/n
                smtpStream.WriteLine(".");

                // get a final ack
                line = smtpStream.ReadLine(30000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    throw new Exception("Total Process Timeout Exceeded for the connection.");
                }
                if (string.IsNullOrEmpty(line))
                {
                    throw new Exception("Timeout finalizing message. No ACK received.");
                }
                else if (!line.StartsWith("250 "))
                {
                    if (line.StartsWith("554"))
                    {
                        // SendAsDenied
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
                smtpStream.WriteLine("QUIT");
                line = smtpStream.ReadLine(10000, Worker);
                if (HardTimeLimit.ElapsedMilliseconds > CONNECTIONTIMEOUT)
                {
                    throw new Exception("Total Process Timeout Exceeded for the connection.");
                }
                if (string.IsNullOrEmpty(line))
                {
                    //we don't actually need to wait for a reply to a QUIT. We can hang up.
                }
                else if (!line.StartsWith("221 "))
                {
                    Worker.ReportProgress(0, new WorkerReport()
                    {
                        LogMessage = string.Format("{0}Unexpected response to QUIT. {1}", MsgIdentifier, line)
                    }); ;
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
