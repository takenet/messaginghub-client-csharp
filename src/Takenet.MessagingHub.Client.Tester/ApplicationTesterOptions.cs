using System;

namespace Takenet.MessagingHub.Client.Tester
{
    public class ApplicationTesterOptions
    {
        public TimeSpan DefaultTimeout;
        public bool EnableConsoleListener;
        public bool UseSeparateTestingAccount = true;
        public int TesterAccountIndex;
        public bool UseErrorStream;
        public Type TestServiceProviderType;

        public ApplicationTesterOptions Clone()
        {
            return new ApplicationTesterOptions
            {
                DefaultTimeout = DefaultTimeout,
                EnableConsoleListener = EnableConsoleListener,
                TestServiceProviderType = TestServiceProviderType,
                UseErrorStream = UseErrorStream,
                UseSeparateTestingAccount = UseSeparateTestingAccount,
                TesterAccountIndex = TesterAccountIndex
            };
        }
    }
}