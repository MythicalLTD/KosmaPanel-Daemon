namespace KosmaPanel.Helpers.Apache2Helper {
    public class Apache2Helper {
        public static async Task<string> Restart() {
            return await BashHelper.BashHelper.ExecuteCommand("systemctl restart apache2");
        }

        public static async Task<string> Stop() {
            return await BashHelper.BashHelper.ExecuteCommand("systemctl stop apache2");
        }

        public static async Task<string> Start() {
            return await BashHelper.BashHelper.ExecuteCommand("systemctl start apache2");
        }
    }
}