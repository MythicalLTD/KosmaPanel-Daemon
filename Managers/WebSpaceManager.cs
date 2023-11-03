using KosmaPanel.Helpers.CertbotHelper;

namespace KosmaPanel.Managers.WebSpaceManager
{
    public class WebSpaceManager
    {
        public static string New(string webserver_port, string ssh_user, string ssh_password, string mysql_port, string ssh_port, string daemon_port, string daemon_key, string daemon_domain) {
            string certificateGenerated = CertbotHelper.GenerateCertificate(daemon_domain, "nginx");
            if (certificateGenerated == "Certificate successfully obtained.") {
                
                return "Succes";
            } else {
                return certificateGenerated;
            }
        }
    }
}