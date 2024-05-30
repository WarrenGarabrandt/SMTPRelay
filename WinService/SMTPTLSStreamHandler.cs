using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.WinService
{
    public class SMTPTLSStreamHandler : ISMTPStream
    {
        public string EncryptionNegotiated = "";
        public string Exception = string.Empty;

        private Stream _stream;
        private SslStream _sslStream;

        public bool Broken = false;

        /// <summary>
        /// If a connection error occurs due to a policy error, that will be populated here
        /// </summary>
        public SslPolicyErrors SslPolicyErrors { get; private set; }

        private StringBuilder _sb;
        private bool _crSeen = false;

        public enum Mode
        {
            Client,
            Server
        }

        public SMTPTLSStreamHandler(Stream stream, Mode mode, string host = null, X509Certificate serverCert = null)
        {
            _stream = stream;
            _sb = new StringBuilder();
            _crSeen = false;
            switch (mode)
            {
                case Mode.Client:
                    _sslStream = new SslStream(_stream, true, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                    _sslStream.WriteTimeout = 30000;
                    try
                    {
                        _sslStream.AuthenticateAsClient(host);
                        UpdateEncryptionMethod();
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            Exception = string.Format("{0}\r\nInner Exception:\r\n{1}", ex.Message, ex.InnerException.Message);
                        }
                        else
                        {
                            Exception = ex.Message;
                        }
                        Broken = true;
                        try
                        {
                            _sslStream.Dispose();
                        }
                        catch { }
                    }
                    break;
                case Mode.Server:
                    _sslStream = new SslStream(stream, true);
                    try
                    {
                        _sslStream.AuthenticateAsServer(serverCert, false, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, true);
                        UpdateEncryptionMethod();
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null)
                        {
                            Exception = string.Format("{0}\r\nInner Exception:\r\n{1}", ex.Message, ex.InnerException.Message);
                        }
                        else
                        {
                            Exception = ex.Message;
                        }
                        Broken = true;
                        try
                        {
                            _sslStream.Dispose();
                        }
                        catch { }
                    }
                    break;
            }
        }

        private void UpdateEncryptionMethod()
        {
            EncryptionNegotiated = string.Format("{0} [{1}+{2}]", _sslStream.SslProtocol, _sslStream.CipherAlgorithm, _sslStream.HashAlgorithm);
        }

        public bool ValidateServerCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            SslPolicyErrors = sslPolicyErrors;
            return false;
        }

        public string ReadLine(int waitms = 10, BackgroundWorker bwCanceller = null)
        {
            if (Broken)
            {
                return null;
            }
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            bool GetingChars = true;
            while (GetingChars && (sw.ElapsedMilliseconds < waitms || waitms == -1))
            {
                if (bwCanceller != null && bwCanceller.CancellationPending)
                {
                    throw new OperationCanceledException();
                }
                byte[] buff = new byte[1];
                int read = _sslStream.Read(buff, 0, 1);
                if (read == 1)
                {
                    GetingChars = true;
                    if (_crSeen)
                    {
                        // see if this is lf. If it is, end of line.
                        if (buff[0] == 10)
                        {
                            _crSeen = false;
                            string line = _sb.ToString();
                            _sb.Clear();
                            sw.Stop();
                            return line;
                        }
                        else
                        {
                            // must have been a rogue cr. Append it
                            _crSeen = false;
                            _sb.Append((char)13);
                        }
                    }
                    if (buff[0] == 13)
                    {
                        // this is a cr. Flag that we've seen a cr. If we don't see a lf next, we go ahead and add the cr to the line.
                        _crSeen = true;
                    }
                    else
                    {
                        _sb.Append((char)buff[0]);
                    }
                }
                else
                {
                    GetingChars = false;
                }
            }
            sw.Stop();
            return null;
        }

        public void WriteLine(string line)
        {
            if (Broken)
            {
                return;
            }
            line = string.Format("{0}\r\n", line);
            byte[] buff = ASCIIEncoding.ASCII.GetBytes(line);
            _sslStream.Write(buff, 0, buff.Length);
            _sslStream.Flush();
        }

        public void Release()
        {
            try
            {
                if (_sslStream != null)
                {
                    _sslStream.Dispose();
                    _sslStream = null;
                }
            }
            catch { }
        }
    }
}
