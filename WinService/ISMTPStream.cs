using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.WinService
{
    public interface ISMTPStream
    {
        /// <summary>
        /// Reads a line of text from the underlying stream. It might buffer some data depending on the stream type
        /// </summary>
        /// <param name="waitms"></param>
        /// <returns></returns>
        string ReadLine(int waitms = 10);

        /// <summary>
        /// Writes a line of text to the underlying stream. Buffers will be flushed to ensure delivery immediately. 
        /// </summary>
        /// <param name="line"></param>
        void WriteLine(string line);

        /// <summary>
        /// Releases the underlying stream so that another handler can take over.
        /// </summary>
        void Release();
    }
}
