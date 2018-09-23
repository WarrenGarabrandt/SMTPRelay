using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Models
{
    public class SmartHost
    {
        public EndPoint EndPoint { get; set; }
        public SmtpCredentials Credentials { get; set; }
    }
}
