using KosmaPanel.Helpers.BashHelper;
namespace KosmaPanel.Managers.ServiceManager
{
    public class ServiceManager
    {
        public static async Task<string> Restart(string name)
        {
            return await BashHelper.ExecuteCommand($"systemctl restart {name}");
        }

        public static async Task<string> Stop(string name)
        {
            return await BashHelper.ExecuteCommand($"systemctl stop {name}");
        }

        public static async Task<string> Start(string name)
        {
            return await BashHelper.ExecuteCommand($"systemctl start {name}");
        }
    }
}