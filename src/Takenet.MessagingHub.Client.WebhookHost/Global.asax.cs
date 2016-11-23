using System.Web.Http;

namespace Takenet.MessagingHub.Client.WebhookHost
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var serviceContainer = MessagingHubConfig.StartAsync().Result;
            GlobalConfiguration.Configuration.DependencyResolver = new MessagingHubClientResolver(serviceContainer);
        }
    }
}
