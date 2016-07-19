using System;

namespace Takenet.MessagingHub.Client.Tester
{
    public class ApplicationTesterOptions
    {
        /// <summary>
        /// Default timeout for sending evenlopes using the tester account
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; }
        /// <summary>
        /// Enable writing tracing information to the console output (Default value: false)
        /// </summary>
        public bool EnableConsoleListener { get; set; }
        /// <summary>
        /// True to write debugging information to the error stream. False to write it to the default output (Default value: false)
        /// </summary>
        public bool UseErrorStream { get; set; }
        /// <summary>
        /// Create a separate account to be used by your application during the tests (Default value: true)
        /// </summary>
        /// <returns>
        /// </returns>
        /// <remarks>
        /// Extensions associated with your application will not work with this separate testing account
        /// </remarks>
        public bool UseSeparateTestingAccount { get; set; } = true;
        /// <summary>
        /// Allow the testing account to send messages in the name of the application and vice versa (Default value: false)
        /// </summary>
        public bool EnableMutualDelegation { get; set; }
        /// <summary>
        /// Index included in the testing account
        /// </summary>
        public int TesterAccountIndex { get; set; }
        /// <summary>
        /// ServiceProvider to be used during the tests. It can be used to mock dependencies used by your application
        /// </summary>
        public Type TestServiceProviderType { get; set; }
        /// <summary>
        /// Instanciate your application when the <see cref="ApplicationTester"/> is instantiated
        /// </summary>
        public bool InstantiateApplication { get; set; } = true;

        /// <summary>
        /// Inform the default name, or name prefix, to use as tester account (Default value: {ApplicationAccountName}$tester)
        /// </summary>
        public string TesterAccountName { get; set; }

        public ApplicationTesterOptions Clone()
        {
            return new ApplicationTesterOptions
            {
                DefaultTimeout = DefaultTimeout,
                EnableConsoleListener = EnableConsoleListener,
                UseErrorStream = UseErrorStream,
                UseSeparateTestingAccount = UseSeparateTestingAccount,
                EnableMutualDelegation = EnableMutualDelegation,
                TesterAccountIndex = TesterAccountIndex,
                TestServiceProviderType = TestServiceProviderType,
                InstantiateApplication = InstantiateApplication,
                TesterAccountName = TesterAccountName
            };
        }
    }
}