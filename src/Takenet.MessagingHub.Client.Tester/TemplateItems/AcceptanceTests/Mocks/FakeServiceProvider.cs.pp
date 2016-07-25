using $rootnamespace$.AcceptanceTests.Base;
using Takenet.MessagingHub.Client.Extensions.Bucket;

namespace $rootnamespace$.AcceptanceTests.Mocks
{
    public class FakeServiceProvider : TestServiceProvider
    {
        static FakeServiceProvider()
        {
            RegisterTestService<IBucketExtension, FakeBucketExtension>();
        }
    }
}