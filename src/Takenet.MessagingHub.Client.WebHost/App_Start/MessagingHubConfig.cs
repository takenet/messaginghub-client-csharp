using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Host;

namespace Takenet.MessagingHub.Client.WebHost
{
    public static class MessagingHubConfig
    {
        public static async Task<IServiceContainer> StartAsync()
        {
            var applicationFileName = Bootstrapper.DefaultApplicationFileName;
            var application = Application.ParseFromJsonFile(Path.Combine(GetAssemblyRoot(), applicationFileName));

            var localServiceProvider = Bootstrapper.BuildServiceProvider(application);

            localServiceProvider.RegisterService(typeof(IServiceProvider), localServiceProvider);
            localServiceProvider.RegisterService(typeof(IServiceContainer), localServiceProvider);
            localServiceProvider.RegisterService(typeof(Application), application);
            Bootstrapper.RegisterSettingsContainer(application, localServiceProvider);

            var envelopeBuffer = new EnvelopeBuffer();
            localServiceProvider.RegisterService(typeof(IEnvelopeBuffer), envelopeBuffer);
            var client = await Bootstrapper.BuildMessagingHubClientAsync(application, () => new MessagingHubClient(new HttpMessagingHubConnection(envelopeBuffer)), localServiceProvider);

            await client.StartAsync().ConfigureAwait(false);

            await Bootstrapper.BuildStartupAsync(application, localServiceProvider);

            return localServiceProvider;
        }

        private static string GetAssemblyRoot()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}