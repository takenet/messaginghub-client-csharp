using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Host;

namespace Takenet.MessagingHub.ConsoleActivator
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var activator = new ApplicationActivator(@"D:\MessagingHubApplications", TimeSpan.FromSeconds(1)))
            {
                Console.WriteLine("Activator started. Press any ket to exit.");
                Console.Read();
            }
            Console.WriteLine("Activator stopped. Press any key to exit.");
            Console.Read();
        }
    }
}
