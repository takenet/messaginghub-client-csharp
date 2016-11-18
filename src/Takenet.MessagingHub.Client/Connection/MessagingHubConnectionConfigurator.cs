using Lime.Protocol;
using System;
using Lime.Messaging.Resources;

namespace Takenet.MessagingHub.Client.Connection
{
    public class MessagingHubConnectionConfigurator<TConfigurator> : IMessagingHubConnectionConfigurator<TConfigurator>
        where TConfigurator : MessagingHubConnectionConfigurator<TConfigurator>
    {
        protected string Identifier { get; private set; }
        protected string Instance { get; private set; }

        protected string Password { get; private set; }
        protected string AccessKey { get; private set; }
        protected TimeSpan SendTimeout { get; private set; }
        protected int MaxConnectionRetries { get; private set; }
        protected string Domain { get; private set; }
        protected string Scheme { get; private set; }
        protected string HostName { get; private set; }
        protected int Port { get; private set; }        
        protected SessionCompression Compression { get; private set; }
        protected SessionEncryption Encryption { get; private set; }
        protected RoutingRule RoutingRule { get; private set; }
        protected int Throughput { get; private set; }
        protected bool AutoNotify { get; private set; }

        protected Identity Identity => Identity.Parse($"{Identifier}@{Domain}");
        protected Uri EndPoint => new Uri($"{Scheme}://{HostName}:{Port}");

        public MessagingHubConnectionConfigurator()
        {
            Domain = Constants.DEFAULT_DOMAIN;
            Scheme = Constants.DEFAULT_SCHEME;
            HostName = Constants.DEFAULT_DOMAIN;
            Port = Constants.DEFAULT_PORT;
            SendTimeout = TimeSpan.FromSeconds(60);
            MaxConnectionRetries = 3;
            Compression = SessionCompression.None;
            Encryption = SessionEncryption.TLS;
            RoutingRule = RoutingRule.Identity;
            AutoNotify = true;
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

        public TConfigurator UsingScheme(string scheme)
        {
            if (string.IsNullOrEmpty(scheme)) throw new ArgumentNullException(nameof(scheme));
            Scheme = scheme;
            return (TConfigurator)this;
        }

        public TConfigurator UsingHostName(string hostName)
        {
            if (string.IsNullOrEmpty(hostName)) throw new ArgumentNullException(nameof(hostName));

            HostName = hostName;
            return (TConfigurator)this;
        }

        public TConfigurator UsingPort(int port)
        {
            if (port <= 0) throw new ArgumentOutOfRangeException(nameof(port));

            Port = port;
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

        public TConfigurator WithAutoNotify(bool enabled)
        {
            AutoNotify = enabled;
            return (TConfigurator)this;
        }

        public TConfigurator WithMaxConnectionRetries(int maxConnectionRetries)
        {
            if (maxConnectionRetries < 1) throw new ArgumentOutOfRangeException(nameof(maxConnectionRetries));
            if (maxConnectionRetries > 5) throw new ArgumentOutOfRangeException(nameof(maxConnectionRetries));

            MaxConnectionRetries = maxConnectionRetries;
            return (TConfigurator)this;
        }

        public TConfigurator WithThroughput(int throughput)
        {
            Throughput = throughput;
            return (TConfigurator)this;
        }
    }
}
