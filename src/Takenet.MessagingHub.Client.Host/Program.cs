using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;

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
                if (args.Any(a => a.TrimStart('-', '/').Equals("help", StringComparison.OrdinalIgnoreCase)))
                {
                    WriteLine("Messaging Hub client host");
                    WriteLine();
                    WriteLine("Usage: mhh <path> [-help] [-pause]");
                    WriteLine();
                    WriteLine("- path: The path of the application host JSON file");
                    WriteLine("- help: Show this help");
                    WriteLine(
                        "- pause: Indicates if the application should pause and wait for user input after the execution");
                    WriteLine();
                    return;
                }

                var applicationFileName =
                    args.FirstOrDefault(a => !a.StartsWith("-") && !a.StartsWith("/")) ??
                    ApplicationActivator.DefaultApplicationFileName;

                if (!File.Exists(applicationFileName))
                {
                    WriteLine($"Could not find the {applicationFileName} file", ConsoleColor.Red);
                    return;
                }

                const ConsoleColor highlightColor = ConsoleColor.DarkCyan;

                WriteLine("Starting application...", highlightColor);
                ConfigureWorkingDirectory(applicationFileName);
                var application = Application.ParseFromJsonFile(applicationFileName);
                var stopabble = await Bootstrapper.StartAsync(application);
                WriteLine("Application started. Press any key to stop.", highlightColor);
                Console.Read();
                WriteLine("Stopping application...", highlightColor);
                await stopabble.StopAsync();
                WriteLine("Application stopped.", highlightColor);
            }
            catch (Exception ex)
            {
                WriteLine("Application failed:");
                WriteLine(ex.ToString(), ConsoleColor.Red);
            }
            finally
            {
                if (args.Any(a => a.TrimStart('-', '/').Equals("pause", StringComparison.OrdinalIgnoreCase)))
                {
                    WriteLine("Press any key to exit.");
                    Console.ReadKey(true);
                }
            }            
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
