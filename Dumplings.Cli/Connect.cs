using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;


namespace Dumplings.Cli
{
    internal class Connect
    {
        public const string server = "178.238.222.15";
        public const string user = "creati14_wasabi";
        public const string psw = "rMgv[egZw};1";
        public const string db = "creati14_wasabi";

        public static MySqlConnection InitDb()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Server = server;
            builder.UserID = user;
            builder.Password = psw;
            builder.Database = db;
            string conn = builder.ToString();
            try
            {
                return new MySqlConnection(conn);
            }
            catch (MySqlException err)
            {
                Console.WriteLine("Nem sikerült a kapcsolat" + err.Message);
                return default(MySqlConnection);
            }
        
        }
    }
}
