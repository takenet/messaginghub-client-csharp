using System;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Transport.Tcp;

namespace Takenet.MessagingHub.Client.Connection
{
    public class TcpTransportFactory : ITransportFactory
    {
        public ITransport Create(Uri endpoint)
        {
            if (endpoint.Scheme != Uri.UriSchemeNetTcp)
            {
                throw new NotSupportedException($"Unsupported URI scheme '{endpoint.Scheme}'");
            }

            return new TcpTransport(traceWriter: new TraceWriter(), envelopeSerializer: new JsonNetSerializer());
        }
    }
}