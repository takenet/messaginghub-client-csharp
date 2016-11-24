using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Listener;
using Topshelf;

namespace Takenet.MessagingHub.Client.Host
{
    /// <summary>
    /// Host utility program for Messaging Hub clients.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            try
            {
                if (HasArgument(args, "help"))
                {
                    WriteLine("Messaging Hub client host");
                    WriteLine();
                    WriteLine("Usage: mhh <path> [-help] [-pause] [-service]");
                    WriteLine();
                    WriteLine("- path: The path of the application host JSON file");
                    WriteLine("- help: Show this help");
                    WriteLine("- service: Run as a Windows Service");
                    WriteLine(
                        "- pause: Indicates if the application should pause and wait for user input after the execution");
                    WriteLine();
                    return;
                }

                if (HasArgument(args, "trace"))
                {
                    Trace.Listeners.Add(new ConsoleTraceListener());
                }

                if (HasArgument(args, "serviceName") ||
                    HasArgument(args, "displayName") ||
                    HasArgument(args, "serviceName"))
                {
                    var serviceName = GetArgumentValue(args, "serviceName");
                    if (serviceName == null)
                    {
                        WriteLine("The 'name' argument is mandatory", ConsoleColor.Red);
                        return;
                    }
                    var serviceDisplayName = GetArgumentValue(args, "displayName") ?? "";
                    var serviceDescription = GetArgumentValue(args, "description") ?? "";

                    var commandLine = string.Empty;
                    if (HasArgument(args, "install"))
                    {
                        commandLine = "install";
                    }
                    else if (HasArgument(args, "uninstall"))
                    {
                        commandLine = "uninstall";
                    }

                    HostFactory.Run(configurator =>
                    {
                        configurator.ApplyCommandLine(commandLine);
                        configurator.SetServiceName(serviceName); //cannot contain spaces or / or \
                            configurator.SetDisplayName(serviceDisplayName);
                        configurator.SetDescription(serviceDescription);
                        configurator.StartAutomatically();
                        configurator.Service<HostService>();
                    });
                }
                else
                {
                    var applicationFileName = GetApplicationFileName(args);
                    if (!File.Exists(applicationFileName))
                    {
                        WriteLine($"Could not find the {applicationFileName} file", ConsoleColor.Red);
                        return;
                    }

                    await Run(applicationFileName);
                }

            }
            catch (Exception ex)
            {
                WriteLine("Application failed:");
                WriteLine(ex.ToString(), ConsoleColor.Red);
            }
            finally
            {
                if (HasArgument(args, "pause"))
                {
                    WriteLine("Press any key to exit.");
                    Console.ReadKey(true);
                }
            }

        }

        private static async Task Run(string applicationFileName)
        {
            const ConsoleColor highlightColor = ConsoleColor.DarkCyan;

            WriteLine("Starting application...", highlightColor);

            var stopabble = await StartAsync(applicationFileName);

            WriteLine("Application started. Press any key to stop.", highlightColor);
            Console.Read();
            WriteLine("Stopping application...", highlightColor);
            await stopabble.StopAsync();
            WriteLine("Application stopped.", highlightColor);
        }


        public static async Task<IStoppable> StartAsync(string applicationFileName)
        {
            ConfigureWorkingDirectory(applicationFileName);
            var application = Application.ParseFromJsonFile(applicationFileName);
            var stopabble = await Bootstrapper.StartAsync(application);
            return stopabble;
        }

        public static string GetApplicationFileName(string[] args)
        {
            var applicationFileName =
                args.FirstOrDefault(a => !a.StartsWith("-") && !a.StartsWith("/")) ??
                Bootstrapper.DefaultApplicationFileName;
            return applicationFileName;
        }

        private static bool HasArgument(string[] args, string argumentName)
        {
            return args.Any(a => a.TrimStart('-', '/').Equals(argumentName, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetArgumentValue(string[] args, string argumentName)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].TrimStart('-', '/').Equals(argumentName, StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length >= i + 1) return args[i + 1];
                    return null;
                }
            }

            return null;
        }

        private static void ConfigureWorkingDirectory(string applicationFileName)
        {
            var path = Path.GetDirectoryName(applicationFileName);
            if (!string.IsNullOrWhiteSpace(path))
            {
                Directory.SetCurrentDirectory(path);
            }
            else
            {
                path = Environment.CurrentDirectory;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                var assemblyName = new AssemblyName(eventArgs.Name);
                var filePath = Path.Combine(path, $"{assemblyName.Name}.dll");
                return File.Exists(filePath) ? Assembly.LoadFile(filePath) : null;
            };
        }

        static void WriteLine(string value = "", ConsoleColor color = ConsoleColor.White)
        {
            var foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = foregroundColor;
            Console.Out.Flush();
        }
    }
}
