using System.Diagnostics;
using System.Net;
using KosmaPanel.Helpers.BashHelper;
using KosmaPanel.Managers.DockerManager;
using KosmaPanel.Managers.LoggerManager;
using MySqlConnector;

namespace KosmaPanel.Managers.ImageManager
{
    public class ImageManager
    {
        public static string? connectionString;
        private static void getConnection()
        {
            try
            {
                var dbHost = ConfigManager.ConfigManager.GetSetting("Daemon", "mysql_host");
                var dbPort = ConfigManager.ConfigManager.GetSetting("Daemon", "mysql_port");
                var dbUsername = ConfigManager.ConfigManager.GetSetting("Daemon", "mysql_username");
                var dbPassword = ConfigManager.ConfigManager.GetSetting("Daemon", "mysql_password");
                var dbName = ConfigManager.ConfigManager.GetSetting("Daemon", "mysql_db_name");
                connectionString = $"Server={dbHost};Port={dbPort};User ID={dbUsername};Password={dbPassword};Database={dbName}";
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, $"Failed to get the connection info from the settings file: \n" + ex.Message);
            }
        }
        private static void ExecuteScript(MySqlConnection connection, string scriptContent)
        {
            using (var command = new MySqlCommand(scriptContent, connection))
            {
                command.ExecuteNonQuery();
            }
        }
        private static void CheckImages()
        {

        }
        public static void Check()
        {
            if (Directory.Exists("/etc/KosmaPanel/images"))
            {
                Program.logger.Log(LogType.Info, "It looks like the image folder is not empty. Please wait while we run a small checkup on the images.");

            }
            else
            {
                Directory.CreateDirectory("/etc/KosmaPanel/images");
                DownloadEverything();
            }
        }
        private async static void DownloadEverything()
        {
            await DownloadImage("html");
        }
        private async static Task Build(string name)
        {
            try
            {
                await BashHelper.ExecuteCommand($"docker build -t kosmapanel:{name} /etc/KosmaPanel/images/{name}");
            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, $"We can't compile the docker image {name}: {ex.Message}");
            }
        }
        public static string RunContainer(string cname, string webserver_port, string ssh_user, string ssh_password, string mysql_port, string ssh_port, string daemon_port, string daemon_key)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(cname);

            if (addresses.Length == 0)
            {
                Program.logger.Log(LogType.Warning, $"Domain '{cname}' does not resolve to any IP address.");
                return "Certificate generation failed: Domain does not resolve to any IP address.";
            }
            else
            {
                string command = $"docker run -d -p {ssh_port}:22 -p {webserver_port}:80 -p {daemon_port}:99 -p {mysql_port}:3306 -e SFTP_USER={ssh_user} -e SFTP_PASSWORD={ssh_password} -e WEBMANAGER_KEY={daemon_key} --name {cname} --memory 512m --cpus 0.5 --volume web_data:/var/www/{cname} --restart unless-stopped kosmapanel:html";

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
                        Program.logger.Log(LogType.Info, $"[{cname}] Certificate successfully obtained.");
                        return "Certificate successfully obtained.";
                    }
                    else
                    {
                        Program.logger.Log(LogType.Error, $"[{cname}] Error generating certificate: {error}");
                        return $"Error generating certificate: {error}";
                    }
                }
            }
        }

        private static async Task DownloadImage(string name)
        {
            if (name == "")
            {
                Program.logger.Log(LogType.Error, "Failed to download this Docker image because the image name is empty.");
            }
            else
            {
                Program.logger.Log(LogType.Info, $"Please wait while we download the image: {name}");
                try
                {
                    Directory.CreateDirectory($"/etc/KosmaPanel/images/{name}");
                    string imageUrl = $"https://github.com/MythicalLTD/KosmaPanel-Images/releases/latest/download/{name}.dockerfile";
                    bool imageExists = await ImageExists(imageUrl);
                    if (imageExists)
                    {
                        Program.logger.Log(LogType.Info, $"Please wait while we download the image: {name}");
                        try
                        {
                            await Download(imageUrl, name);
                        }
                        catch (Exception ex)
                        {
                            Program.logger.Log(LogType.Error, $"Failed to download the docker image {name}: " + ex.ToString());
                        }
                    }
                    else
                    {
                        Program.logger.Log(LogType.Error, $"Docker image {name} does not exist at {imageUrl}");
                    }
                }
                catch (Exception ex)
                {
                    Program.logger.Log(LogType.Error, $"Failed to download the docker image {name}: " + ex.ToString());
                }
            }
        }

        private static async Task<bool> ImageExists(string imageUrl)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Head, imageUrl);
                try
                {
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (HttpRequestException)
                {
                    return false;
                }
            }
        }

        private static async Task Download(string imageUrl, string name)
        {
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    long contentLength = response.Content.Headers.ContentLength ?? -1;
                    var downloadStream = await response.Content.ReadAsStreamAsync();

                    string savePath = Path.Combine($"/etc/KosmaPanel/images/{name}", $"{name}.dockerfile");

                    using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;

                        while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;

                            Program.logger.Log(LogType.Info, $"Image {name} downloaded {totalBytesRead} of {contentLength} bytes");


                        }
                    }
                    Program.logger.Log(LogType.Info, $"Docker image {name} downloaded successfully.");
                    try
                    {
                        File.Move($"/etc/KosmaPanel/images/{name}/{name}.dockerfile", $"/etc/KosmaPanel/images/{name}/Dockerfile");
                    }
                    catch (Exception ex)
                    {
                        Program.logger.Log(LogType.Info, "Failed to rename the image: " + ex.Message);
                    }
                    try
                    {
                        Program.logger.Log(LogType.Info, $"Please wait while we compile: {name}");
                        await Build(name);
                        Program.logger.Log(LogType.Info, $"We compiled {name}");
                        try
                        {
                            getConnection();
                            using (var connection = new MySqlConnection(connectionString))
                            {
                                connection.Open();
                                ExecuteScript(connection, "INSERT INTO `images` (`name`, `docker`) VALUES ('" + name + "', 'kosmapanel:" + name + "');");
                                connection.Close();
                            }
                            Program.logger.Log(LogType.Info, "We saved the image in the database");
                        }
                        catch (Exception ex)
                        {
                            Program.logger.Log(LogType.Info, "Failed to save to the database: " + ex.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.logger.Log(LogType.Info, "Failed to compile: " + ex.Message);
                    }

                }
                else
                {
                    Program.logger.Log(LogType.Error, $"Failed to download Docker image {name}. HTTP status code: {response.StatusCode}");
                }
            }
        }
    }
}