using MySqlConnector;
using System.Data;

namespace KosmaPanel.Managers.DatabaseQueryManager
{
    public class DatabaseQueryManager
    {
        private MySqlConnection _connection;

        public DatabaseQueryManager(MySqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, MySqlParameter[]? parameters = null)
        {
            DataTable dataTable = new DataTable();

            using (MySqlCommand cmd = new MySqlCommand(query, _connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (MySqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    dataTable.Load(reader);
                }
            }

            return dataTable;
        }

        public async Task<int> ExecuteNonQueryAsync(string query, MySqlParameter[]? parameters = null)
        {
            using (MySqlCommand cmd = new MySqlCommand(query, _connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                return await cmd.ExecuteNonQueryAsync();
            }
        }
    }

}
