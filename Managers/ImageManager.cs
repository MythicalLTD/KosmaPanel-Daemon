using KosmaPanel.Managers.LoggerManager;

namespace KosmaPanel.Managers.ImageManager
{
    public class ImageManager
    {
        public static void Check()
        {
            if (Directory.Exists("/etc/KosmaPanel/images"))
            {
                Program.logger.Log(LogType.Warning, "It looks like the image folder is not empty. Please wait while we run a small checkup on the images.");

            }
            else
            {
                Directory.CreateDirectory("/etc/KosmaPanel/images");
                DownloadEverything();
            }
        }
        private async static void DownloadEverything() {
            await DownloadImage("html");
            //await DownloadImage("html");
        }
        private static async Task DownloadImage(string name)
        {
            if (name == "")
            {
                Program.logger.Log(LogType.Error, "Failed to download this Docker image because the image name is empty.");
            }
            else
            {
                Program.logger.Log(LogType.Warning, $"Please wait while we download the image {name}");
                try
                {
                    string imageUrl = $"https://github.com/MythicalLTD/KosmaPanel-Images/releases/latest/download/{name}.dockerfile";
                    bool imageExists = await ImageExists(imageUrl);
                    if (imageExists)
                    {
                        Program.logger.Log(LogType.Warning, $"Please wait while we download the image {name}");
                        try
                        {
                            await Download(imageUrl,name);
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

                    string savePath = Path.Combine("/etc/KosmaPanel/images", $"{name}.dockerfile");

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
                }
                else
                {
                    Program.logger.Log(LogType.Error, $"Failed to download Docker image {name}. HTTP status code: {response.StatusCode}");
                }
            }
        }
    }
}