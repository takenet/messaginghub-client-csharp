using Lime.Protocol;
using System;
using Lime.Messaging.Resources;

namespace Takenet.MessagingHub.Client.Connection
{
    public class MessagingHubConnectionConfigurator<TConfigurator> : IMessagingHubConnectionConfigurator<TConfigurator>
        where TConfigurator : MessagingHubConnectionConfigurator<TConfigurator>
    {
        public const string DEFAULT_DOMAIN = "msging.net";

        protected string Identifier { get; private set; }
        protected string Instance { get; private set; }

        protected string Password { get; private set; }
        protected string AccessKey { get; private set; }
        protected TimeSpan SendTimeout { get; private set; }
        protected int MaxConnectionRetries { get; private set; }
        protected string Domain { get; private set; }
        protected string HostName { get; private set; }
        protected SessionCompression Compression { get; private set; }
        protected SessionEncryption Encryption { get; private set; }
        protected RoutingRule RoutingRule { get; private set; }
        protected TimeSpan Throughput { get; private set; }

        protected Identity Identity => Identity.Parse($"{Identifier}@{Domain}");
        protected Uri EndPoint => new Uri($"net.tcp://{HostName}:55321");

        public MessagingHubConnectionConfigurator()
        {
            HostName = DEFAULT_DOMAIN;
            Domain = DEFAULT_DOMAIN;
            SendTimeout = TimeSpan.FromSeconds(20);
            MaxConnectionRetries = 3;
            Compression = SessionCompression.None;
            Encryption = SessionEncryption.TLS;
            RoutingRule = RoutingRule.Identity;
        }

        public TConfigurator UsingPassword(string identifier, string password)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ArgumentNullException(nameof(identifier));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            Identifier = identifier;
            Password = password;

            return (TConfigurator)this;
        }

        public TConfigurator UsingGuest()
        {
            Identifier = Guid.NewGuid().ToString();
            return (TConfigurator)this;
        }

        public TConfigurator UsingAccessKey(string identifier, string accessKey)
        {
            if (string.IsNullOrEmpty(identifier)) throw new ArgumentNullException(nameof(identifier));
            if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException(nameof(accessKey));

            Identifier = identifier;
            AccessKey = accessKey;

            return (TConfigurator)this;
        }

        public TConfigurator UsingInstance(string instance)
        {
            Instance = instance;
            return (TConfigurator)this;
        }

        public TConfigurator UsingRoutingRule(RoutingRule routingRule)
        {
            RoutingRule = routingRule;
            return (TConfigurator)this;
        }

        public TConfigurator UsingHostName(string hostName)
        {
            if (string.IsNullOrEmpty(hostName)) throw new ArgumentNullException(nameof(hostName));

            HostName = hostName;
            return (TConfigurator)this;
        }

        public TConfigurator UsingDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));

            Domain = domain;
            return (TConfigurator)this;
        }

        public TConfigurator UsingEncryption(SessionEncryption sessionEncryption)
        {
            Encryption = sessionEncryption;
            return (TConfigurator)this;
        }

        public TConfigurator UsingCompression(SessionCompression sessionCompression)
        {
            Compression = sessionCompression;
            return (TConfigurator)this;
        }

        public TConfigurator WithSendTimeout(TimeSpan timeout)
        {
            SendTimeout = timeout;
            return (TConfigurator)this;
        }

        public TConfigurator WithMaxConnectionRetries(int maxConnectionRetries)
        {
            if (maxConnectionRetries < 1) throw new ArgumentOutOfRangeException(nameof(maxConnectionRetries));
            if (maxConnectionRetries > 5) throw new ArgumentOutOfRangeException(nameof(maxConnectionRetries));

            MaxConnectionRetries = maxConnectionRetries;
            return (TConfigurator)this;
        }

        public TConfigurator WithThroughput(TimeSpan throughput)
        {
            Throughput = throughput;
            return (TConfigurator)this;
        }
    }
}
