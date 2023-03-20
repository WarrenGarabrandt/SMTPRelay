using SMTPRelay.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay.Configuration.Helpers
{
    public static class SettingsHelper
    {
        public static int GetIntValue(string category, string setting)
        {
            string val = SQLiteDB.System_GetValue(category, setting);
            int valParsed;
            if (!Int32.TryParse(val, out valParsed))
            {
                val = SQLiteDB.System_GetDefaultValue(category, setting);
                SQLiteDB.System_AddUpdateValue(category, setting, val);

            }
            if (!Int32.TryParse(val, out valParsed))
            {
                valParsed = 0;
            }
            return valParsed;
        }

        public static string GetStrValue(string category, string setting)
        {
            string val = SQLiteDB.System_GetValue(category, setting);
            if (string.IsNullOrEmpty(val))
            {
                val = SQLiteDB.System_GetDefaultValue(category, setting);
                SQLiteDB.System_AddUpdateValue(category, setting, val);
            }
            return val;
        }
    }
}
