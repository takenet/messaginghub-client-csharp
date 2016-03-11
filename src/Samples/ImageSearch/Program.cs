using Bing;
using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Textc;
using Takenet.Textc;
using Takenet.Textc.Csdl;
using Takenet.Textc.Scorers;

namespace ImageSearch
{
    class Program
    { 

        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var factory = typeof (TextcMessageReceiverFactory);
            
            // Starts the client
            var stoppable = await Bootstrapper.StartAsync();

            Console.WriteLine("Press any key to stop");
            Console.ReadKey();

            // Stop the client
            await stoppable.StopAsync();
        }
    }
}
