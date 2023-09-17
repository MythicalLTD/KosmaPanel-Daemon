using KosmaPanel.Helpers;
using Salaros.Configuration;

namespace KosmaPanel.Managers;

public class ConfigManager
{

    public static string d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
    public static string? mysql_host;
    public static string? mysql_port;
    public static string? mysql_username;
    public static string? mysql_password;
    public static string? mysql_name;
    public static void CheckSettings()
    {
        try
        {
            var cfg = new ConfigParser(d_settings);
            if (!File.Exists(d_settings))
            {
                UpdateSetting("Daemon", "host", "127.0.0.1");
                UpdateSetting("Daemon", "port", "5001");
                UpdateSetting("Daemon", "key", "");
                UpdateSetting("Daemon", "ssh_ip", "");
                UpdateSetting("Daemon", "ssh_port", "");
                UpdateSetting("Daemon", "ssh_username", "");
                UpdateSetting("Daemon", "ssh_password", "");
                UpdateSetting("Daemon", "mysql_host", "127.0.0.1");
                UpdateSetting("Daemon", "mysql_port", "3306");
                UpdateSetting("Daemon", "mysql_username", "");
                UpdateSetting("Daemon", "mysql_password", "");
                UpdateSetting("Daemon", "mysql_db_name", "");
                Program.logger.Log(LogType.Warning, "Looks like this is your first time running our daemon. Please close the app, go into config.ini, and configure your app");
                Environment.Exit(0x0);
            }
            var d_host = GetSetting("Daemon", "host");
            var d_port = GetSetting("Daemon", "port");
            var d_key = GetSetting("Daemon", "key");
            var d_ssh_ip = GetSetting("Daemon", "ssh_ip");
            var d_ssh_port = GetSetting("Daemon", "ssh_port");
            var d_ssh_username = GetSetting("Daemon", "ssh_username");
            var d_ssh_password = GetSetting("Daemon", "ssh_password");
            var cfg_mysql_host = GetSetting("Daemon", "mysql_host");
            var cfg_mysql_port = GetSetting("Daemon", "mysql_port");
            var cfg_mysql_username = GetSetting("Daemon", "mysql_username");
            var cfg_mysql_password = GetSetting("Daemon", "mysql_password");
            var cfg_mysql_name = GetSetting("Daemon", "mysql_db_name");
            if (cfg_mysql_host == "") {
                cfg_mysql_host = "127.0.0.1";
            }
            if (cfg_mysql_port == "") {
                cfg_mysql_port = "3306";
            }
            if (d_host == "")
            {
                d_host = "127.0.0.1";
            }
            if (d_port == "")
            {
                d_port = "5001";
            }
            if (cfg_mysql_password == "")
            {
                Program.logger.Log(LogType.Error, "Failed to start: 'We did not find any mysql connection information inside the settings.ini.'");
                Environment.Exit(0x0);
            }
            if (cfg_mysql_name == "")
            {
                Program.logger.Log(LogType.Error, "Failed to start: 'We did not find any mysql connection information inside the settings.ini.'");
                Environment.Exit(0x0);
            }
            if (cfg_mysql_username == "")
            {
                Program.logger.Log(LogType.Error, "Failed to start: 'We did not find any mysql connection information inside the settings.ini.'");
                Environment.Exit(0x0);
            }
            mysql_host = cfg_mysql_host;
            mysql_port = cfg_mysql_port;
            mysql_username = cfg_mysql_username;
            mysql_password = cfg_mysql_password;
            mysql_name = cfg_mysql_name;
            if (d_ssh_ip == "")
            {
                Program.logger.Log(LogType.Error, "Failed to start: 'We did not find any ssh connection information inside the settings.ini.'");
                Environment.Exit(0x0);
            }
            if (d_ssh_port.ToString() == "")
            {
                Program.logger.Log(LogType.Error, "Failed to start: 'We did not find any ssh connection information inside the settings.ini.'");
                Environment.Exit(0x0);
            }
            if (d_ssh_username == "")
            {
                Program.logger.Log(LogType.Error, "Failed to start: 'We did not find any ssh connection information inside the settings.ini.'");
                Environment.Exit(0x0);
            }
            if (d_ssh_password == "")
            {
                Program.logger.Log(LogType.Error, "Failed to start: 'We did not find any ssh connection information inside the settings.ini.'");
                Environment.Exit(0x0);
            }
            if (d_key == "")
            {
                Program.logger.Log(LogType.Error, "Failed to start: 'Please use a strong key'");
                Environment.Exit(0x0);
            }
            if (!KeyChecker.isStrongKey(d_key))
            {
                Program.logger.Log(LogType.Error, "Failed to start: 'Please use a strong key'");
                Environment.Exit(0x0);
            }
            Program.logger.Log(LogType.Info, "Check passed for 'config.ini'");
        }
        catch (Exception ex)
        {
            Program.logger.Log(LogType.Error, "Failed to check config: " + ex.Message);
            Environment.Exit(0x0);
        }
    }

    public static string GetSetting(string app, string setting)
    {
        try
        {
            var cfg = new ConfigParser(d_settings);
            var st = cfg.GetValue(app, setting);
            return st;
        }
        catch (Exception ex)
        {
            Program.logger.Log(LogType.Error, "Failed to get setting: " + ex.Message);
            Environment.Exit(0x0);
            return null;
        }
    }

    public static void UpdateSetting(string app, string setting, string value)
    {
        try
        {
            var cfg = new ConfigParser(d_settings);
            cfg.SetValue(app, setting, value);
            cfg.Save();
            Program.logger.Log(LogType.Info, $"Updated: {setting}");
        }
        catch (Exception ex)
        {
            Program.logger.Log(LogType.Error, "Failed to update settings: " + ex.Message);
            Environment.Exit(0x0);
        }
    }
}
