using SMTPRelay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay
{
    /// <summary>
    /// Sends queued item to a remote SMTP server, logs the result.
    /// </summary>
    public class SmtpClient
    {
        private BackgroundWorker worker;
        public SmtpClient(SendQueue item)
        {
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.RunWorkerAsync(item);
        }

        private enum HostState
        {
            NotConnected,           // no connection to smart host, or a previous connection has closed/failed.
            WaitHostWelcome,        // connection established, waiting for smart host to send welcome greeting
            SendHello,              // send the helo command to the host.
            WaitHost250Hello,       // wait for the host to send a 250 Hello. It might come back with a 500 or 550 first. We should retry at least 3 times before giving up.
            SendAUTHLOGIN,          // sends the auth login to authenticate.
            WaitAuthUserPrompt,     // Wait for the host to prompt for the username.
            WaitAuthPassPrompt,     // wait for the host to prompt for the password.
            WaitAuthSuccessful,     // wait for the host to reply 235 authentication successful
            HostConnected,          // we're connected. Send Mail From whenver you are ready
            HostConnectionFailure,  // connection to smart host has failed.
            HostServiceUnavailable, // got a 521/421. Tell client unavailable, and hang up.
            //HostAuthenticationFail,  // Authentication has failed.
            SendMailFrom,

            Quitting                // Send QUIT. Wiating for reply
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            SendQueue item = e.Argument as SendQueue;
            if (item == null)
            {
                e.Result = new StatusMessage("Failed to process SendQueue Item. No argument provided.", MessagePriority.Error);
            }
            Envelope env = SQLiteDB.GetEnvelopeByID(item.EnvelopeID);
            if (env == null)
            {
                // no envelope associated with this SendQueue
                // make SendLog entry
                SQLiteDB.SendLogAddLogEntry(item.EnvelopeID, item.Recipient, "Failure: no envelope found.", item.AttemptCount);
                // delete SendQueue entry
                SQLiteDB.SendQueueDeleteByID(item.SendQueueID);
                e.Result = new StatusMessage(string.Format("Failed to send to <{0}>. No envelope found. ", item.Recipient), MessagePriority.Error);
                return;
            }
            if (env.ChunkCount == 0)
            {
                // Envelope is empty; no message body.
                // make SendLog entry
                SQLiteDB.SendLogAddLogEntry(item.EnvelopeID, item.Recipient, "Failure: envelope is empty.", item.AttemptCount);
                // delete SendQueue entry
                SQLiteDB.SendQueueDeleteByID(item.SendQueueID);
                e.Result = new StatusMessage(string.Format("Failed to send from <{0}> to <{1}>. Envelope was empty.", env.Sender, item.Recipient), MessagePriority.Error);
                return;
            }
            long ActualChunkCount = SQLiteDB.MailChunkGetCountForEnvelope(env.EnvelopeID);
            if (ActualChunkCount != env.ChunkCount)
            {
                // unexpected chunk count
                // make SendLog entry
                SQLiteDB.SendLogAddLogEntry(item.EnvelopeID, item.Recipient, "Failure: envelope is incorrect size.", item.AttemptCount);
                // delete SendQueue entry
                SQLiteDB.SendQueueDeleteByID(item.SendQueueID);
                e.Result = new StatusMessage(string.Format("Failed to send from <{0}> to <{1}>. Unexpected envelope chunk count.", env.Sender, item.Recipient), MessagePriority.Error);
                return;
            }

            // establish a connection to the smart host and log in
            Stopwatch hostTimeout = new Stopwatch();
            // Connection to smart host for when we need that.
            TcpClient SmartHost = null;
            // net stream for the smart host, when we need that.
            NetworkStream shStream = null;
            // set initial state for the state machine that tracks everything
            HostState hState = HostState.NotConnected;  // not connected to smart host
            int shHelloRetries = 0;     // how many times to retry Smart Host HELO before giving up entirely.

            // buffer for smart host reading
            byte[] shRead = new byte[4096];
            int shReadPos = 0;

            bool SendFinished = false;

            try
            {
                string str = null;  // we're going to do a lot of buffer to string converstion, so reuse this string

                while (!worker.CancellationPending && !SendFinished)
                {
                    System.Threading.Thread.Sleep(10);
                    // read from buffers if data is available. We don't handle the data here, just receive it until buffer for now.
                    if (shStream != null && shStream.DataAvailable)
                    {
                        int len = shStream.Read(shRead, shReadPos, shRead.Length - shReadPos);
                        shReadPos += len;   // add in the number of bytes we just read to our position of the next character to read at.
                    }
                    // state machin that handles the converstaion, and sending of previously read buffers of data.
                    
                    // we keep coming here until host is ready to rock.
                    // or we fail and send a fatal error to client.
                    // hState is status of the host state machine. after connecting for first email, we keep it alive until clint goes away to save time.
                    if (hostTimeout.ElapsedMilliseconds > StaticConfiguration.MaxHostTimeoutms)
                    {
                        // 30 seconds to try to connect to smart host is plenty of time. it's not going to happen.
                        hostTimeout.Reset();
                        hState = HostState.HostConnectionFailure;
                    }
                    switch (hState)
                    {
                        case HostState.NotConnected:
                            // start connecting now
                            try
                            {
                                hostTimeout.Restart();
                                SmartHost = new TcpClient(StaticConfiguration.SmartHost.EndPoint.Address, StaticConfiguration.SmartHost.EndPoint.Port);
                                shStream = SmartHost.GetStream();
                                hState = HostState.WaitHostWelcome;
                            }
                            catch// (Exception ex)
                            {
                                hState = HostState.HostConnectionFailure;
                            }
                            break;
                        case HostState.WaitHostWelcome:
                            // connection established, waiting for host welcome message
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("220 "))
                                {
                                    hState = HostState.SendHello;
                                }
                                else
                                {
                                    hState = HostState.HostConnectionFailure;
                                }
                            }
                            break;
                        case HostState.SendHello:
                            // got a 220 message. Send a HELO
                            str = string.Format("HELO {0}\r\n", StaticConfiguration.ThisHostName);
                            WriteStringToStream(str, shStream);
                            hState = HostState.WaitHost250Hello;
                            break;
                        case HostState.WaitHost250Hello:
                            // see if the server replies with 250 (okay), 500/501/504 (retry up to 3 times), 521/421 unavailable.
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("250 "))
                                {
                                    // got a good response
                                    hState = HostState.SendAUTHLOGIN;
                                    shHelloRetries = 0;
                                }
                                else if (str.StartsWith("521") || str.StartsWith("421"))
                                {
                                    hState = HostState.HostServiceUnavailable;
                                }
                                else if (str.StartsWith("500") || str.StartsWith("501") || str.StartsWith("504"))
                                {
                                    shHelloRetries += 1;
                                    if (shHelloRetries < 3)
                                    {
                                        hState = HostState.SendHello;
                                    }
                                    else
                                    {
                                        hState = HostState.HostConnectionFailure;
                                    }
                                }
                                else
                                {
                                    hState = HostState.HostConnectionFailure;
                                }
                            }
                            break;
                        case HostState.SendAUTHLOGIN:
                            // send login command.
                            str = "AUTH LOGIN\r\n";
                            WriteStringToStream(str, shStream);
                            hState = HostState.WaitAuthUserPrompt;
                            break;
                        case HostState.WaitAuthUserPrompt:
                            // waiting for host to send username prompt.
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("334 VXNlcm5hbWU6"))
                                {
                                    // send username
                                    str = string.Format("{0}\r\n", Base64(StaticConfiguration.SmartHost.Credentials.Username));
                                    WriteStringToStream(str, shStream);
                                    hState = HostState.WaitAuthPassPrompt;
                                }
                                else
                                {
                                    shHelloRetries += 1;
                                    if (shHelloRetries < 3)
                                    {
                                        hState = HostState.SendAUTHLOGIN;
                                    }
                                    else
                                    {
                                        hState = HostState.HostConnectionFailure;
                                    }
                                }
                            }
                            break;
                        case HostState.WaitAuthPassPrompt:
                            // waiting for host to send password prompt.
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("334 UGFzc3dvcmQ6"))
                                {
                                    // send password
                                    str = string.Format("{0}\r\n", Base64(StaticConfiguration.SmartHost.Credentials.Password));
                                    WriteStringToStream(str, shStream);
                                    hState = HostState.WaitAuthSuccessful;
                                }
                                else
                                {
                                    hState = HostState.SendAUTHLOGIN;
                                }
                            }
                            break;
                        case HostState.WaitAuthSuccessful:
                            // waiting for host to reply with 235 auth successful
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("535 "))
                                {
                                    // authentication failed. we were provided with bad credentials, and cannot continue with this smart host.
                                    SQLiteDB.SendLogAddLogEntry(item.EnvelopeID, item.Recipient, "Smart Host Authentication Failure", item.AttemptCount);
                                    item.State = SendQueue.SendQueueStates.SmartHostPasswordFail;
                                    SQLiteDB.SendQueueUpdate(item);
                                    e.Result = new StatusMessage("Failed to authenticate to the Smart Host.", MessagePriority.Error);
                                    return;
                                }
                                else if (str.StartsWith("235 "))
                                {
                                    // we are authenticated
                                    hState = HostState.HostConnected;
                                }
                                else
                                {
                                    hState = HostState.HostConnectionFailure;
                                }
                            }
                            break;
                        case HostState.HostConnectionFailure:
                            //something has gone wrong. Log the event. Calculate next retry
                            SQLiteDB.SendLogAddLogEntry(item.EnvelopeID, item.Recipient, "Smart Host Connection Failure.", item.AttemptCount);
                            item.State = SendQueue.SendQueueStates.Ready;
                            string delay = CalculateRetryOffset((int)item.AttemptCount);
                            if (delay == null)
                            {
                                SQLiteDB.SendLogAddLogEntry(item.EnvelopeID, item.Recipient, string.Format("Last Retry, giving up. Envelope {0} Recipient {1}", item.EnvelopeID, item.Recipient), item.AttemptCount);
                                SQLiteDB.SendQueueDeleteByID(item.SendQueueID);
                            }
                            else
                            {
                                item.RetryDelay = delay;
                                SQLiteDB.SendQueueUpdate(item);
                            }

                            // clean up.
                            try
                            {
                                shStream.Dispose();
                                shStream = null;
                            }
                            catch { }
                            try
                            {
                                SmartHost.Dispose();
                                SmartHost = null;
                            }
                            catch { }
                            return;
                        case HostState.HostServiceUnavailable:
                            // smart host unavailable. terminate channel
                            //something has gone wrong. Log the event. Calculate next retry
                            SQLiteDB.SendLogAddLogEntry(item.EnvelopeID, item.Recipient, "Smart Host Service Unavailable.", item.AttemptCount);
                            item.State = SendQueue.SendQueueStates.Ready;
                            string delayu = CalculateRetryOffset((int)item.AttemptCount);
                            if (delayu == null)
                            {
                                SQLiteDB.SendLogAddLogEntry(item.EnvelopeID, item.Recipient, string.Format("Last Retry, giving up. Envelope {0} Recipient {1}", item.EnvelopeID, item.Recipient), item.AttemptCount);
                                SQLiteDB.SendQueueDeleteByID(item.SendQueueID);
                            }
                            else
                            {
                                item.RetryDelay = delayu;
                                SQLiteDB.SendQueueUpdate(item);
                            }
                            return;
                        case HostState.HostConnected:
                            hState = HostState.SendMailFrom;
                            break;
                        case HostState.SendMailTo:
                            str = "MAIL TO: \r\n";
                            WriteStringToStream(str, shStream);
                            hState = HostState.WaitAuthUserPrompt;
                            break;
                    }
                }

                if (worker.CancellationPending)
                {
                    SQLiteDB.SendLogAddLogEntry(item.EnvelopeID, item.Recipient, "Service stopping. Aborting item.", item.AttemptCount);
                    item.AttemptCount--;
                    item.State = SendQueue.SendQueueStates.Ready;
                    string delay = "+2 minutes";
                    item.RetryDelay = delay;
                    SQLiteDB.SendQueueUpdate(item);
                    return;
                }


                // specify the recipient

                // if rejected, log and queue for retry

                // Start writing out chunks, querying SQLite for each as we go.

                // If error at any point, abort the connection and log the error. Mark the SendQueue item for retry

                // If successfully sent, get the receipt and log success. Delete the SendQueue item.

            }
            catch (Exception ex)
            {
                e.Result = new StatusMessage(ex.Message, MessagePriority.Error);
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                if (shStream != null)
                {
                    try
                    {
                        shStream.Dispose();
                    }
                    catch { }
                }

                if (SmartHost != null)
                {
                    try
                    {
                        SmartHost.Close();
                    }
                    catch { }
                    try
                    {
                        SmartHost.Dispose();
                    }
                    catch { }
                }

            }

        }

        private string CalculateRetryOffset(int attempts)
        {
            switch (attempts)
            {
                case 1:
                    return "+5 minutes";
                case 2:
                    return "+15 minutes";
                case 3:
                    return "+30 minutes";
                case 4:
                    return "+1 hour";
                case 5:
                    return "+4 hours";
                case 6:
                    return "+6 hours";
                case 7:
                    return "+6 hours";
                case 8:
                    return "+12 hours";
                case 9:
                    return "+1 day";
                case 10:
                    return "+1 day";
                case 11:
                    return "+1 day";
                case 12:
                    return "+1 day";
                default:
                    return null;
            }
        }

        // reads a string from the buffer, and prepares the buffer for the next read. 
        // returns null if nothing to read.
        private string BufferReadLine(ref byte[] buff, ref int pos)
        {
            int p = 0;
            string result = null;
            while (p + 1 < pos)
            {
                if (buff[p] == '\r' && buff[p + 1] == '\n')
                {
                    // pointing to the \r, and +1 is \n. Since zero based index, using the \r index as the count will read up to one char before the \r, omitting the newline
                    result = Encoding.ASCII.GetString(buff, 0, p);
                    p++;    // increment to point to the \n (the end of the line)
                    if (pos - p <= 1)   // pos is the next character in the buffer to write (not actually a character to read). Subtract position of the \n, and it should be 1 if the index after the \n is where we will write to next
                    {
                        // So, we just read out the entire buffer. Clear all that out and reset buffer write position.
                        Array.Clear(buff, 0, pos);
                        pos = 0;
                        break;
                    }
                    else
                    {
                        // since there is data beyond the newline char, we need to shift that to the start of the array.
                        p++;    // increment to point to first char in next string line
                        byte[] temp = new byte[buff.Length];
                        Array.Copy(buff, p, temp, 0, pos - p);
                        buff = temp;
                        pos = pos - p; // move the index of next write position left by the index we just shifted to zero. So now we point to the same character, but at the new shifted offset.
                        break;
                    }
                }
                p++;
            }
            if (pos == buff.Length)
            {
                // we filled the buffer but couldn't find any new lines?  Wow. That sucks.  Flush it all as text and hope that we don't split up any character bytes. ASCII should be single byte per character
                // this is really a buffer overflow that could cause problems, but if the SMTP conversation has not gone off the rails, it *should* never happen.
                result = Encoding.ASCII.GetString(buff, 0, pos);
                Array.Clear(buff, 0, pos);
                pos = 0;
            }
            if (result != null)
            {
                Debug.WriteLine(result);
            }
            return result;
        }

        // writes a buffer of bytes to a stream, leaving the buffer and position pointer alone.
        private void WriteBufferToStream(ref byte[] buff, int pos, NetworkStream stream)
        {
            stream.Write(buff, 0, pos);
            return;
        }

        // writes a string as ASCII bytes to a stream.
        private void WriteStringToStream(string str, NetworkStream stream)
        {
            Debug.WriteLine(str);
            byte[] buff = Encoding.ASCII.GetBytes(str);
            WriteBufferToStream(ref buff, buff.Length, stream);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusMessage msg = e.UserState as StatusMessage;
            if (msg != null)
            {
                Messages.Enqueue(msg);
            }
        }
        private string Base64(string data)
        {
            var plainTextBytes = Encoding.ASCII.GetBytes(data);
            return Convert.ToBase64String(plainTextBytes);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusMessage msg = e.Result as StatusMessage;
            if (msg != null)
            {
                Messages.Enqueue(msg);
            }
            Done = true;
        }

        public Queue<StatusMessage> Messages = new Queue<StatusMessage>();

        public bool Done { get; private set; }

        public void Dispose()
        {
            try
            {
                if (worker.IsBusy)
                {
                    worker.CancelAsync();
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    while (worker.IsBusy && sw.ElapsedMilliseconds < 5000)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                }
            }
            catch { }
        }
    }
}
