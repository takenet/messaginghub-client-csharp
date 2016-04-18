using System;
using Lime.Protocol;

namespace Takenet.MessagingHub.Client.Connection
{
    /// <summary>
    /// Configure a connection with the Messaging Hub
    /// </summary>
    public interface IMessagingHubConnectionConfigurator<out TConfigurator>
        where TConfigurator : IMessagingHubConnectionConfigurator<TConfigurator>
    {
        /// <summary>
        /// Inform an account to be used to connect to the Messaging Hub
        /// </summary>
        /// <param name="identifier">Account identifier</param>
        /// <param name="accessKey">Account access key</param>
        /// <returns>The same instance of the <see cref="IMessagingHubConnectionConfigurator"/>, configured to use the given account</returns>
        TConfigurator UsingAccessKey(string identifier, string accessKey);

        /// <summary>
        /// Inform an account to be used to connect to the Messaging Hub
        /// </summary>
        /// <param name="identifier">Account identifier</param>
        /// <param name="password">Account password</param>
        /// <returns>The same instance of the <see cref="IMessagingHubConnectionConfigurator"/>, configured to use the given account</returns>
        TConfigurator UsingPassword(string identifier, string password);

        /// <summary>
        /// Overrides the session compression mode
        /// </summary>
        /// <param name="sessionCompression">The desired <see cref="SessionCompression">compression mode</see></param>
        /// <returns>The same instance of the <see cref="IMessagingHubConnectionConfigurator"/>, configured to use the given compression mode</returns>
        TConfigurator UsingCompression(SessionCompression sessionCompression);

        /// <summary>
        /// Overrides the default domain with the given one
        /// </summary>
        /// <param name="domain">The domain to be used</param>
        /// <returns>The same instance of the <see cref="IMessagingHubConnectionConfigurator"/>, configured to use the given domain</returns>
        TConfigurator UsingDomain(string domain);

        /// <summary>
        /// Overrides the session encryption mode
        /// </summary>
        /// <param name="sessionEncryption">The desired <see cref="SessionEncryption">encryption mode</see></param>
        /// <returns>The same instance of the <see cref="IMessagingHubConnectionConfigurator"/>, configured to use the given encryption mode</returns>
        TConfigurator UsingEncryption(SessionEncryption sessionEncryption);

        /// <summary>
        /// Use the guest account to connect to the Messaging Hub
        /// </summary>
        /// <returns>The same instance of the <see cref="IMessagingHubConnectionConfigurator"/>, configured to use the guest account</returns>
        TConfigurator UsingGuest();

        /// <summary>
        /// Overrides the default host name with the given one. It can be used to connect to an alternative instance of the Messaging Hub or another Lime endpoint
        /// </summary>
        /// <param name="hostName">The address of the host, without the protocol prefix and port number sufix</param>
        /// <returns>The same instance of the <see cref="IMessagingHubConnectionConfigurator"/>, configured to use the given host name</returns>
        TConfigurator UsingHostName(string hostName);

        /// <summary>
        /// Overrides the default <see cref="IMessagingHubConnection.MaxConnectionRetries">maximum connection retries</see>
        /// </summary>
        /// <param name="maxConnectionRetries">The maximum number of connection retries. The number must be at least 1 and at most 5</param>
        /// <returns>The same instance of the <see cref="IMessagingHubConnectionConfigurator"/>, configured to use the given maximum connection retries</returns>
        TConfigurator WithMaxConnectionRetries(int maxConnectionRetries);

        /// <summary>
        /// Overrides the default <see cref="IMessagingHubConnection.SendTimeout">send timeout</see>
        /// </summary>
        /// <param name="timeout">A timespan representing the desired send timeout</param>
        /// <returns>The same instance of the <see cref="IMessagingHubConnectionConfigurator"/>, configured to use the given send timeout</returns>
        TConfigurator WithSendTimeout(TimeSpan timeout);
    }
}