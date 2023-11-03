
using System.Net;
using System.Text;
using KosmaPanel.Managers.ConfigManager;
using KosmaPanel.Managers.LoggerManager;
using KosmaPanel.Managers.PowerManager;
using KosmaPanel.Managers.ServiceManager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Renci.SshNet;

namespace KosmaPanel.Services.WebServerService
{

    class WebServerService
    {

        public void Start(string d_port, string d_host)
        {
            //Console.SetOut(TextWriter.Null);
            //Console.SetError(TextWriter.Null);

            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    int port = int.Parse(d_port);
                    options.Listen(IPAddress.Parse(d_host), port);
                })
                .Configure(ConfigureApp)
                .Build();

            host.Run();
        }
        private static void ConfigureApp(IApplicationBuilder app)
        {
            app.Run(ProcessRequest);
        }
        private static async Task ProcessRequest(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var (isValidKey, keyMessage) = IsAuthorized(request);
            if (isValidKey)
            {
                var absolutePath = request.Path.Value.TrimStart('/');
                switch (absolutePath)
                {
                    case "":
                        {
                            var errorResponse = new
                            {
                                message = "Bad Request",
                                error = "Please provide a valid API endpoint."
                            };
                            var errorJson = JsonConvert.SerializeObject(errorResponse);
                            var errorBuffer = Encoding.UTF8.GetBytes(errorJson);
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.ContentType = "application/json";
                            response.ContentLength = errorBuffer.Length;
                            await response.Body.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                            break;
                        }
                    case "system/shutdown":
                        {
                            var shutdownResponse = new
                            {
                                message = "Server shutdown initiated",
                                status = "Please wait..."
                            };

                            var shutdownJson = JsonConvert.SerializeObject(shutdownResponse);
                            var shutdownBuffer = Encoding.UTF8.GetBytes(shutdownJson);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";
                            response.ContentLength = shutdownBuffer.Length;
                            await response.Body.WriteAsync(shutdownBuffer, 0, shutdownBuffer.Length);
                            await Task.Delay(5000);
                            PowerManager.ShutdownServerLinux();
                            break;
                        }
                    case "webspaces/create":
                        {
                            
                            break;
                        }
                    case "daemon/shutdown":
                        {
                            var shutdownResponse = new
                            {
                                message = "Daemon shutdown initiated",
                                status = "Please wait..."
                            };

                            var shutdownJson = JsonConvert.SerializeObject(shutdownResponse);
                            var shutdownBuffer = Encoding.UTF8.GetBytes(shutdownJson);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";
                            response.ContentLength = shutdownBuffer.Length;
                            await response.Body.WriteAsync(shutdownBuffer, 0, shutdownBuffer.Length);
                            await Task.Delay(5000);
                            await ServiceManager.Stop("daemon");
                            break;
                        }
                    case "daemon/reboot":
                        {
                            var rebootResponse = new
                            {
                                message = "Daemon reboot initiated",
                                status = "Please wait..."
                            };

                            var rebootJson = JsonConvert.SerializeObject(rebootResponse);
                            var rebootBuffer = Encoding.UTF8.GetBytes(rebootJson);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";
                            response.ContentLength = rebootBuffer.Length;
                            await response.Body.WriteAsync(rebootBuffer, 0, rebootBuffer.Length);
                            await Task.Delay(5000);
                            await ServiceManager.Restart("daemon");
                            break;
                        }

                    case "system/info":
                        {
                            var osInfo = new
                            {
                                code = 200,
                                os_type = LinuxMetricsService.LinuxMetricsService.os_type,
                                kernel = LinuxMetricsService.LinuxMetricsService.kernel_name,
                                uptime = LinuxMetricsService.LinuxMetricsService.uptime,
                                disk_info = new
                                {
                                    total = LinuxMetricsService.LinuxMetricsService.disk_total,
                                    used = LinuxMetricsService.LinuxMetricsService.disk_used,
                                    free = LinuxMetricsService.LinuxMetricsService.disk_free,
                                },
                                ram_info = new
                                {
                                    total = LinuxMetricsService.LinuxMetricsService.ram_total,
                                    used = LinuxMetricsService.LinuxMetricsService.ram_used,
                                    free = LinuxMetricsService.LinuxMetricsService.ram_free,
                                },
                                cpu_info = new
                                {
                                    name = LinuxMetricsService.LinuxMetricsService.cpu_model,
                                    usage = LinuxMetricsService.LinuxMetricsService.cpu_usage,
                                },

                            };
                            var osInfoJson = JsonConvert.SerializeObject(osInfo);
                            var osInfoBuffer = Encoding.UTF8.GetBytes(osInfoJson);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";
                            response.ContentLength = osInfoBuffer.Length;
                            await response.Body.WriteAsync(osInfoBuffer, 0, osInfoBuffer.Length);
                            break;
                        }
                    case "system/reboot":
                        {
                            var rebootResponse = new
                            {
                                message = "Server reboot initiated",
                                status = "Please wait..."
                            };

                            var rebootJson = JsonConvert.SerializeObject(rebootResponse);
                            var rebootBuffer = Encoding.UTF8.GetBytes(rebootJson);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";
                            response.ContentLength = rebootBuffer.Length;
                            await response.Body.WriteAsync(rebootBuffer, 0, rebootBuffer.Length);
                            await Task.Delay(5000);
                            PowerManager.RebootServerLinux();
                            break;
                        }
                    case "test":
                        {
                            var presponse = new
                            {
                                message = "Example Request",
                                error = "This is an example request"
                            };
                            var pjson = JsonConvert.SerializeObject(presponse);
                            var pBuffer = Encoding.UTF8.GetBytes(pjson);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.ContentType = "application/json";
                            response.ContentLength = pBuffer.Length;
                            await response.Body.WriteAsync(pBuffer, 0, pBuffer.Length);
                            break;
                        }
                    case "system/execute":
                        {
                            string command = request.Query["command"]!;
                            try
                            {
                                var sshHost = ConfigManager.GetSetting("Daemon", "ssh_ip");
                                var sshUsername = ConfigManager.GetSetting("Daemon", "ssh_username");
                                var sshPassword = ConfigManager.GetSetting("Daemon", "ssh_password");
                                var sshPort = int.Parse(ConfigManager.GetSetting("Daemon", "ssh_port"));
                                try
                                {
                                    using var client = new SshClient(sshHost, sshPort, sshUsername, sshPassword);
                                    client.Connect();
                                    var sshCommand = client.CreateCommand(command);
                                    var result = sshCommand.Execute();
                                    client.Disconnect();

                                    var executeResponse = new
                                    {
                                        message = "Command Executed",
                                        result = result
                                    };

                                    var executeJson = JsonConvert.SerializeObject(executeResponse);
                                    var executeBuffer = Encoding.UTF8.GetBytes(executeJson);
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    response.ContentType = "application/json";
                                    response.ContentLength = executeBuffer.Length;
                                    await response.Body.WriteAsync(executeBuffer, 0, executeBuffer.Length);
                                }
                                catch (Exception ex)
                                {
                                    Program.logger.Log(LogType.Error, $"[WebServer] {ex.Message}");
                                    var errorResponse = new
                                    {
                                        message = "I'm sorry, but I can't reach the web manager ssh client!",
                                        error = ex.Message
                                    };

                                    var errorJson = JsonConvert.SerializeObject(errorResponse);
                                    var errorBuffer = Encoding.UTF8.GetBytes(errorJson);
                                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                    response.ContentType = "application/json";
                                    response.ContentLength = errorBuffer.Length;
                                    await response.Body.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                                }
                            }
                            catch (Exception ex)
                            {
                                Program.logger.Log(LogType.Error, $"[WebServer] {ex.Message}"); 
                                var errorResponse = new
                                {
                                    message = "I'm sorry, but some unexpected error got thrown out, and I don't know how to handle it. Please contact support.",
                                    error = ex.Message
                                };

                                var errorJson = JsonConvert.SerializeObject(errorResponse);
                                var errorBuffer = Encoding.UTF8.GetBytes(errorJson);
                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                response.ContentType = "application/json";
                                response.ContentLength = errorBuffer.Length;
                                await response.Body.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                            }

                            break;
                        }
                    default:
                        {
                            var errorResponse = new
                            {
                                message = "Page not found",
                                error = "The requested page does not exist."
                            };
                            var errorJson = JsonConvert.SerializeObject(errorResponse);
                            var errorBuffer = Encoding.UTF8.GetBytes(errorJson);
                            response.StatusCode = (int)HttpStatusCode.NotFound;
                            response.ContentType = "application/json";
                            response.ContentLength = errorBuffer.Length;
                            await response.Body.WriteAsync(errorBuffer, 0, errorBuffer.Length);
                            break;
                        }
                }
            }
            else
            {
                var errorResponse = new
                {
                    message = keyMessage,
                    error = "Invalid API key."
                };
                var errorJson = JsonConvert.SerializeObject(errorResponse);
                var errorBuffer = Encoding.UTF8.GetBytes(errorJson);
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.ContentType = "application/json";
                response.ContentLength = errorBuffer.Length;
                await response.Body.WriteAsync(errorBuffer, 0, errorBuffer.Length);
            }
        }
        private static (bool isValid, string message) IsAuthorized(HttpRequest request)
        {
            string apiKey = request.Headers["Authorization"]!;
            if (string.IsNullOrEmpty(apiKey))
            {
                return (false, "API key is empty.");
            }

            if (apiKey == ConfigManager.GetSetting("Daemon", "key"))
            {
                return (true, "Authorized.");
            }

            return (false, "API key is invalid.");
        }
    }

}