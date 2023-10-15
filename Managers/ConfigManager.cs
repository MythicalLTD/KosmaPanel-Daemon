using KosmaPanel.Helpers;
using KosmaPanel.Helpers.KeyChecker;
using KosmaPanel.Managers.LoggerManager;
using Salaros.Configuration;

namespace KosmaPanel.Managers.ConfigManager
{
    public class ConfigManager
    {

        public static string d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
        public static string? mysql_host;
        public static string? mysql_port;
        public static string? mysql_username;
        public static string? mysql_password;
        public static string? mysql_name;
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

}