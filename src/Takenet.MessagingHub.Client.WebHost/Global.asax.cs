using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Takenet.MessagingHub.Client.WebHost.Controllers;

namespace Takenet.MessagingHub.Client.WebHost
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var serviceContainer = MessagingHubConfig.StartAsync().Result;
            serviceContainer.RegisterService(
                typeof(EnvelopeController), 
                () => new EnvelopeController(serviceContainer.GetService(typeof(IEnvelopeBuffer)) as IEnvelopeBuffer));
            GlobalConfiguration.Configuration.DependencyResolver = new MessagingHubClientResolver(serviceContainer);
        }
    }
}
