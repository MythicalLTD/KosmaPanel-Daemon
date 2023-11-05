using System.Diagnostics;
using KosmaPanel.Managers.LoggerManager;

namespace KosmaPanel.Helpers.WebServerHelper
{
    public class WebServerHelper
    {
        public static string Restart()
        {
            string command = $"systemctl restart nginx";

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
                    Program.logger.Log(LogType.Info, $"WebServer successfully restarted.");
                    return "WebServer successfully restarted.";
                }
                else
                {
                    Program.logger.Log(LogType.Error, $"Failed to restart the webserver: {error}");
                    return $"Failed to restart the webserver: {error}";
                }
            }
        }

        public static async Task<string> New(string domain, string port)
        {

            string nginxConfig = $@"
server {{
    listen 80;
    server_name {domain};
    return 301 https://$server_name$request_uri;
}}

server {{
    listen 443 ssl http2;
    server_name {domain};

    ssl_certificate /etc/letsencrypt/live/{domain}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/{domain}/privkey.pem;
    ssl_session_cache shared:SSL:10m;
    ssl_protocols TLSv1.2 TLSv1.3; 
    ssl_ciphers 'TLS_AES_128_GCM_SHA256:TLS_AES_256_GCM_SHA384:TLS_CHACHA20_POLY1305_SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-RSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers off;

    location / {{
        proxy_pass http://localhost:{port}/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection ""Upgrade"";
        proxy_set_header Host $host;
        proxy_buffering off;
        proxy_set_header X-Real-IP $remote_addr;
    }}

    location /ws {{
        proxy_pass http://localhost:{port}/ws;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection ""Upgrade"";
        proxy_set_header Host $host;
        proxy_buffering off;
        proxy_set_header X-Real-IP $remote_addr;
    }}
}}";

            string filePath = $"/etc/nginx/sites-available/{domain}.conf";
            string fileContent = nginxConfig;

            try
            {
                File.WriteAllText(filePath, fileContent);

                string enableCommand = $"sudo ln -s {filePath} /etc/nginx/sites-enabled/{domain}.conf";
                await BashHelper.BashHelper.ExecuteCommand(enableCommand);
                Program.logger.Log(LogType.Info, $"[{domain}] We created the nginx file.");
                return "We created the nginx file.";
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, $"[{domain}] Failed to create the nginx file: {ex.Message}");
                return $"Failed to create the nginx file: {ex.Message}";
            }
        }
    }
}