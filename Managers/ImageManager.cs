using KosmaPanel.Helpers.BashHelper;
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
        public static void Check()
        {
            if (Directory.Exists("/etc/KosmaPanel/images"))
            {
                Program.logger.Log(LogType.Info, "It looks like the image folder is not empty. Please wait while we run a small checkup on the images.");
                CheckAllImages();
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
        public static async void CheckImageExists(string imageName)
        {
            bool imageExistsInFolder = File.Exists($"/etc/KosmaPanel/images/{imageName}/Dockerfile");

            bool imageExistsInDatabase = ImageExistsInDatabase(imageName);

            if (imageExistsInFolder && imageExistsInDatabase)
            {
                Program.logger.Log(LogType.Info, $"Image '{imageName}' exists in both the folder and the database.");
            }
            else if (!imageExistsInFolder && imageExistsInDatabase)
            {
                Program.logger.Log(LogType.Info, $"Image '{imageName}' exists in the database but not in the folder. Downloading...");
                await DownloadImage(imageName);
            }
            else if (imageExistsInFolder && !imageExistsInDatabase)
            {
                Program.logger.Log(LogType.Info, $"Image '{imageName}' exists in the folder but not in the database. Adding to the database...");
                AddImageToDatabase(imageName);
            }
            else
            {
                Program.logger.Log(LogType.Info, $"Image '{imageName}' does not exist in the folder or the database. Downloading...");
                await DownloadImage(imageName);
            }
        }
        private static bool ImageExistsInDatabase(string imageName)
        {
            getConnection();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT COUNT(*) FROM images WHERE name = @imageName", connection))
                {
                    command.Parameters.AddWithValue("@imageName", imageName);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        public static void CheckAllImages()
        {
            getConnection();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT name FROM images", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string? imageName = reader["name"].ToString();
                        #pragma warning disable
                        CheckImageExists(imageName);
                        #pragma warning restore
                    }
                }
                connection.Close();
            }
        }

        private static void AddImageToDatabase(string imageName)
        {
            getConnection();
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                ExecuteScript(connection, "INSERT INTO `images` (`name`, `docker`) VALUES (@imageName, @dockerName)");
                connection.Close();
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