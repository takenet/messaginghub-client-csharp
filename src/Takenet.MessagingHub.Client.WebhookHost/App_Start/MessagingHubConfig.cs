using System;
using System.Configuration;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Serialization.Newtonsoft;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.WebhookHost.Controllers;

namespace Takenet.MessagingHub.Client.WebhookHost
{
    public static class MessagingHubConfig
    {
        public static async Task<IServiceContainer> StartAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var applicationFileName = Bootstrapper.DefaultApplicationFileName;
            var application = Application.ParseFromJsonFile(Path.Combine(GetAssemblyRoot(), applicationFileName));
            ApplyConfigurationOverrides(application);

            var typeResolver = new TypeResolver();
            var localServiceProvider = Bootstrapper.BuildServiceProvider(application, typeResolver);

            localServiceProvider.RegisterService(typeof(IServiceProvider), localServiceProvider);
            localServiceProvider.RegisterService(typeof(IServiceContainer), localServiceProvider);
            localServiceProvider.RegisterService(typeof(Application), application);
            Bootstrapper.RegisterSettingsContainer(application, localServiceProvider, typeResolver);

            var envelopeBuffer = new EnvelopeBuffer();
            localServiceProvider.RegisterService(typeof(IEnvelopeBuffer), envelopeBuffer);
            localServiceProvider.RegisterService(typeof(EnvelopeController), () => new EnvelopeController(envelopeBuffer));

            var client = await Bootstrapper.BuildClientAsync(application, () => new MessagingHubClient(new HttpOnDemandClientChannel(envelopeBuffer, new JsonNetSerializer(), application)), localServiceProvider, typeResolver, cancellationToken);
            var listener = new MessagingHubListener(client, !application.DisableNotify);
            await client.StartAsync(listener, cancellationToken).ConfigureAwait(false);

            await Bootstrapper.BuildStartupAsync(application, localServiceProvider, typeResolver);

            return localServiceProvider;
        }

        private static void ApplyConfigurationOverrides(Application application)
        {
            application.Identifier = GetOverride(() => application.Identifier);
            application.AccessKey = GetOverride(() => application.AccessKey);
        }

        private static string GetOverride(Expression<Func<string>> propertyLambda)
        {
            var memberExpression = propertyLambda.Body as MemberExpression;

            if (memberExpression == null) return propertyLambda.Compile().Invoke();
            return ConfigurationManager.AppSettings[$"MessagingHub.{memberExpression.Member.Name}"] ?? propertyLambda.Compile().Invoke();
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