using Takenet.MessagingHub.Client.Tester;

namespace $rootnamespace$.AcceptanceTests.Base
{
    public class TestServiceProvider : ApplicationTesterServiceProvider
    {
        public static void RegisterTestService<TInterface, TClass>()
            where TInterface : class
            where TClass : class, TInterface
        {
            // Appication Service Provider
            ((ServiceProvider)ApplicationTester.ApplicationServiceProvider).Container.RegisterSingleton<TInterface, TClass>();
        }
    }
}
