using System;

namespace Takenet.MessagingHub.Client.Tester
{
    public class ApplicationTesterOptions
    {
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

        public ApplicationTesterOptions Clone()
        {
            return new ApplicationTesterOptions
            {
                DefaultTimeout = DefaultTimeout,
                EnableConsoleListener = EnableConsoleListener,
                UseErrorStream = UseErrorStream,
                UseSeparateTestingAccount = UseSeparateTestingAccount,
                TesterAccountIndex = TesterAccountIndex,
                TestServiceProviderType = TestServiceProviderType,
                InstantiateApplication = InstantiateApplication
            };
        }
    }
}