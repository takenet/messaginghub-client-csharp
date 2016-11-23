using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Lime.Protocol.Serialization.Newtonsoft;

namespace Takenet.MessagingHub.Client.WebhookHost.Models
{
    public class EnvelopeModelBinder : IModelBinder
    {
        private readonly JsonNetSerializer _serializer;

        public EnvelopeModelBinder()
        {
            _serializer = new JsonNetSerializer();
        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var json = actionContext.Request.Content.ReadAsStringAsync().Result;
            bindingContext.Model = _serializer.Deserialize(json);
            return bindingContext.Model != null;        
        }
    }
}