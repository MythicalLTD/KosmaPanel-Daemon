using System.Net;
using System.Text;
using Renci.SshNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Salaros.Configuration;
using Newtonsoft.Json;

namespace KosmaPanelWebManager
{
    public class Program
    {
        public static string d_host = string.Empty;
        public static string d_port = string.Empty;
        public static string d_key = string.Empty;
        public static string d_settings = string.Empty;
        public static string d_ssh_ip = string.Empty;
        public static int d_ssh_port;
        public static string d_ssh_username = string.Empty;
        public static string d_ssh_password = string.Empty;
        public static string mcascii = @" 
  _  __                         _____                 _ 
 | |/ /                        |  __ \               | |
 | ' / ___  ___ _ __ ___   __ _| |__) |_ _ _ __   ___| |
 |  < / _ \/ __| '_ ` _ \ / _` |  ___/ _` | '_ \ / _ \ |
 | . \ (_) \__ \ | | | | | (_| | |  | (_| | | | |  __/ |
 |_|\_\___/|___/_| |_| |_|\__,_|_|   \__,_|_| |_|\___|_|
                                                        
    ";
        public static string version = "1.0.0";
        public static Logger logger = new Logger();

        public static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine(mcascii);
            if (!OperatingSystem.IsLinux())
            {
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Sorry you have to be on debain / linux to use our daemon");
            }
            if (args.Contains("-version"))
            {
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) You are running version: '" + version + "'", DateTime.Now);
                Environment.Exit(0x0);
            }
            else if (args.Contains("-resetkey"))
            {
                d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
                try
                {
                    var cfg = new ConfigParser(d_settings);
                    string skey = KeyChecker.GenerateStrongKey();
                    cfg.SetValue("Daemon", "key", skey);
                    cfg.Save();
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) We updated your daemon settings", DateTime.Now);
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Your key is: '" + skey + "'", DateTime.Now);
                    Environment.Exit(0x0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[{0:HH:mm:ss}] (Daemon) Failed to generate a key: " + ex.Message, DateTime.Now);
                    Environment.Exit(0x0);
                }
            }
            else if (args.Length > 0)
            {
                Console.WriteLine("[{0:HH:mm:ss}] (Daemon) This is an invalid startup argument. Please use '-help' to get more information.", DateTime.Now);
                Environment.Exit(0x0);
            }
            logger.Log(LogType.Info, "Please wait while we start KosmaPanelWebManager");
            LoadSettings();
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    int port = int.Parse(d_port);
                    options.Listen(IPAddress.Parse(d_host), port);
                })
                .Configure(ConfigureApp)
                .Build();
            logger.Log(LogType.Info, "Daemon started on: " + d_host + ":" + d_port);
            logger.Log(LogType.Info, "Secret key: " + d_key);
            host.Run();
        }
        private static void LoadSettings()
        {
            try
            {
                d_settings = Directory.GetCurrentDirectory() + @"/config.ini";
                var cfg = new ConfigParser(d_settings);
                if (!File.Exists(d_settings))
                {
                    cfg.SetValue("Daemon", "host", "127.0.0.1");
                    cfg.SetValue("Daemon", "port", "1953");
                    cfg.SetValue("Daemon", "key", "");
                    cfg.SetValue("Daemon", "ssh_ip", "");
                    cfg.SetValue("Daemon", "ssh_port", "");
                    cfg.SetValue("Daemon", "ssh_username", "");
                    cfg.SetValue("Daemon", "ssh_password", "");
                    cfg.Save();
                    logger.Log(LogType.Warning, "Looks like this is your first time running our daemon. Please close the app, go into config.ini, and configure your app");
                    Environment.Exit(0x0);
                }
                d_host = cfg.GetValue("Daemon", "host");
                d_port = cfg.GetValue("Daemon", "port");
                d_key = cfg.GetValue("Daemon", "key");
                d_ssh_ip = cfg.GetValue("Daemon", "ssh_ip");
                d_ssh_port = int.Parse(cfg.GetValue("Daemon", "ssh_port"));
                d_ssh_username = cfg.GetValue("Daemon", "ssh_username");
                d_ssh_password = cfg.GetValue("Daemon", "ssh_password");
                if (d_host == "")
                {
                    d_host = "127.0.0.1";
                }
                if (d_port == "")
                {
                    d_port = "1953";
                }
                if (d_ssh_ip == "")
                {
                    logger.Log(LogType.Error, "Failed to start: 'We did not find any ssh connection information inside the settings.ini.'");
                    Environment.Exit(0x0);
                }
                if (d_ssh_port.ToString() == "")
                {
                    logger.Log(LogType.Error, "Failed to start: 'We did not find any ssh connection information inside the settings.ini.'");
                    Environment.Exit(0x0);
                }
                if (d_ssh_username == "")
                {
                    logger.Log(LogType.Error, "Failed to start: 'We did not find any ssh connection information inside the settings.ini.'");
                    Environment.Exit(0x0);
                }
                if (d_ssh_password == "")
                {
                    logger.Log(LogType.Error, "Failed to start: 'We did not find any ssh connection information inside the settings.ini.'");
                    Environment.Exit(0x0);
                }
                if (d_key == "")
                {
                    logger.Log(LogType.Error, "Failed to start: 'Please use a strong key'");
                    Environment.Exit(0x0);
                }
                if (!KeyChecker.isStrongKey(d_key))
                {
                    logger.Log(LogType.Error, "Failed to start: 'Please use a strong key'");
                    Environment.Exit(0x0);
                }
                logger.Log(LogType.Info, "Loaded daemon config from 'config.ini'");
            }
            catch (Exception ex)
            {
                logger.Log(LogType.Error, "Failed to load config: " + ex.Message);
                Environment.Exit(0x0);
            }
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
                    case "execute":
                        {
                            string command = request.Query["command"];
                            try
                            {
                                var sshHost = d_ssh_ip;
                                var sshUsername = d_ssh_username;
                                var sshPassword = d_ssh_password;
                                var sshPort = d_ssh_port;
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
                                } catch (Exception ex) {
                                    Console.WriteLine("Error: " + ex.Message);
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
                                Console.WriteLine("Error: " + ex.Message);
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
            string apiKey = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return (false, "API key is empty.");
            }

            if (apiKey == d_key)
            {
                return (true, "Authorized.");
            }

            return (false, "API key is invalid.");
        }

    }

}