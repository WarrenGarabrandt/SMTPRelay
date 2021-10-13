using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.WinService
{
    public class SMTPTLSStreamHandler : ISMTPStream
    {
        public string EncryptionNegotiated = "";

        private Stream _stream;
        private SslStream _sslStream;

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
                    try
                    {
                        _sslStream.AuthenticateAsClient(host);
                        UpdateEncryptionMethod();
                    }
                    catch (AuthenticationException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        try
                        {
                            _sslStream.Dispose();
                        }
                        catch { }
                        throw;
                    }
                    break;
                case Mode.Server:
                    _sslStream = new SslStream(stream, true);
                    try
                    {
                        _sslStream.AuthenticateAsServer(serverCert, false, SslProtocols.None, true);
                        UpdateEncryptionMethod();
                    }
                    catch (AuthenticationException ex)
                    {
                        System.Diagnostics.Debug.Write(ex.Message);
                        try
                        {

                            _sslStream.Dispose();
                        }
                        catch { }
                        throw;
                    }
                    break;
            }
        }

        private void UpdateEncryptionMethod()
        {
            string keyexch;
            switch (_sslStream.KeyExchangeAlgorithm)
            {
                case ExchangeAlgorithmType.None:
                    keyexch = "none";
                    break;
                case ExchangeAlgorithmType.RsaSign:
                    keyexch = "RSASign";
                    break;
                case ExchangeAlgorithmType.RsaKeyX:
                    keyexch = "RSAKeyX";
                    break;
                case ExchangeAlgorithmType.DiffieHellman:
                    keyexch = "ECDH";
                    break;
                default:
                    keyexch = _sslStream.KeyExchangeAlgorithm.ToString();
                    break;
            }
            EncryptionNegotiated = string.Format("{0} [{1}+{2}+{3}]", _sslStream.SslProtocol, _sslStream.CipherAlgorithm, _sslStream.HashAlgorithm, keyexch);
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

        public string ReadLine(int waitms = 10)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            bool GetingChars = true;
            while (GetingChars || sw.ElapsedMilliseconds < waitms || waitms == -1)
            {
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
            return null;
        }

        public void WriteLine(string line)
        {
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
