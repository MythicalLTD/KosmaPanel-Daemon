using KosmaPanel.Helpers.CertbotHelper;

namespace KosmaPanel.Managers.WebSpaceManager
{
    public class WebSpaceManager
    {
        public static string New(string webserver_port, string ssh_user, string ssh_password, string mysql_port, string ssh_port, string daemon_port, string daemon_key, string daemon_domain, string img_name)
        {
            if (img_name == null)
            {
                return "This image dose not exist";
            }
            else {
                string certificateGenerated = CertbotHelper.GenerateCertificate(daemon_domain, "nginx");
                if (certificateGenerated == "Certificate successfully obtained.")
                {
                    string dockerManagerMsg = DockerManager.DockerManager.RunContainerWithVolume(daemon_domain, webserver_port, ssh_user, ssh_password, mysql_port, ssh_port, daemon_port, daemon_key, "html");
                    if (dockerManagerMsg == "")
                    {

                    }
                    else
                    {
                        return "Failed to create the website";
                    }
                    return "Succes";
                }
                else
                {
                    return certificateGenerated;
                }
            }
        }
    }
}