using System.Diagnostics;
using System.Timers;
using KosmaPanel.Helpers.BashHelper;
using KosmaPanel.Managers.LoggerManager;

namespace KosmaPanel.Services.DDosDetectionService
{
    public class DDosDetectionService
    {
        private Process? ScriptProcess;
        public bool IsRunning => ScriptProcess?.HasExited ?? false;
        private readonly System.Timers.Timer timer;

        public DDosDetectionService()
        {
            timer = new System.Timers.Timer(600000);
            timer.Elapsed += TimerElapsed!;
            timer.AutoReset = true;
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Start();
        }
        public void Start()
        {
            Stop();
            Task.Run(RunDetection);
        }
        public void Stop()
        {
            if (ScriptProcess != null && !ScriptProcess.HasExited)
            {
                ScriptProcess.Kill();
                ScriptProcess.WaitForExit();
            }
        }

        private async Task RunDetection()
        {
            try
            {
                Program.logger.Log(LogType.Info, "Starting DDoS detection");
                if (!File.Exists("ddosDetection.bash"))
                {
                    Program.logger.Log(LogType.Info, "Downloading script");
                    using var httpClient = new HttpClient();
                    var script = await httpClient.GetStringAsync("https://gist.githubusercontent.com/Marcel-Baumgartner/0310679f6f6e03a4bad26d784231fa13/raw/ddosDetection.sh");
                    await File.WriteAllTextAsync("ddosDetection.bash", script);
                }

                Program.logger.Log(LogType.Info, "Executing script...");
                ScriptProcess = await BashHelper.ExecuteCommandRaw("bash ddosDetection.bash");
                while (!ScriptProcess.StandardOutput.EndOfStream)
                {
                    var line = await ScriptProcess.StandardOutput.ReadLineAsync();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (!line.StartsWith("DATA"))
                        continue;

                    var parts = line.Trim().Split(":");

                    if (parts[1] == "START")
                    {
                        var ip = parts[2];
                        var packets = parts[3];

                        Program.logger.Log(LogType.Warning, "Server under DDoS attack!");

                    }
                    else if (parts[1] == "END")
                    {
                        var ip = parts[2];
                        var traffic = parts[3];

                        Program.logger.Log(LogType.Warning, "Server no longer under DDoS attack!");
                    }
                }

                await ScriptProcess.WaitForExitAsync();
                Program.logger.Log(LogType.Error, "DDoS detection script stopped. Restart the daemon to start again");
            }
            catch (Exception e)
            {
                Program.logger.Log(LogType.Error, "Error running ddos detection: " + e.Message);
            }
        }
    }
}