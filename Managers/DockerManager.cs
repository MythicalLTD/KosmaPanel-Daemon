using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;
using KosmaPanel.Managers.LoggerManager;

namespace KosmaPanel.Managers.DockerManager
{
    public class DockerManager
    {
        private DockerClient _dockerClient;

        public DockerManager(string dockerHost = "unix:///var/run/docker.sock")
        {
            _dockerClient = new DockerClientConfiguration(new Uri(dockerHost)).CreateClient();
        }

        public static string RunContainerWithVolume(string cname, string webserver_port, string ssh_user, string ssh_password, string mysql_port, string ssh_port, string daemon_port, string daemon_key, string type)
        {
            string volumeName = $"{cname}_web_data";
            string volumePath = $"/etc/KosmaPanel/volumes/{volumeName}";

            if (!Directory.Exists(volumePath))
            {
                string volumeCreationResult = VolumeManager.VolumeManager.Create(volumeName, "1g");
                if (!volumeCreationResult.StartsWith("Volume created successfully"))
                {
                    Program.logger.Log(LogType.Error, $"Error creating volume: {volumeCreationResult}");
                    return $"Error creating volume: {volumeCreationResult}";
                }
            }

            string command = $"docker run -d -p {ssh_port}:22 -p {webserver_port}:80 -p {daemon_port}:99 -p {mysql_port}:3306 -e SFTP_USER={ssh_user} -e SFTP_PASSWORD={ssh_password} -e WEBMANAGER_KEY={daemon_key} --name {cname} --memory 512m --cpus 0.5 --volume {volumeName}:/var/www/html --restart unless-stopped kosmapanel:{type}";

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
                    Program.logger.Log(LogType.Info, $"[{cname}] Container successfully started.");
                    return "Container successfully started.";
                }
                else
                {
                    Program.logger.Log(LogType.Error, $"[{cname}] Error starting container: {error}");
                    return $"Error starting container: {error}";
                }
            }
        }

        public async Task<IList<ContainerListResponse>> ListContainersAsync()
        {
            var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters()
            {
                All = true
            });
            return containers;
        }

        public async Task StartContainerAsync(string containerId)
        {
            await _dockerClient.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
        }

        public async Task StopContainerAsync(string containerId)
        {
            await _dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters());
        }

        public async Task RemoveContainerAsync(string containerId)
        {
            await _dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());
        }

        public async Task<bool> ContainerExistsAsync(string containerId)
        {
            var containers = await ListContainersAsync();
            return containers.Any(c => c.ID == containerId);
        }

        public async Task<IList<ImagesListResponse>> ListImagesAsync()
        {
            var images = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters()
            {
                All = true
            });
            return images;
        }
    }
}
