using KosmaPanel.Managers.ConfigManager;
using KosmaPanel.Managers.FileManager;
using KosmaPanel.Managers.LoggerManager;
using MySqlConnector;

namespace KosmaPanel.Helpers.MigrationHelper
{
    public class MigrationHelper
    {
        public static string? connectionString;
        private const string MigrationConfigFilePath = "migrates.ini";
        FileManager fm = new FileManager();
        public void Now()
        {
            if (fm.MFolderExists())
            {
                ExecuteScripts();
            }
            else
            {
                Program.logger.Log(LogType.Error, "It looks like you are missing some important core files; please redownload KosmaPanel!!");
            }
        }
        private void getConnection()
        {
            try
            {
                var dbHost = ConfigManager.GetSetting("Daemon", "mysql_host");
                var dbPort = ConfigManager.GetSetting("Daemon", "mysql_port");
                var dbUsername = ConfigManager.GetSetting("Daemon", "mysql_username");
                var dbPassword = ConfigManager.GetSetting("Daemon", "mysql_password");
                var dbName = ConfigManager.GetSetting("Daemon", "mysql_db_name");
                connectionString = $"Server={dbHost};Port={dbPort};User ID={dbUsername};Password={dbPassword};Database={dbName}";
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, $"Failed to get the connection info from the settings file: \n" + ex.Message);
            }
        }

        private void ExecuteScript(MySqlConnection connection, string scriptContent)
        {
            using (var command = new MySqlCommand(scriptContent, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void ExecuteScripts()
        {
            try
            {
                getConnection();

                string[] scriptFiles = Directory.GetFiles("migrate/", "*.sql")
                    .OrderBy(scriptFile => Convert.ToInt32(Path.GetFileNameWithoutExtension(scriptFile)))
                    .ToArray();

                HashSet<string> migratedScripts = ReadMigratedScripts();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (string scriptFile in scriptFiles)
                    {
                        string scriptContent = File.ReadAllText(scriptFile);
                        string scriptFileName = Path.GetFileName(scriptFile);

                        if (migratedScripts.Contains(scriptFileName))
                        {
                            Program.logger.Log(LogType.Info, $"Script {scriptFileName} was already migrated. Skipping.");
                            continue;
                        }

                        Program.logger.Log(LogType.Info, "Executing script: " + scriptFileName);
                        ExecuteScript(connection, scriptContent);

                        migratedScripts.Add(scriptFileName);
                        WriteMigratedScripts(migratedScripts);
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, "Migration error: " + ex.Message);
            }
        }

        private HashSet<string> ReadMigratedScripts()
        {
            HashSet<string> migratedScripts = new HashSet<string>();

            if (File.Exists(MigrationConfigFilePath))
            {
                using (StreamReader reader = new StreamReader(MigrationConfigFilePath))
                {
                    string line;
#pragma warning disable
                    while ((line = reader.ReadLine()) != null)
                    {
                        migratedScripts.Add(line.Trim());
                    }
#pragma warning restore
                }
            }

            return migratedScripts;
        }

        private void WriteMigratedScripts(HashSet<string> migratedScripts)
        {
            using (StreamWriter writer = new StreamWriter(MigrationConfigFilePath))
            {
                foreach (string scriptFileName in migratedScripts)
                {
                    writer.WriteLine(scriptFileName);
                }
            }
        }
    }
}