using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web.Http;
using Takenet.MessagingHub.Client.WebHost.Models;

namespace $rootnamespace$
{
    public class EnvelopeController : ApiController
    {
        private readonly IEnvelopeBuffer _envelopeBuffer;

        public EnvelopeController(IEnvelopeBuffer envelopeBuffer)
        {
            _envelopeBuffer = envelopeBuffer;
        }

        [Route("messages")]
        public Task Post(Lime.Protocol.Message message)
        {
            return _envelopeBuffer.Messages.SendAsync(message);
        }

        [Route("notifications")]
        public Task Post(Lime.Protocol.Notification notification)
        {
            return _envelopeBuffer.Notifications.SendAsync(notification);
        }
    }
}
