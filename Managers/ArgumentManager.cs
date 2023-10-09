using KosmaPanel.Helpers.KeyChecker;
using KosmaPanel.Helpers.MigrationHelper;
using KosmaPanel.Managers.ConfigManager;
using KosmaPanel.Managers.LoggerManager;

namespace KosmaPanel.Managers.ArgumentManager
{
    public class ArgumentManager
    {
        public static bool ProcessArguments(string[] args)
        {
            if (args.Length == 0)
            {
                return false;
            }

            string option = args[0].ToLower();

            switch (option)
            {
                case "--sethost":
                    if (args.Length == 2)
                    {
                        string key = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "host", key);
                        return true;
                    }
                    break;
                case "--setport":
                    if (args.Length == 2)
                    {
                        string port = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "port", port);
                        return true;
                    }
                    break;
                case "--setkey":
                    if (args.Length == 2)
                    {
                        string enkey = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "key", enkey);
                        return true;
                    }
                    break;
                case "--setsshport":
                    if (args.Length == 2)
                    {
                        string port = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "ssh_port", port);
                        return true;
                    }
                    break;
                case "--setsship":
                    if (args.Length == 2)
                    {
                        string host = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "ssh_ip", host);
                        return true;
                    }
                    break;
                case "--setsshusername":
                    if (args.Length == 2)
                    {
                        string username = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "ssh_username", username);
                        return true;
                    }
                    break;
                case "--setsshpassword":
                    if (args.Length == 2)
                    {
                        string password = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "ssh_password", password);
                        return true;
                    }
                    break;
                case "--setmysqlport":
                    if (args.Length == 2)
                    {
                        string port = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "mysql_port", port);
                        return true;
                    }
                    break;
                case "--setmysqldbname":
                    if (args.Length == 2)
                    {
                        string name = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "mysql_db_name", name);
                        return true;
                    }
                    break;
                case "--setmysqlhost":
                    if (args.Length == 2)
                    {
                        string host = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "mysql_host", host);
                        return true;
                    }
                    break;
                case "--setmysqlusername":
                    if (args.Length == 2)
                    {
                        string username = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "mysql_username", username);
                        return true;
                    }
                    break;
                case "--setmysqlpassword":
                    if (args.Length == 2)
                    {
                        string password = args[1];
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "mysql_password", password);
                        return true;
                    }
                    break;
                case "-migrate-database":
                    try
                    {
                        MigrationHelper mg = new MigrationHelper();
                        mg.Now();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Program.logger.Log(LogType.Error, "Failed to migrate: " + ex.Message);
                    }
                    break;
                case "-help":
                    try
                    {
                        Console.WriteLine("╔≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡⊳ KosmaPanel CLI ⊲≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡╗");
                        Console.WriteLine("‖                                                                                                           ‖");
                        Console.WriteLine("‖    -help                   ‖ Opens a help menu with the available commands.                               ‖");
                        Console.WriteLine("‖    -version                ‖ See the version / build version of the CLI.                                  ‖");
                        Console.WriteLine("‖    -migrate-database       ‖ Create and setup all tables in the database                                  ‖");
                        Console.WriteLine("‖    -resetkey               ‖ Generate a new encryption key for KosmaPanel.                                ‖");
                        Console.WriteLine("‖    --purgelogs             ‖ Delete all the cached logs for KosmaPanel.                                   ‖");
                        Console.WriteLine("‖    --sethost               ‖ Set the host ip for KosmaPanel.                                              ‖");
                        Console.WriteLine("‖    --setport               ‖ Set the host port for KosmaPanel.                                            ‖");
                        Console.WriteLine("‖    --setkey                ‖ Set the host key for KosmaPanel.                                             ‖");
                        Console.WriteLine("‖    --setsshhost            ‖ Set the ssh ip for KosmaPanel.                                               ‖");
                        Console.WriteLine("‖    --setsshport            ‖ Set the ssh port for KosmaPanel.                                             ‖");
                        Console.WriteLine("‖    --setsshusername        ‖ Set the ssh username for KosmaPanel.                                         ‖");
                        Console.WriteLine("‖    --setsshpassword        ‖ Set the ssh password for KosmaPanel.                                         ‖");
                        Console.WriteLine("‖    --setmysqlhost          ‖ Set the MySQL host for the connection.                                       ‖");
                        Console.WriteLine("‖    --setmysqlport          ‖ Set the MySQL port for the connection.                                       ‖");
                        Console.WriteLine("‖    --setmysqlusername      ‖ Set the MySQL username for the connection.                                   ‖");
                        Console.WriteLine("‖    --setmysqlpassword      ‖ Set the MySQL password for the connection.                                   ‖");
                        Console.WriteLine("‖    --setmysqldbname        ‖ Set the MySQL database name for the connection.                              ‖");
                        Console.WriteLine("‖                                                                                                           ‖");
                        Console.WriteLine("╚≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡⊳ Copyright 2023 MythicalSystems ⊲≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡≡╝");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Program.logger.Log(LogType.Error, "Failed to display help message: " + ex.Message);
                    }
                    break;
                case "-purgelogs":
                    try
                    {
                        Program.logger.PurgeLogs();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Program.logger.Log(LogType.Error, "Failed to purge the logs: " + ex.Message);

                    }
                    break;
                case "-resetkey":
                    ConfigManager.ConfigManager.d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
                    try
                    {
                        string skey = KeyChecker.GenerateStrongKey();
                        ConfigManager.ConfigManager.UpdateSetting("Daemon", "key", skey);
                        Program.logger.Log(LogType.Info, "We updated your daemon settings");
                        Program.logger.Log(LogType.Info, $"Your key is: {skey}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Program.logger.Log(LogType.Error, $"Failed to generate a key: {ex.Message}");

                    }
                    break;
                case "-version":
                    Program.logger.Log(LogType.Info, $"You are running version: {Program.version}");
                    return true;
            }

            return false; // Argument processing failed
        }
    }

}