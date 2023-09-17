using KosmaPanel.Helpers.BashHelper;

namespace KosmaPanel.Managers.PowerManager
{
    public class PowerManager
    {
        public static async void RebootServerLinux()
        {
            await BashHelper.ExecuteCommand("sudo reboot");
        }

        public static async void ShutdownServerLinux()
        {
            await BashHelper.ExecuteCommand("sudo poweroff");
        }
    }

}