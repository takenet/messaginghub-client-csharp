using System.Web.Http;
using Lime.Protocol;
using Takenet.MessagingHub.Client.WebhookHost.Models;

namespace Takenet.MessagingHub.Client.WebhookHost
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.BindParameter(typeof(Message), new EnvelopeModelBinder());
            config.BindParameter(typeof(Notification), new EnvelopeModelBinder());
        }
    }
}
