/*
 * using SMTPRelay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

<<<<<<< HEAD
=======
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
            SendMailFrom,           // send a MAIL FROM to the smart host.
            WaitHostMailFromResp,   // wait for response to MAIL FROM command from host
            SendRcptTo,             // Send the RCPT message
            WaitHostRcptResponse,   // Wait for the host to respond to the recipient
            Quitting                // Send QUIT. Wiating for reply
        }

>>>>>>> a49b241 (Start adding sending functions.)
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            SendQueue QueueItem = e.Argument as SendQueue;
            if (QueueItem == null)
            {
                e.Result = new StatusMessage("Failed to process SendQueue Item. No argument provided.", MessagePriority.Error);
            }
<<<<<<< HEAD
=======
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
                        case HostState.SendMailFrom:
                            // we are connected to the smart host. Go ahead and send that MAIL FROM now.
                            str = string.Format("MAIL FROM: {0}\r\n", item.Recipient);
                            WriteStringToStream(str, shStream);
                            hState = HostState.WaitHostMailFromResp;
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
>>>>>>> a49b241 (Start adding sending functions.)


            

        }
        
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusMessage msg = e.UserState as StatusMessage;
            if (msg != null)
            {
                Messages.Enqueue(msg);
            }
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

*/