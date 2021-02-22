using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPRelay
{
    public static class SQLiteStrings
    {
        public static string[] Format_Database = new string[]
        {
            @"CREATE TABLE System (Category TEXT, Setting TEXT, Value TEXT);"
        };
        
        public static string System_Get_Version = @"SELECT Value FROM System WHERE Category = $Category AND Setting = $Setting;";
        public static string System_Set_Version = @"INSERT INTO System(Category, Setting, Value) VALUES ($Category, $Setting, $Value);";
    }
}
