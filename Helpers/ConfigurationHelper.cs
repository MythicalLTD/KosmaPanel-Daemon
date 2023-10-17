using KosmaPanel.Managers.ConfigManager;
using KosmaPanel.Managers.LoggerManager;
using MySqlConnector;
using Renci.SshNet;

namespace KosmaPanel.Helpers.ConfigurationHelper;

public class ConfigurationHelper
{
    public static void StartSetup()
    {
        string defaultHost = "127.0.0.1";
        string defaultPort = "5001";

        Program.logger.Log(LogType.Info, "Hi, please fill in your database configuration for KosmaPanel.");
        Console.Write("Host [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{defaultHost}");
        Console.ResetColor();
        Console.Write("]: ");

        string? host = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(host))
        {
            host = defaultHost;
        }

        Console.Write("Port [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{defaultPort}");
        Console.ResetColor();
        Console.Write("]: ");

        string? port = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(port))
        {
            port = defaultPort;
        }

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port))
        {
            Program.logger.Log(LogType.Error, "Invalid input. Please provide all the required values.");
            Program.Stop();
        }
        try
        {
            ConfigManager.UpdateSetting("Daemon", "host", host);
            ConfigManager.UpdateSetting("Daemon", "port", port);

            Program.logger.Log(LogType.Info, "Done we saved your settings in our configuration file");
            Program.Stop();
        }
        catch (Exception ex)
        {
            Program.logger.Log(LogType.Error, $"Failed to connect to MySQL: {ex.Message}");
            Program.Stop();
        }
    }
    public static void StartDBGUI()
    {
        string defaultHost = "127.0.0.1";
        string defaultPort = "3306";
        string defaultUsername = "KosmaPanel";
        string deafultdbName = "kosmapanel_daemon";

        Program.logger.Log(LogType.Info, "Hi, please fill in your database configuration for KosmaPanel.");
        Console.Write("Host [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{defaultHost}");
        Console.ResetColor();
        Console.Write("]: ");

        string? host = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(host))
        {
            host = defaultHost;
        }

        Console.Write("Port [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{defaultPort}");
        Console.ResetColor();
        Console.Write("]: ");

        string? port = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(port))
        {
            port = defaultPort;
        }

        Console.Write("Username [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{defaultUsername}");
        Console.ResetColor();
        Console.Write("]: ");

        string? username = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(username))
        {
            username = defaultUsername;
        }

        Console.Write("Password: ");
        string password = ReadPasswordInput();

        Console.Write("Database Name [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{deafultdbName}");
        Console.ResetColor();
        Console.Write("]: ");

        string? dbName = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(dbName))
        {
            dbName = deafultdbName;
        }
        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(dbName))
        {
            Program.logger.Log(LogType.Error, "Invalid input. Please provide all the required values.");
            Program.Stop();
        }
        try
        {
            using var connection = new MySqlConnection($"Server={host};Port={port};User ID={username};Password={password};Database={dbName}");
            connection.Open();
            Program.logger.Log(LogType.Info, "Connected to MySQL, saving database configuration to config.");
            connection.Close();
            ConfigManager.UpdateSetting("Daemon", "mysql_host", host);
            ConfigManager.UpdateSetting("Daemon", "mysql_port", port);
            ConfigManager.UpdateSetting("Daemon", "mysql_username", username);
            ConfigManager.UpdateSetting("Daemon", "mysql_password", password);
            ConfigManager.UpdateSetting("Daemon", "mysql_db_name", dbName);

            Program.logger.Log(LogType.Info, "Done we saved your MySQL connection to your config file");
            Program.Stop();
        }
        catch (Exception ex)
        {
            Program.logger.Log(LogType.Error, $"Failed to connect to MySQL: {ex.Message}");
            Program.Stop();
        }
    }

    public static void StartSSHGUI()
    {
        string defaultHost = "127.0.0.1";
        string defaultPort = "22";
        string defaultUsername = "root";

        Program.logger.Log(LogType.Info, "Hi, please fill in your ssh configuration for KosmaPanel.");
        Console.Write("Host [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{defaultHost}");
        Console.ResetColor();
        Console.Write("]: ");

        string? host = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(host))
        {
            host = defaultHost;
        }

        Console.Write("Port [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{defaultPort}");
        Console.ResetColor();
        Console.Write("]: ");

        string? port = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(port))
        {
            port = defaultPort;
        }

        Console.Write("Username [");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{defaultUsername}");
        Console.ResetColor();
        Console.Write("]: ");

        string? username = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(username))
        {
            username = defaultUsername;
        }

        Console.Write("Password: ");
        string password = ReadPasswordInput();

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Program.logger.Log(LogType.Error, "Invalid input. Please provide all the required values.");
            Program.Stop();
        }
        try
        {
            try
            {
                int sshport = int.Parse(port);
                using (var client = new SshClient(host, sshport, username, password))
                {
                    client.Connect();
                    client.RunCommand("ls");
                    client.Disconnect();
                }
            }
            catch (Renci.SshNet.Common.SshAuthenticationException authEx)
            {
                Program.logger.Log(LogType.Error, $"Authentication failed: {authEx.Message}");
            }
            catch (Renci.SshNet.Common.SshConnectionException connEx)
            {
                Program.logger.Log(LogType.Error, $"Connection failed: {connEx.Message}");

            }
            catch (Exception ex)
            {
                Program.logger.Log(LogType.Error, $"An error occurred: {ex.Message}");
            }
            ConfigManager.UpdateSetting("Daemon", "ssh_ip", host);
            ConfigManager.UpdateSetting("Daemon", "ssh_port", port);
            ConfigManager.UpdateSetting("Daemon", "ssh_username", username);
            ConfigManager.UpdateSetting("Daemon", "ssh_password", password);

            Program.logger.Log(LogType.Info, "Done we saved your ssh connection to your config file");
            Program.Stop();
        }
        catch (Exception ex)
        {
            Program.logger.Log(LogType.Error, $"Failed to connect to the ssh server: {ex.Message}");
            Program.Stop();
        }
    }

    private static string ReadPasswordInput()
    {
        string password = "";
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password = password.Remove(password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        }
        return password;
    }
}