using System;
using System.Collections.Generic;
using System.Text;
using Dumplings.Helpers;
using MySql.Data.MySqlClient;

namespace Dumplings.Cli
{
    internal class Connect
    {
        public static MySqlConnection InitDb(string connectionString)
        {
            try
            {
                return new MySqlConnection(connectionString);
            }
            catch (MySqlException exc)
            {
                Logger.LogError(exc, $"Couldn't connect to database with provided connection string.\n The string was: '{connectionString}'.");
                return null;
            }
        }
    }
}
