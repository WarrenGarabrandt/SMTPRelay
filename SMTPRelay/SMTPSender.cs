using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;


namespace SMTPRelay
{
    public class SMTPSender
    {
        public SMTPSender()
        {
            if (string.IsNullOrWhiteSpace(StaticConfiguration.DatabasePath))
            {
                throw new Exception("No database specified. Add a 'Database=' line to the config file.");
            }
            if (!System.IO.File.Exists(StaticConfiguration.DatabasePath))
            {
                FormatNewDatabase();
                //throw new Exception(string.Format("The specified Database file does not exist: {0}", StaticConfiguration.DatabasePath));
            }
            InitDatabase();
            // start background worker that watches for new outbound mail objects and picks them up.
        }

        private string DatabaseConnectionString
        {
            get
            {
                return string.Format(string.Format("Data Source={0}", StaticConfiguration.DatabasePath));
            }
        }

        private bool FormatNewDatabase()
        {
            SQLiteConnection.CreateFile(StaticConfiguration.DatabasePath);
            var parms = new List<KeyValuePair<string, string>>();
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                foreach (string cmdstr in SQLiteStrings.Format_Database)
                {
                    RunNonQuery(s, cmdstr, parms);
                }
                parms.Add(new KeyValuePair<string, string>("$Category", "System"));
                parms.Add(new KeyValuePair<string, string>("$Setting", "Version"));
                parms.Add(new KeyValuePair<string, string>("$Value", "1.0"));
                RunNonQuery(s, SQLiteStrings.System_Set_Version, parms);
            }
            return true;
        }

        private bool InitDatabase()
        {
            using (var s = new SQLiteConnection(DatabaseConnectionString))
            {
                s.Open();
                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>();
                parms.Add(new KeyValuePair<string, string>("$Category", "System"));
                parms.Add(new KeyValuePair<string, string>("$Setting", "Version"));
                string version = RunValueQuery(s, SQLiteStrings.System_Get_Version, parms);
                if (string.IsNullOrEmpty(version))
                {
                    throw new Exception("Incompatible database version.");
                }
                // Chance to detect old version and perform an in place database upgrade.
                switch (version)
                {
                    case "1.0":
                        return true;
                    default:
                        throw new Exception("Incompatible database version.");
                }
            }
        }

        private string RunValueQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, string>> parms)
        {
            string result = null;
            var command = conn.CreateCommand();
            command.CommandText = query;
            foreach (var kv in parms)
            {
                command.Parameters.AddWithValue(kv.Key, kv.Value);
            }
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result = reader.GetString(0);
                }
            }
            return result;
        }

        private void RunNonQuery(SQLiteConnection conn, string query, List<KeyValuePair<string, string>> parms)
        {
            var command = conn.CreateCommand();
            command.CommandText = query;
            foreach (var kv in parms)
            {
                command.Parameters.AddWithValue(kv.Key, kv.Value);
            }
            command.ExecuteNonQuery();
        }

    }
}
