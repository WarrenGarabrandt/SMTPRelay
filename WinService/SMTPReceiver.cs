using SMTPRelay.Model;
using SMTPRelay.Model.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.WinService
{
    public class SMTPReceiver
    {
        public const int CONNECTION_TIMEOUTMS = 120000;

        private BackgroundWorker Worker;        
        public bool Running;        
        public ConcurrentQueue<WorkerReport> WorkerReports;

        public SMTPReceiver(TcpClient client)
        {
            WorkerReports = new ConcurrentQueue<WorkerReport>();
            Running = true;
            Worker = new BackgroundWorker();
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += Worker_DoWork;
            Worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            Worker.RunWorkerAsync(client);
        }

        public void Cancel()
        {
            Worker.CancelAsync();
        }

        private enum SMTPStates
        {
            SendHello,                            // Send the 220 welcome message, proceed to RcvClientHello,
            RcvClientHello,                       // Wait for client to send the EHLO/HELO command. If get EHLO proceed to SendServerExtensions. Else, proceed to SendSuccessAck
            SendServerExtensions,                 // Client has send EHLO/HELO, transmit our extnesions. proceed to SendSuccessAck
            SendSuccessAck,                       // Send 250 Ok. Procedd to WaitForClientVerb.
            WaitForClientVerb,                    // Wait for the client to send something. Depending on command... 
                                                  // RSET, clear mail object, proceed to SendSuccessAck. This is probably an antivirus interception.
                                                  // NOOP, proceed to SendSuccessAck
                                                  // STARTTLS, proceed to SendStartTLSNotAvail
                                                  // AUTH, proceed to SendAUTHLOGINUsernameChallenge
                                                  // MAIL FROM, proceed to ProcessMAILFROM.
                                                  // RCPT TO, proceed to ProcessRCPTTOMessage
                                                  // QUIT, proceed to ProcessQUITMessage.
                                                  // DATA, proceed to ProcessDATAMessage.
                                                  // For all other messages, proceed to GenericNotImplemented.
            SendStartTLSNotAvail,                 // Send 454 TLS not available. proceed to WaitForClientVerb.
            SendStartTLSReq,                      // Command received that requires StartTLS first. Send 530 StartTLS Required. Proceed to WaitForClientVerb
            SendAuthReq,                          // Authentication required. Send 550 Authentication Required. Proceed to WaitForClientVerb
            SendBadCommandSeq,                    // Got a RCPT TO with no MAIL object yet. Send 503 Bad sequence of commands. Proceed to WaitForClientVerb.
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
            SendRCPTTOTooMany,                    // Send 452 Too Many recipients. Proceed to WaitForClientVerb
            SendRCPTTOOk,                         // Got a RCPT TO that is valid. Send 250 Ok, proceed to WaitForClientVerb.
            ProcessDATAMessage,                   // We got a DATA command. Should be authenticated, in MAIL mode, with at least one recipient. Create necessary database objects.
                                                  // If we are not in MAIL mode, or there are no recipients, proceed to SendBadCommandSeq
                                                  // If we are ready to begin receiving data, proceed to SendDATAOk.
            SendDATAOk,                           // We have valid records in the database and are ready to receive. Send 354 End data with <CR><LF>.<CR><LF>. Proceed to ReceiveDATABlock
            ReceiveDATABlock,                     // We are receiving the message. When we get a \r\n.\r\n, proceed to FinalizeDATABlock.
                                                  // If the message data exceeds our mesage limit, 
            FailDATABlock,                        // Send 552 Requested mail actions aborted – Exceeded storage allocation.
                                                  // Delete all message data, mark Envelope as failed. Proceed to BadCommandDisconnect.
            FinalizeDATABlock,                    // We have finished receiving the message. Finish processing, and add item to the outbound queue. Proceed to SendDATAAck.
            SendDATAAck,                          // Send the 250 Ok and tracking ID to client. Clear MAIL object so another can be started. Proceed to WaitForClientVerb.
            ProcessQUITMessage,                   // Client sent QUIT. Send 221 Bye and close the connection.
            BadCommandDisconnect,                 // Send 421 Too Many errors. Close the connection.
            GenericNotImplemented,                // Generic error that command isn't implemented. 550 Command not Implemented.
                                                  // If not exceeds error limit, Go to WaitForClientVerb
                                                  // if exceeds error limit, go to BadCommandDisconnect.
            ExitProcessing                        // Exit the message loop, close streams, close socket, exit the worker.
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // keeps track of our converation timeout. Every time we 
            System.Diagnostics.Stopwatch swTimeout = new System.Diagnostics.Stopwatch();
            swTimeout.Start();
            bool eSMTP= false;   // turns on ESMTP extensions for this connection.
            tblUser connectedUser = null;   // when they Authenticate, this is the user they have logged in with. They are unauthenticated if this is NULL.
            int AUTHLOGINTries = 4;     // they get a limited number of AUTH LOGIN tries before we disconnect them.
            int BadCommandTries = 10;   // They get a limited number of bad commands tries before we disconnect them.

            try
            {
                TcpClient client = e.Argument as TcpClient;
                if (client == null)
                {
                    throw new Exception("Invalid TcpClient passed to SMTPReceiver.");
                }
                NetworkStream stream = client.GetStream();

                bool CloseConnection = false;
                while (!Worker.CancellationPending && !CloseConnection)
                {
                    if (swTimeout.ElapsedMilliseconds > CONNECTION_TIMEOUTMS)
                    {
                        CloseConnection = true;
                    }

                }

            }
            catch (Exception ex)
            {
                e.Result = new WorkerReport()
                {
                    LogError = ex.Message
                };
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
