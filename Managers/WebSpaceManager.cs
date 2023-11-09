using System;
using KosmaPanel.Helpers.CertbotHelper;
using KosmaPanel.Helpers.WebServerHelper;

namespace KosmaPanel.Managers.WebSpaceManager
{
    public class WebSpaceManager
    {
        public static async Task<string> Remove(string cname)
        {
            try
            {
                if (cname == null)
                {
                    return "Please provide all required values";

                }
                else
                {
                    string wbhelper = await WebServerHelper.Remove(cname);
                    if (wbhelper == "We created the nginx file.")
                    {
                        string webserverstauts = WebServerHelper.Restart();
                        if (webserverstauts == "WebServer successfully restarted.")
                        {
                            string certbotstatus = CertbotHelper.DeleteCertificate(cname);
                            if (certbotstatus == "Certificate successfully deleted.")
                            {
                                string killctstatus = DockerManager.DockerManager.KillContainer(cname);
                                if (killctstatus == "Container successfully killed.")
                                {
                                    string rmctstatus = DockerManager.DockerManager.DeleteContainer(cname);
                                    if (rmctstatus == "Container successfully deleted.")
                                    {
                                        return "We just deleted the website!";
                                    }
                                    else
                                    {
                                        return rmctstatus;
                                    }
                                }
                                else
                                {
                                    return killctstatus;
                                }
                            }
                            else
                            {
                                return certbotstatus;
                            }
                        }
                        else
                        {
                            return webserverstauts;
                        }
                    }
                    else
                    {
                        return wbhelper;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";

            }
        }

        public static string Stop(string cname)
        {
            try
            {
                if (cname == null)
                {
                    return "Please provide all required values";
                }
                else
                {
                    string ctstopstatus = DockerManager.DockerManager.StopContainer(cname);
                    if (ctstopstatus == "Container successfully stopped.")
                    {
                        return "Container successfully stopped.";
                    }
                    else
                    {
                        return ctstopstatus;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public static string Start(string cname)
        {
            try
            {
                if (cname == null)
                {
                    return "Please provide all required values";
                }
                else
                {
                    string ctstartstatus = DockerManager.DockerManager.StartContainer(cname);
                    if (ctstartstatus == "Container successfully started.")
                    {
                        return "Container successfully started.";
                    }
                    else
                    {
                        return ctstartstatus;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public static string Reboot(string cname)
        {
            try
            {
                if (cname == null)
                {
                    return "Please provide all required values";
                }
                else
                {
                    string ctrebootstatus = DockerManager.DockerManager.RebootContainer(cname);
                    if (ctrebootstatus == "Container successfully rebooted.")
                    {
                        return "Container successfully rebooted.";
                    }
                    else
                    {
                        return ctrebootstatus;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public static string Kill(string cname)
        {
            try
            {
                if (cname == null)
                {
                    return "Please provide all required values";
                }
                else
                {
                    string ctkillstatus = DockerManager.DockerManager.KillContainer(cname);
                    if (ctkillstatus == "Container successfully killed.")
                    {
                        return "Container successfully killed.";
                    }
                    else
                    {
                        return ctkillstatus;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }
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
