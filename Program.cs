using KosmaPanel.Managers;
using KosmaPanel.Helpers;
using KosmaPanel.Services;

namespace KosmaPanel
{
    public class Program
    {

        public static string mcascii = @" 
  _  __                         _____                 _ 
 | |/ /                        |  __ \               | |
 | ' / ___  ___ _ __ ___   __ _| |__) |_ _ _ __   ___| |
 |  < / _ \/ __| '_ ` _ \ / _` |  ___/ _` | '_ \ / _ \ |
 | . \ (_) \__ \ | | | | | (_| | |  | (_| | | | |  __/ |
 |_|\_\___/|___/_| |_| |_|\__,_|_|   \__,_|_| |_|\___|_|
                                                        
    ";
        public static string version = "1.0.0";
        public static LoggerManager logger = new LoggerManager();

        public static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine(mcascii);
            if (!OperatingSystem.IsLinux())
            {
                logger.Log(LogType.Error, "Sorry you have to be on debain / linux to use our daemon");
                Environment.Exit(0x0);
            }
            if (args.Contains("-version"))
            {
                logger.Log(LogType.Info, $"You are running version: {version}");
                Environment.Exit(0x0);
            }
            else if (args.Length == 2 && args[0] == "--setHost")
            {
                string key = args[1];
                ConfigManager.UpdateSetting("Daemon", "host", key);
                Environment.Exit(0x0);
            }
            else if (args.Length == 2 && args[0] == "--setPort")
            {
                string port = args[1];
                ConfigManager.UpdateSetting("Daemon", "port", port);
                Environment.Exit(0x0);
            }
            else if (args.Length == 2 && args[0] == "--setKey")
            {
                string enkey = args[1];
                ConfigManager.UpdateSetting("Daemon", "key", enkey);
                Environment.Exit(0x0);
            }
            else if (args.Length == 2 && args[0] == "--setSshPort")
            {
                string port = args[1];
                ConfigManager.UpdateSetting("Daemon", "ssh_port", port);
                Environment.Exit(0x0);
            }
            else if (args.Length == 2 && args[0] == "--setSshHost")
            {
                string host = args[1];
                ConfigManager.UpdateSetting("Daemon", "ssh_ip", host);
                Environment.Exit(0x0);
            }
            else if (args.Length == 2 && args[0] == "--setSshUsername")
            {
                string username = args[1];
                ConfigManager.UpdateSetting("Daemon", "ssh_username", username);
                Environment.Exit(0x0);
            }
            else if (args.Length == 2 && args[0] == "--setSshPassword")
            {
                string password = args[1];
                ConfigManager.UpdateSetting("Daemon", "ssh_password", password);
                Environment.Exit(0x0);
            }
            else if (args.Contains("-resetkey"))
            {
                ConfigManager.d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
                try
                {
                    string skey = KeyChecker.GenerateStrongKey();
                    ConfigManager.UpdateSetting("Daemon", "key", skey);
                    logger.Log(LogType.Info, "We updated your daemon settings");
                    logger.Log(LogType.Info, $"Your key is: {skey}");
                    Environment.Exit(0x0);
                }
                catch (Exception ex)
                {
                    logger.Log(LogType.Error, $"Failed to generate a key: {ex.Message}");
                    Environment.Exit(0x0);
                }
            }
            else if (args.Length > 0)
            {
                logger.Log(LogType.Warning, "This is an invalid startup argument. Please use '-help' to get more information.");
                Environment.Exit(0x0);
            }
            logger.Log(LogType.Info, "Please wait while we start KosmaPanel");
            LinuxMetricsService.getOsInfo();
            DDosDetectionService dds = new DDosDetectionService();
            dds.Start();
            try
            {
                ConfigManager.CheckSettings();
            }
            catch (Exception ex)
            {
                logger.Log(LogType.Error, "Sorry but i start the config manager: " + ex.Message);
                Environment.Exit(0x0);
            }

            try
            {
                logger.Log(LogType.Info, "Daemon started on: " + ConfigManager.GetSetting("Daemon", "host") + ":" + ConfigManager.GetSetting("Daemon", "port"));
                logger.Log(LogType.Info, "Secret key: " + ConfigManager.GetSetting("Daemon", "key"));
                WebServerService wbs = new WebServerService();
                wbs.Start(ConfigManager.GetSetting("Daemon", "port"), ConfigManager.GetSetting("Daemon", "host"));
            }
            catch (Exception ex)
            {
                logger.Log(LogType.Error, "Sorry but i cant start the daemon webserver: " + ex.Message);
                Environment.Exit(0x0);
            }
        }
    }
}