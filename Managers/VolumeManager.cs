using System.Diagnostics;

namespace KosmaPanel.Managers.VolumeManager
{
    public class VolumeManager
    {
        public static string Create(string volumeName, string size)
        {
            string deviceOption = "/etc/KosmaPanel/volumes";
            using (var process = new Process())
            {
                process.StartInfo.FileName = "docker";
                process.StartInfo.Arguments = $"volume create --name {volumeName} --opt type=none --opt device={deviceOption} --opt o=size={size}";
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

        public static string Delete(string volumeName)
        {
            string deleteVolumeCommand = $"docker volume rm {volumeName}";

            using (var process = new Process())
            {
                process.StartInfo.FileName = "docker";
                process.StartInfo.Arguments = $"volume rm {volumeName}";
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
                process.StartInfo.FileName = "docker";
                process.StartInfo.Arguments = $"volume resize {volumeName} --size {newSize}";
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
