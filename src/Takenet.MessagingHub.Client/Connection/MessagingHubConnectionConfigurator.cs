using Lime.Protocol;
using System;

namespace Takenet.MessagingHub.Client.Connection
{
    public class MessagingHubConnectionConfigurator<TConfigurator> : IMessagingHubConnectionConfigurator<TConfigurator>
        where TConfigurator : MessagingHubConnectionConfigurator<TConfigurator>
    {
        public const string DEFAULT_DOMAIN = "msging.net";

        protected string Account { get; private set; }
        protected string Password { get; private set; }
        protected string AccessKey { get; private set; }
        protected TimeSpan SendTimeout { get; private set; }
        protected int MaxConnectionRetries { get; private set; }
        protected string Domain { get; private set; }
        protected string HostName { get; private set; }
        protected SessionCompression Compression { get; private set; }
        protected SessionEncryption Encryption { get; private set; }

        protected Identity Identity => Identity.Parse($"{Account}@{Domain}");
        protected Uri EndPoint => new Uri($"net.tcp://{HostName}:55321");

        public MessagingHubConnectionConfigurator()
        {
            HostName = DEFAULT_DOMAIN;
            Domain = DEFAULT_DOMAIN;
            SendTimeout = TimeSpan.FromSeconds(20);
            MaxConnectionRetries = 3;
            Compression = SessionCompression.None;
            Encryption = SessionEncryption.TLS;
        }

        public TConfigurator UsingAccount(string account, string password)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentNullException(nameof(account));
            if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));

            Account = account;
            Password = password;

            return (TConfigurator)this;
        }

        public TConfigurator UsingGuest()
        {
            Account = Guid.NewGuid().ToString();
            return (TConfigurator)this;
        }

        public TConfigurator UsingAccessKey(string account, string accessKey)
        {
            if (string.IsNullOrEmpty(account)) throw new ArgumentNullException(nameof(account));
            if (string.IsNullOrEmpty(accessKey)) throw new ArgumentNullException(nameof(accessKey));

            Account = account;
            AccessKey = accessKey;

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
    }
}
