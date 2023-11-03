using System.Diagnostics;
using System.Net;
using KosmaPanel.Managers.LoggerManager;

namespace KosmaPanel.Helpers.CertbotHelper
{
    public class CertbotHelper
    {
        public static string GenerateCertificate(string domain, string webserver)
        {
            try
            {
                if (webserver == "nginx")
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(domain);

                    if (addresses.Length == 0)
                    {
                        Program.logger.Log(LogType.Warning, $"Domain '{domain}' does not resolve to any IP address.");
                        return $"[{domain}] Certificate generation failed: Domain does not resolve to any IP address.";
                    }
                    else
                    {
                        string command = $"certbot certonly --nginx --non-interactive --agree-tos --register-unsafely-without-email -d {domain}";

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
                                Program.logger.Log(LogType.Info, $"[{domain}] Certificate successfully obtained.");
                                return "Certificate successfully obtained.";
                            }
                            else
                            {
                                Program.logger.Log(LogType.Error, $"[{domain}] Error generating certificate: {error}");
                                return $"Error generating certificate: {error}";
                            }
                        }
                    }
                }
                else if (webserver == "apache2")
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(domain);

                    if (addresses.Length == 0)
                    {
                        Program.logger.Log(LogType.Warning, $"Domain '{domain}' does not resolve to any IP address.");
                        return "Certificate generation failed: Domain does not resolve to any IP address.";
                    }
                    else
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
                                Program.logger.Log(LogType.Info, $"[{domain}] Certificate successfully obtained.");
                                return "Certificate successfully obtained.";
                            }
                            else
                            {
                                Program.logger.Log(LogType.Error, $"[{domain}] Error generating certificate: {error}");
                                return $"Error generating certificate: {error}";
                            }
                        }
                    }
                }
                else
                {
                    Program.logger.Log(LogType.Error, $"[{domain}] Please define a webserver that exists: nginx | apache2");
                    return $"An error occurred";
                }
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, $"[{domain}] An error occurred: {ex.Message}");
                return $"An error occurred: {ex.Message}";
            }
        }

        public static string RenewCertificate(string domain)
        {
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(domain);

                if (addresses.Length == 0)
                {
                    Program.logger.Log(LogType.Warning, $"Domain '{domain}' does not resolve to any IP address.");
                    return "Certificate generation failed: Domain does not resolve to any IP address.";
                }
                else
                {
                    string command = $"certbot renew --cert-name {domain}";

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
                            Program.logger.Log(LogType.Info, $"[{domain}] Certificate successfully obtained.");
                            return "Certificate successfully obtained.";
                        }
                        else
                        {
                            Program.logger.Log(LogType.Error, $"[{domain}] Error generating certificate: {error}");
                            return $"Error generating certificate: {error}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, $"[{domain}] An error occurred: {ex.Message}");
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}