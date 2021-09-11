using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Model.DB
{
    public class tblSystem
    {
        //CREATE TABLE System (Category TEXT, Setting TEXT, Value TEXT);

        public tblSystem(string category, string setting, string value)
        {
            Category = category;
            Setting = setting;
            Value = value;
        }
        public string Category { get; set; }

        public string Setting { get; set; }

        public string Value { get; set; }
    }
}
