using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace SMTPRelay
{
    public class SmtpClient : IDisposable
    {
        public SmtpClient(TcpClient client)
        {
            worker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerAsync(client);
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            StatusMessage msg = e.UserState as StatusMessage;
            if (msg != null)
            {
                Messages.Enqueue(msg);
            }
        }

        public enum MessagePriority
        {
            Information,
            Warning,
            Error
        }

        public class StatusMessage
        {
            public StatusMessage(string message, MessagePriority priority)
            {
                Message = message;
                Priority = priority;
            }
            public string Message { get; set; }
            public MessagePriority Priority { get; set; }
        }

        public Queue<StatusMessage> Messages = new Queue<StatusMessage>();

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusMessage msg = e.Result as StatusMessage;
            if (msg != null)
            {
                Messages.Enqueue(msg);
            }
            Done = true;
        }

        private enum State
        {
            WelcomeClient,      // Accepted a client connection. We need to say hello with them.
            WaitClientHello,    // waiting for client to say hello
            WaitClientMAILFROM, // waiting for client to send MAIL FROM command. At this point, we need to initiate a connection to the smart host.
            ConnectToSmartHost, // we have a MAIL FROM item, so connect to smart host.
            SendMAILFROM,       // send a MAIL FROM to the smart host.
            WaitHostMailFromResp, // wait for response to MAIL FROM command from host
            WaitClientRCPTTO,   // wait for client to send RCPT TO command
            WaitHostRCPTResp,   // wait for host RCPT response
            WaitClientRCPTorDATA,   // wait for client to send a RCPT or a DATA command.
            WaitHostDATAResp,       // wait for Host to respond to DATA command.
            TransferDATA,           // we are relaying data from the client to the host, and from the host to the client now.
            WaitHostDataEndResp,    // we are waiting for a host response to the end of the DATA block
            WaitHostQuitResp,       // client sent quit. Wait for host to respond.
            CloseChannel,        // close channel
            MailFromTimeout     // we've timed out waiting for MAIL FROM command. Quit if we're connected to smart host.
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
            HostAuthenticationFail,  // Authentication has failed.
            Quitting                // Send QUIT. Wiating for reply
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch clientTimeout = new Stopwatch();
            Stopwatch hostTimeout = new Stopwatch();
            Stopwatch EndOfCallTimer = new Stopwatch();
            clientTimeout.Start();
            EndOfCallTimer.Reset();
            // get our incoming client connection
            TcpClient Client = e.Argument as TcpClient;
            // Connection to smart host for when we need that.
            TcpClient SmartHost = null;
            // net stream for the smart host, when we need that.
            NetworkStream shStream = null;
            // net stream for the client, which we'll grab straight away after entering the try below
            NetworkStream cStream = null;
            // set initial state for the state machine that tracks everything

            State state = State.WelcomeClient;    // Accepted a client connection. We need to say hello with them.
            HostState hState = HostState.NotConnected;  // not connected to smart host
            string ClientName = null;   // name of the client that connects
            string MessageMailFrom = null;  // from address of this mail item.
            string MessageLastRecipient = null;
            List<string> MessageRecipients = new List<string>();
            int BadMessages = 0;        // how many bad messages has the client sent. To many = disconnect
            int shHelloRetries = 0;     // how many times to retry Smart Host HELO before giving up entirely.

            // buffer for smart host reading
            byte[] shRead = new byte[4096];
            int shReadPos = 0;

            // buffer for client reading
            byte[] cRead = new byte[4096];
            int cReadPos = 0;

            bool AllDone = false;

            try
            {
                cStream = Client.GetStream();
                
                while (!worker.CancellationPending && clientTimeout.ElapsedMilliseconds < StaticConfiguration.MaxClientTimeoutms && BadMessages < 20 && !AllDone)
                {
                    Thread.Sleep(1);
                    // read from buffers if data is available. We don't handle the data here, just receive it until buffer for now.
                    if (shStream != null && shStream.DataAvailable)
                    {
                        int len = shStream.Read(shRead, shReadPos, shRead.Length - shReadPos);
                        shReadPos += len;   // add in the number of bytes we just read to our position of the next character to read at.
                    }
                    if (cStream != null && cStream.DataAvailable)
                    {
                        int len = cStream.Read(cRead, cReadPos, cRead.Length - cReadPos);
                        cReadPos += len;
                        clientTimeout.Restart();
                    }
                    // state machin that handles the converstaion, and sending of previously read buffers of data.
                    string str = null;  // we're going to do a lot of buffer to string converstion, so reuse this string
                    switch (state)
                    {
                        case State.WelcomeClient:
                            str = string.Format("220 {0} Smtp Relay Agent ready at {1}\r\n", StaticConfiguration.ThisHostName, DateTime.Now.ToString("ddd, d MMM yyyy HH:mm:ss zzz"));
                            WriteStringToStream(str, cStream);
                            state = State.WaitClientHello;
                            break;
                        case State.WaitClientHello:
                            str = BufferReadLine(ref cRead, ref cReadPos);
                            if (str != null)
                            {
                                // let's see what client said
                                if (str.StartsWith("HELO") || str.StartsWith("EHLO"))
                                {
                                    // client is saying hello
                                    if (str.Length > 5)
                                    {
                                        ClientName = str.Substring(5);
                                    }
                                    else
                                    {
                                        ClientName = "anonymous";
                                    }
                                    str = string.Format("250 {0} Hello {1}, very nice to meet you.\r\n", StaticConfiguration.ThisHostName, ClientName);
                                    WriteStringToStream(str, cStream);
                                    state = State.WaitClientMAILFROM;
                                    EndOfCallTimer.Restart();
                                }
                                else
                                {
                                    // don't understand
                                    BadMessages++;
                                    str = "500 Command not implemented\r\n";
                                    WriteStringToStream(str, cStream);
                                }
                            }
                            break;
                        case State.WaitClientMAILFROM:
                            str = BufferReadLine(ref cRead, ref cReadPos);
                            if (str != null)
                            {
                                MessageRecipients.Clear();
                                MessageMailFrom = "";
                                EndOfCallTimer.Restart();
                                if (str.StartsWith("MAIL FROM:") && str.Length > 10)
                                {
                                    MessageMailFrom = str.Substring(10).Trim();
                                    state = State.ConnectToSmartHost;
                                }
                                else if (str.StartsWith("QUIT"))
                                {
                                    str = "221 Bye\r\n";
                                    WriteStringToStream(str, cStream);
                                    if (hState == HostState.HostConnected)
                                    {
                                        str = "QUIT\r\n";
                                        WriteStringToStream(str, shStream);
                                        state = State.WaitHostQuitResp;
                                    }
                                    else
                                    {
                                        state = State.CloseChannel;
                                    }
                                }
                                else
                                {
                                    // don't understand
                                    BadMessages++;
                                    str = "500 Command not implemented\r\n";
                                    WriteStringToStream(str, cStream);
                                }
                            }
                            else
                            {
                                if (EndOfCallTimer.ElapsedMilliseconds > StaticConfiguration.MaxClientIdleTimeoutms)
                                {
                                    // we've waited more than 30 seconds for the client to send the MAIL FROM: command.  Close.
                                    EndOfCallTimer.Reset();
                                    state = State.MailFromTimeout;
                                }
                            }
                            break;
                        case State.ConnectToSmartHost:
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
                                            hState = HostState.HostConnectionFailure;
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
                                    //something has gone wrong. inform the client.
                                    str = "451 Requested action aborted: Unable to connect to remote Smart Host.\r\n";
                                    WriteStringToStream(str, cStream);
                                    state = State.WaitClientMAILFROM;
                                    EndOfCallTimer.Restart();
                                    hState = HostState.NotConnected;
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
                                    break;
                                case HostState.HostServiceUnavailable:
                                    // smart host unavailable. terminate channel
                                    str = string.Format("421 {0} Smart Host service unavailable. Closing transmission channel.\r\n", StaticConfiguration.ThisHostName);
                                    WriteStringToStream(str, cStream);
                                    state = State.CloseChannel;
                                    hState = HostState.NotConnected;
                                    break;
                                case HostState.HostConnected:
                                    state = State.SendMAILFROM;
                                    break;
                            }
                            break;
                        case State.SendMAILFROM:
                            // we are connected to the smart host. Go ahead and send that MAIL FROM now.
                            str = string.Format("MAIL FROM: {0}\r\n", MessageMailFrom);
                            WriteStringToStream(str, shStream);
                            state = State.WaitHostMailFromResp;
                            break;
                        case State.WaitHostMailFromResp:
                            // waiting for smart host to reply to our previously send MAIL FROM request.
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("250 "))
                                {
                                    // mail from accepted
                                    str = string.Format("250 {0} Sender ok.\r\n", MessageMailFrom);
                                    WriteStringToStream(str, cStream);
                                    state = State.WaitClientRCPTTO;
                                }
                                else if (str.StartsWith("421 "))
                                {
                                    str = "421 Service not available. Closing transmission channel.\r\n";
                                    WriteStringToStream(str, cStream);
                                    state = State.CloseChannel;
                                }
                                else
                                {
                                    // some unexpected message. Relay to client so they can figure out what to do with it.
                                    WriteStringToStream(string.Format("{0}\r\n", str), cStream);
                                }
                            }
                            break;
                        case State.WaitClientRCPTTO:
                            // we need at least one RCPT TO command before we can accept any DATA commands.
                            str = BufferReadLine(ref cRead, ref cReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("RCPT TO:"))
                                {
                                    MessageLastRecipient = str.Substring(8).Trim();
                                    WriteStringToStream(string.Format("{0}\r\n", str), shStream);
                                    state = State.WaitHostRCPTResp;
                                }
                                else
                                {
                                    str = string.Format("500 Syntax error. Command unrecognized.\r\n");
                                    WriteStringToStream(str, cStream);
                                    state = State.WaitClientRCPTTO;
                                }
                            }
                            break;
                        case State.WaitHostRCPTResp:
                            // we are waiting for a response to the last RCPT TO command we relayed
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("250 ") || str.StartsWith("251 "))
                                {
                                    // host accepted that address
                                    str = string.Format("250 Recipient accepted.\r\n");
                                    WriteStringToStream(str, cStream);
                                    state = State.WaitClientRCPTorDATA;
                                    MessageRecipients.Add(MessageLastRecipient);
                                }
                                else if (str.StartsWith("421"))
                                {
                                    str = "421 Service not available. Closing transmission channel.\r\n";
                                    WriteStringToStream(str, cStream);
                                    state = State.CloseChannel;
                                }
                                else
                                {
                                    // relay unknown response to client
                                    WriteStringToStream(string.Format("{0}\r\n", str), cStream);
                                    state = State.WaitClientRCPTorDATA;
                                    // not sure if success or failure, so accept either DATA or RCPT command at this point.
                                }
                            }
                            break;
                        case State.WaitClientRCPTorDATA:
                            // we are waiting for another RCPT or DATA
                            str = BufferReadLine(ref cRead, ref cReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("RCPT TO:"))
                                {
                                    MessageLastRecipient = str.Substring(8).Trim();
                                    WriteStringToStream(string.Format("{0}\r\n", str), shStream);
                                    state = State.WaitHostRCPTResp;
                                }
                                else if (str.StartsWith("DATA"))
                                {
                                    // envelope is filled out (from and to). now we relay data.
                                    str = "DATA\r\n";
                                    WriteStringToStream(str, shStream);
                                    state = State.WaitHostDATAResp;
                                }
                                else
                                {
                                    str = string.Format("500 Syntax error. Command unrecognized.\r\n");
                                    WriteStringToStream(str, cStream);
                                    state = State.WaitClientRCPTTO;
                                }
                            }
                            break;
                        case State.WaitHostDATAResp:
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("354"))
                                {
                                    str = "354 Okay. End message with \".\" on a line by itself.\r\n";
                                    WriteStringToStream(str, cStream);
                                    state = State.TransferDATA;
                                }
                                else if (str.StartsWith("421"))
                                {
                                    str = "421 Service not available. Closing transmission channel.\r\n";
                                    WriteStringToStream(str, cStream);
                                    state = State.CloseChannel;
                                }
                                else
                                {
                                    // relay unknown message to client
                                    WriteStringToStream(string.Format("{0}\r\n", str), cStream);
                                    state = State.CloseChannel;
                                }
                            }
                            break;
                        case State.TransferDATA:
                            // we relay data between host and client until client sends the message end marker \r\n.\r\n
                            // server first, in case it needs to interrupt
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                WriteStringToStream(string.Format("{0}\r\n", str), cStream);
                                if (str.StartsWith("421"))
                                {
                                    str = "421 Service not available. Closing transmission channel.\r\n";
                                    WriteStringToStream(str, cStream);
                                    state = State.CloseChannel;
                                }
                            }
                            if (state == State.TransferDATA)
                            {
                                str = BufferReadLine(ref cRead, ref cReadPos);
                                if (str != null)
                                {
                                    WriteStringToStream(string.Format("{0}\r\n", str), shStream);
                                    if (str == ".")
                                    {
                                        state = State.WaitHostDataEndResp;
                                    }
                                }
                            }
                            break;
                        case State.WaitHostDataEndResp:
                            // waiting for the host to respond to the end of the DATA block
                            str = BufferReadLine(ref shRead, ref shReadPos);
                            if (str != null)
                            {
                                if (str.StartsWith("250"))
                                {
                                    // relay 250 message with receipt token to the client
                                    WriteStringToStream(string.Format("{0}\r\n", str), cStream);
                                    state = State.WaitClientMAILFROM;
                                    EndOfCallTimer.Restart();
                                    worker.ReportProgress(0, new StatusMessage(string.Format("Relayed message from {0} to {1}", MessageMailFrom, FormatRecipients(MessageRecipients)), MessagePriority.Information));
                                }
                                else
                                {
                                    // relay unknown message to client, and hang up
                                    WriteStringToStream(string.Format("{0}\r\n", str), cStream);
                                    state = State.CloseChannel;
                                }
                            }
                            break;
                        case State.WaitHostQuitResp:
                            // waiting for host to respond to our quit response. we actually don't care what they respond with. the QUIT was a courtesy to them so they can free resources more quickly.
                            state = State.CloseChannel;
                            break;
                        case State.CloseChannel:
                            AllDone = true;
                            break;
                        case State.MailFromTimeout:
                            if (hState == HostState.HostConnected)
                            {
                                str = "QUIT\r\n";
                                WriteStringToStream(str, shStream);
                                hState = HostState.Quitting;
                            }
                            else if (hState == HostState.Quitting)
                            {
                                str = BufferReadLine(ref shRead, ref shReadPos);
                                if (str != null)
                                {
                                    if (str.StartsWith("221"))
                                    {
                                        hState = HostState.NotConnected;
                                        state = State.CloseChannel;
                                        try
                                        {
                                            shStream.Close();
                                        }
                                        catch { }
                                        try
                                        {
                                            
                                            shStream.Dispose();
                                        }
                                        catch { }
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
                                        shStream = null;
                                        SmartHost = null;                                        
                                    }
                                }
                            }
                            break;
                    }
                }
                if (hState == HostState.HostAuthenticationFail)
                {
                    e.Result = new StatusMessage("Smart Host Authentication Failed.", MessagePriority.Error);
                }
                else
                {
                    e.Result = null;
                }
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
                if (cStream != null)
                {
                    try
                    {
                        cStream.Dispose();
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
                if (Client != null)
                {
                    try
                    {
                        Client.Close();
                    }
                    catch { }
                    try
                    {
                        Client.Dispose();
                    }
                    catch { }
                }
            }
        }

        // writes a string as ASCII bytes to a stream.
        private void WriteStringToStream(string str, NetworkStream stream)
        {
            Debug.WriteLine(str);
            byte[] buff = Encoding.ASCII.GetBytes(str);
            WriteBufferToStream(ref buff, buff.Length, stream);
        }

        // writes a buffer of bytes to a stream, clearing the buffer when done.
        private void WriteBufferToStream(ref byte[] buff, ref int pos, NetworkStream stream)
        {
            stream.Write(buff, 0, pos);
            Array.Clear(buff, 0, pos);
            pos = 0;
            return;
        }

        // writes a buffer of bytes to a stream, leaving the buffer and position pointer alone.
        private void WriteBufferToStream(ref byte[] buff, int pos, NetworkStream stream)
        {
            stream.Write(buff, 0, pos);
            return;
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

        private string Base64(string data)
        {
            var plainTextBytes = Encoding.ASCII.GetBytes(data);
            return Convert.ToBase64String(plainTextBytes);
        }

        private BackgroundWorker worker;

        public bool Done { get; private set; }

        private string FormatRecipients(List<string> rcpts)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string rcp in rcpts)
            {
                sb.AppendFormat("{0}; ", rcp);
            }
            return sb.ToString();
        }

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
