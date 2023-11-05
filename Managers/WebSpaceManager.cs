using System;
using KosmaPanel.Helpers.CertbotHelper;
using KosmaPanel.Helpers.WebServerHelper;

namespace KosmaPanel.Managers.WebSpaceManager
{
    public class WebSpaceManager
    {
        public static async Task<string> New(string webserver_port, string ssh_user, string ssh_password, string mysql_port, string ssh_port, string daemon_port, string daemon_key, string daemon_domain, string img_name)
        {
            try
            {
                if (webserver_port == null || ssh_user == null || ssh_password == null || mysql_port == null || ssh_port == null || daemon_port == null || daemon_key == null || daemon_domain == null || img_name == null)
                {
                    return "Please provide all required values";
                }
                else
                {
                    string img_status = ImageManager.ImageManager.CheckImageExists_String(img_name);
                    if (img_status == "Image exists in both the folder and the database.")
                    {
                        string certificateGenerated = CertbotHelper.GenerateCertificate(daemon_domain);
                        if (certificateGenerated == "Certificate successfully obtained.")
                        {
                            string dockerManagerMsg = DockerManager.DockerManager.RunContainerWithVolume(daemon_domain, webserver_port, ssh_user, ssh_password, mysql_port, ssh_port, daemon_port, daemon_key, "html");
                            if (dockerManagerMsg == "Container successfully started.")
                            {
                                string webserverconfig = await WebServerHelper.New(daemon_domain, webserver_port);
                                if (webserverconfig == "We created the nginx file.")
                                {
                                    string webserverstauts = WebServerHelper.Restart();
                                    if (webserverstauts == "WebServer successfully restarted.")
                                    {
                                        return "We just created the website!";
                                    }
                                    else
                                    {
                                        return webserverstauts;
                                    }
                                }
                                else
                                {
                                    return webserverconfig;
                                }
                            }
                            else
                            {
                                return dockerManagerMsg;
                            }
                        }
                        else
                        {
                            return certificateGenerated;
                        }
                    }
                    else
                    {
                        return img_status;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
