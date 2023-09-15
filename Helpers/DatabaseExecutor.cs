using KosmaPanel.Managers;
using KosmaPanel;
using MySql.Data.MySqlClient;
using System.Data;

public class DatabaseExecutor
{
    private MySqlConnection connection;

    public DatabaseExecutor(DatabaseConnectionManager dbConnection)
    {
        connection = dbConnection.GetConnection();
    }

    public DataTable ExecuteQuery(string query, MySqlParameter[]? parameters = null)
    {
        DataTable dataTable = new DataTable();
        try
        {
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
        }
        catch (Exception ex)
        {
            Program.logger.Log(LogType.Error, $"Error executing query: {ex.Message}");
        }
        return dataTable;
    }

    public int ExecuteNonQuery(string query, MySqlParameter[]? parameters = null)
    {
        int rowsAffected = 0;
        try
        {
            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                rowsAffected = cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Program.logger.Log(LogType.Error, $"Error executing non-query: {ex.Message}");
        }
        return rowsAffected;
    }
}
