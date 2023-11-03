using Docker.DotNet;
using Docker.DotNet.Models;

namespace KosmaPanel.Managers.DockerManager
{
    public class DockerManager
    {
        private DockerClient _dockerClient;

        public DockerManager(string dockerHost = "unix:///var/run/docker.sock")
        {
            _dockerClient = new DockerClientConfiguration(new Uri(dockerHost)).CreateClient();
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
