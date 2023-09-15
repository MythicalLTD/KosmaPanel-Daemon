using System;
using KosmaPanel;
using KosmaPanel.Helpers;

namespace KosmaPanel.Managers;

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
