using KosmaPanel.Managers.ArgumentManager;
using KosmaPanel.Managers.ConfigManager;
using KosmaPanel.Managers.LoggerManager;
using KosmaPanel.Services.DDosDetectionService;
using KosmaPanel.Services.LinuxMetricsService;
using KosmaPanel.Services.WebServerService;

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
        public static string? connectionString;
        public static void Main(string[] args)
        {
            Console.Clear();
            logger.Log(LogType.Info, mcascii);
            if (!OperatingSystem.IsLinux())
            {
                logger.Log(LogType.Error, "Sorry you have to be on debain / linux to use our daemon");
                Environment.Exit(0x0);
            }
            if (ArgumentManager.ProcessArguments(args))
            {
                Environment.Exit(0x0);
            }
            try
            {
                logger.Log(LogType.Info, "Please wait while we start KosmaPanel");
                LinuxMetricsService.getOsInfo();
                DDosDetectionService dds = new DDosDetectionService();
                dds.Start();
                ConfigManager.CheckSettings();
                connectionString = $"Server={ConfigManager.mysql_host};Port={ConfigManager.mysql_port};User ID={ConfigManager.mysql_username};Password={ConfigManager.mysql_password};Database={ConfigManager.mysql_name}";
                logger.Log(LogType.Info, "Daemon started on: " + ConfigManager.GetSetting("Daemon", "host") + ":" + ConfigManager.GetSetting("Daemon", "port"));
                logger.Log(LogType.Info, "Secret key: " + ConfigManager.GetSetting("Daemon", "key"));
                WebServerService wbs = new WebServerService();
                wbs.Start(ConfigManager.GetSetting("Daemon", "port"), ConfigManager.GetSetting("Daemon", "host"));
            }
            catch (Exception ex)
            {
                logger.Log(LogType.Error, "Sorry but i cant start the daemon: " + ex.Message);
                Environment.Exit(0x0);
            }

        }
    }
}