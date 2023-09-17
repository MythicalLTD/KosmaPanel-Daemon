using MySqlConnector;
using System;
using System.Data;

namespace KosmaPanel.Managers.DatabaseConnectionManager
{
    public class DatabaseConnectionManager : IDisposable
    {
        private MySqlConnection _connection;

        public DatabaseConnectionManager(string connectionString)
        {
            _connection = new MySqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        public void CloseConnection()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        public MySqlConnection GetConnection()
        {
            return _connection;
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }

}

