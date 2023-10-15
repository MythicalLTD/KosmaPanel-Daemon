using KosmaPanel.Helpers;
using KosmaPanel.Helpers.BashHelper;
using KosmaPanel.Managers;
using KosmaPanel.Managers.LoggerManager;

namespace KosmaPanel.Services.LinuxMetricsService
{

    public class LinuxMetricsService
    {
        public static string? cpu_model;
        public static string? cpu_usage;
        public static string? uptime;
        public static string? os_name;
        public static string? os_type;
        public static string? ram_used;
        public static string? ram_total;
        public static string? ram_free;
        public static string? disk_used;
        public static string? disk_total;
        public static string? disk_free;
        public static string? kernel_name;

        //AMD EPYC 7713 64-Core Processor
        public static async Task<string> GetCpuModel()
        {
            return await BashHelper.ExecuteCommand("lscpu | grep 'Model name' | awk -F: '{print $2}' | sed 's/^ *//'");
        }
        //1 week, 1 day, 16 hours, 55 minutes
        public static async Task<string> GetUptime()
        {
            return
                await BashHelper
                    .ExecuteCommand("uptime -p | sed 's/^up //'");
        }
        //Ubuntu 22.04.3 LTS
        public static async Task<string> GetOsName()
        {
            return await BashHelper
                .ExecuteCommand("lsb_release -s -d");
        }
        public static async Task<string> GetCpuUsage()
        {
            return await BashHelper
                .ExecuteCommand("top -b -n 1 | grep '%Cpu(s)' | awk '{print $2}' | cut -d. -f1");
        }
        public static async Task<string> GetRamUsed()
        {
            var ram = await BashHelper.ExecuteCommand("grep 'MemFree:' /proc/meminfo | awk '{print $2}'");
            int ram_used_mb = (int)(int.Parse(ram) / 1024);
            return ram_used_mb.ToString();
        }
        public static async Task<string> GetRamTotal()
        {
            var ram = await BashHelper.ExecuteCommand("grep 'MemTotal:' /proc/meminfo | awk '{print $2}'");
            int ram_total_mb = (int)(int.Parse(ram) / 1024);
            return ram_total_mb.ToString();
        }
        public static async Task<string> GetRamFree()
        {
            string ramUsed = await GetRamUsed();
            string ramTotal = await GetRamTotal();

            int ramUsedMB = int.Parse(ramUsed);
            int ramTotalMB = int.Parse(ramTotal);

            int ramFreeMB = ramTotalMB - ramUsedMB;

            return ramFreeMB.ToString();
        }
        public static async Task<string> GetDiskUsed()
        {
            string diskInfo = await BashHelper.ExecuteCommand("df -B 1 --total | tail -1 | awk '{print $2,$3}'");
            string[] diskInfoParts = diskInfo.Split(' ');
            string diskUsedStr = diskInfoParts[1];
            long diskUsed = long.Parse(diskUsedStr);
            long diskUsedMB = diskUsed / 1024 / 1024;
            return diskUsedMB.ToString();
        }
        public static async Task<string> GetDiskTotal()
        {
            string diskInfo = await BashHelper.ExecuteCommand("df -B 1 --total | tail -1 | awk '{print $2,$3}'");

            string[] diskInfoParts = diskInfo.Split(' ');
            string diskTotalStr = diskInfoParts[0];
            long diskTotal = long.Parse(diskTotalStr);
            long diskTotalMB = diskTotal / 1024 / 1024;
            return diskTotalMB.ToString();
        }

        public static async Task<string> GetDiskFree()
        {
            string diskInfo = await BashHelper.ExecuteCommand("df -B 1 --total | tail -1 | awk '{print $2,$3}'");

            string[] diskInfoParts = diskInfo.Split(' ');
            string diskTotalStr = diskInfoParts[0];
            string diskUsedStr = diskInfoParts[1];

            long diskTotal = long.Parse(diskTotalStr);
            long diskUsed = long.Parse(diskUsedStr);

            long diskTotalMB = diskTotal / 1024 / 1024;
            long diskUsedMB = diskUsed / 1024 / 1024;
            long diskFree = diskTotal - diskUsed;
            long diskFreeMB = diskFree / 1024 / 1024;

            return diskFreeMB.ToString();
        }
        public static async Task<string> getKernelName()
        {
            return await BashHelper
                .ExecuteCommand("uname -r");
        }
        public static async void getOsInfo()
        {
            try
            {
                if (OperatingSystem.IsLinux())
                {
                    os_type = "Linux";
                }
                string osCpu = await GetCpuModel();
                cpu_model = osCpu.Replace("\n", "");
                string osUptime = await GetUptime();
                uptime = osUptime.Replace("\n", "");
                string osName = await GetOsName();
                os_name = osName.Replace("\n", "");
                string ramUsed = await GetRamUsed();
                ram_used = ramUsed.Replace("\n", "");
                string ramTotal = await GetRamTotal();
                ram_total = ramTotal.Replace("\n", "");
                string ramFree = await GetRamFree();
                ram_free = ramFree.Replace("\n", "");
                string diskUsed = await GetDiskUsed();
                disk_used = diskUsed.Replace("\n", "");
                string diskTotal = await GetDiskTotal();
                disk_total = diskTotal.Replace("\n", "");
                string diskFree = await GetDiskFree();
                disk_free = diskFree.Replace("\n", "");
                string kernelName = await getKernelName();
                kernel_name = kernelName.Replace("\n", "");
                string cpuUsage = await GetCpuUsage();
                cpu_usage = cpuUsage.Replace("\n", "");
                //Program.logger.Log(LogType.Info, "Operating System: " + os_name);
                //Program.logger.Log(LogType.Info, "Kernel: " + kernel_name);
                //Program.logger.Log(LogType.Info, "Uptime: " + uptime);
                //Program.logger.Log(LogType.Info, $"Disk: {disk_used} (Used) GB / {disk_free} GB (Free) / {disk_total} GB (Total)");
                //Program.logger.Log(LogType.Info, $"Ram: {ram_used} MB (Used) / {ram_free} MB (Free) / {ram_total} MB (Total)");
                //Program.logger.Log(LogType.Info, "CPU Model: " + cpu_model);
                //Program.logger.Log(LogType.Info, "CPU Usage: " + cpu_usage + "%/100%");
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, "Faild to get the os info: '" + ex.Message + "'");
            }
        }
    }
}