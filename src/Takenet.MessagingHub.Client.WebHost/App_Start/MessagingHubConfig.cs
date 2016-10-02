using System;
using System.Threading.Tasks;
using System.Web;
using Takenet.MessagingHub.Client.Host;

namespace Takenet.MessagingHub.Client.WebHost
{
    public static class MessagingHubConfig
    {
        public static async Task StartAsync()
        {
            var applicationFileName = Bootstrapper.DefaultApplicationFileName;
            var application = Application.ParseFromJsonFile(applicationFileName);
            var stopabble = await Bootstrapper.StartAsync(application);
        }
    }
}