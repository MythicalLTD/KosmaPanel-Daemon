using KosmaPanel;
using MySqlConnector;

namespace KosmaPanel.Helpers.SqlScriptExecutor
{
    public static class SqlScriptExecutor
    {
        public static void Execute(string query)
        {
            using (MySqlConnection connection = new MySqlConnection(Program.connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}