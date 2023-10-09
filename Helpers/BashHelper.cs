
using System.Diagnostics;
using KosmaPanel.Managers;
using KosmaPanel.Managers.LoggerManager;

namespace KosmaPanel.Helpers.BashHelper;
public class BashHelper
{
    public static async Task<string> ExecuteCommand(string command)
    {
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            Program.logger.Log(LogType.Error, await process.StandardError.ReadToEndAsync());
        }
        return output;
    }
    public static Task<Process> ExecuteCommandRaw(string command)
    {
        Process process = new Process();

        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.Start();

        return Task.FromResult(process);
    }
}