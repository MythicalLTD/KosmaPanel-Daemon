using System.Diagnostics;

namespace KosmaPanel.Managers.VolumeManager
{
    public class VolumeManager
    {
        private static readonly string VolumePath = "/etc/KosmaPanel/volumes";

        public static string Create(string volumeName, string size)
        {
            EnsureVolumePathExists();
            string createVolumeCommand = $"docker volume create --name {volumeName} --opt type=none --opt device={VolumePath}/{volumeName} --opt o=size={size}";

            using (var process = new Process())
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{createVolumeCommand}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return "Volume created successfully.";
                }
                else
                {
                    return $"Error creating volume: {error}";
                }
            }
        }
        private static void EnsureVolumePathExists()
        {
            if (!Directory.Exists(VolumePath))
            {
                Directory.CreateDirectory(VolumePath);
            }
        }
        public static string Delete(string volumeName)
        {
            string deleteVolumeCommand = $"docker volume rm {volumeName}";

            using (var process = new Process())
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{deleteVolumeCommand}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return "Volume deleted successfully.";
                }
                else
                {
                    return $"Error deleting volume: {error}";
                }
            }
        }

        public static string Resize(string volumeName, string newSize)
        {
            string resizeVolumeCommand = $"docker volume resize {volumeName} --size {newSize}";

            using (var process = new Process())
            {
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = $"-c \"{resizeVolumeCommand}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return "Volume resized successfully.";
                }
                else
                {
                    return $"Error resizing volume: {error}";
                }
            }
        }
    }
}
