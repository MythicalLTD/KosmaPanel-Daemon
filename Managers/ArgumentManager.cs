using KosmaPanel.Helpers.KeyChecker;
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