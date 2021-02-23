using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Models
{
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
}
