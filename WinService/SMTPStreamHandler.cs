﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace SMTPRelay.WinService
{
    public class SMTPStreamHandler : ISMTPStream
    {
        private Stream _stream;

        /// <summary>
        /// Reads and writes text on a stream one line at a time.
        /// </summary>
        /// <param name="stream">The stream to read/write data. No extra characters are read so that streams can be swapped.</param>
        public SMTPStreamHandler(Stream stream)
        {
            _sb = new StringBuilder();
            _stream = stream;
            _stream.WriteTimeout = 30000;
        }

        private StringBuilder _sb;
        private bool _crSeen = false;

        /// <summary>
        /// Reads a line of text from the stream, or returns NULL if a full line isn't ready.
        /// Waits a specified number of milliseconds for new data before giving up.
        /// If waitms is specified as -1, then it waits indefinitely.
        /// </summary>
        /// <param name="waitms">How long to wait in milliseconds before giving up and returning nothing</param>
        /// <returns>A full line of text </returns>
        public string ReadLine(int waitms = 10, BackgroundWorker bwCanceller = null)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            bool GetingChars = true;
            // Stream.Read blocks if there is no data available, so we want to use the DataAvailable method on the NetworkStream before trying that.
            if (waitms != -1 && _stream is NetworkStream stream)
            {
                while (!stream.DataAvailable && sw.ElapsedMilliseconds < waitms)
                {
                    System.Threading.Thread.Sleep(1);
                }
                if (!stream.DataAvailable)
                {
                    return null;
                }
            }
            while (GetingChars && (sw.ElapsedMilliseconds < waitms || waitms == -1))
            {
                if (bwCanceller != null && bwCanceller.CancellationPending)
                {
                    throw new OperationCanceledException();
                }
                byte[] buff = new byte[1];
                int read = _stream.Read(buff, 0, 1);
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
            line = string.Format("{0}\r\n", line);
            byte[] buff = ASCIIEncoding.ASCII.GetBytes(line);
            _stream.Write(buff, 0, buff.Length);
            _stream.Flush();
        }

        public void Release()
        {
            return;
        }
    }
}
