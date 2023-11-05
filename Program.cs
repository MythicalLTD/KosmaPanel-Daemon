using KosmaPanel.Managers.ArgumentManager;
using KosmaPanel.Managers.ConfigManager;
using KosmaPanel.Managers.ImageManager;
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
        public static string appWorkingDir = AppDomain.CurrentDomain.BaseDirectory;
        
        public static void Main(string[] args)
        {
            try
            {
                Start(args);
            }
            catch (Exception ex)
            {
                logger.Log(LogType.Error, "Sorry but i cant start the daemon: " + ex.Message);
                Program.Stop();
            }
        }
        public static void Stop() {
            logger.Log(LogType.Info, "Please wait while we shut down the daemon.");
            Environment.Exit(0x0);
        }
        public static void Crash(string message) {
            logger.Log(LogType.Error, "We are sorry but the daemon crashed please make sure to report this to the support team: "+message);
            Environment.Exit(0x0);
        }
        private static void Start(string[] args)
        {
            Console.Clear();
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            LinuxMetricsService.getOsInfo();
            logger.Log(LogType.Info, mcascii);
            if (!OperatingSystem.IsLinux())
            {
                logger.Log(LogType.Error, "Sorry, but you have to be on Debian or Linux to use our daemon.");
                Program.Stop();
            }
            if (ArgumentManager.ProcessArguments(args))
            {
                Program.Stop();
            }
            logger.Log(LogType.Info, "Please wait while we start KosmaPanel Daemon");
            DDosDetectionService dds = new DDosDetectionService();
            dds.Start();
            connectionString = $"Server={ConfigManager.mysql_host};Port={ConfigManager.mysql_port};User ID={ConfigManager.mysql_username};Password={ConfigManager.mysql_password};Database={ConfigManager.mysql_name}";
            logger.Log(LogType.Info, "Daemon started on: " + ConfigManager.GetSetting("Daemon", "host") + ":" + ConfigManager.GetSetting("Daemon", "port"));
            logger.Log(LogType.Info, "Secret key: " + ConfigManager.GetSetting("Daemon", "key"));
            WebServerService wbs = new WebServerService();
            ImageManager.Check();
            wbs.Start(ConfigManager.GetSetting("Daemon", "port"), ConfigManager.GetSetting("Daemon", "host"));
        }
    }
}