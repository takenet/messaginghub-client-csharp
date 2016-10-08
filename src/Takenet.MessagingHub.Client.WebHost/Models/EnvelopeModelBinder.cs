using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using System;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Takenet.MessagingHub.Client.WebHost.Models
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