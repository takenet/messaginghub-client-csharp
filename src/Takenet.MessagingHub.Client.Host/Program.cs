using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
                    WriteLine("Usage: mhh <path> [-help] [-pause]");
                    WriteLine();
                    WriteLine("- path: The path of the application host JSON file");
                    WriteLine("- help: Show this help");
                    WriteLine(
                        "- pause: Indicates if the application should pause and wait for user input after the execution");
                    WriteLine();
                    return;
                }

                if (HasArgument(args, "trace"))
                {
                    Trace.Listeners.Add(new ConsoleTraceListener());
                }

                var applicationFileName =
                    args.FirstOrDefault(a => !a.StartsWith("-") && !a.StartsWith("/")) ??
                    Bootstrapper.DefaultApplicationFileName;

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
                if (HasArgument(args, "pause"))
                {
                    WriteLine("Press any key to exit.");
                    Console.ReadKey(true);
                }
            }            
        }

        private static bool HasArgument(string[] args, string argumentName)
        {
            return args.Any(a => a.TrimStart('-', '/').Equals(argumentName, StringComparison.OrdinalIgnoreCase));
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
