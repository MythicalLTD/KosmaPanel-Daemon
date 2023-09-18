using System.Diagnostics;
using KosmaPanel.Managers.LoggerManager;

namespace KosmaPanel.Helpers.CertbotHelper
{
    public class CertbotHelper
    {
        private readonly string domain;
        public CertbotHelper(string domain)
        {
            this.domain = domain;
        }
        public void GenerateCertificate()
        {
            try
            {
                string command = $"certbot certonly --apache --non-interactive --agree-tos --register-unsafely-without-email -d {domain}";

                using (var process = new Process())
                {
                    process.StartInfo.FileName = "/bin/bash";
                    process.StartInfo.Arguments = $"-c \"{command}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        Program.logger.Log(LogType.Info, "Certificate successfully obtained.");
                    }
                    else
                    {
                        Program.logger.Log(LogType.Error, $"Error generating certificate: {error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, $"An error occurred: {ex.Message}");
            }
        }
    }
}