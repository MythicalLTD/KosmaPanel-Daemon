using MySql.Data.MySqlClient;
using System;

namespace KosmaPanel.Managers
{
    public class DatabaseConnectionManager
    {
        private MySqlConnection connection;

        public DatabaseConnectionManager(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            try
            {
                connection.Open();
                Program.logger.Log(LogType.Info, "Connected to the database.");
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, $"Error: {ex.Message}");
                Environment.Exit(0x0);
            }
        }

        public void CloseConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                Program.logger.Log(LogType.Info, "Disconnected from the database.");   
            }
        }

        public MySqlConnection GetConnection()
        {
            return connection;
        }
    }
}