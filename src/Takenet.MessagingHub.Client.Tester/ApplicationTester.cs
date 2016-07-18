using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Newtonsoft.Json;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace Takenet.MessagingHub.Client.Tester
{
    public sealed class ApplicationTester : IDisposable
    {
        private readonly ApplicationTesterOptions _options;
        private static ConsoleTraceListener _listener;

        /// <summary>
        /// 
        /// </summary>
        public static IServiceProvider ApplicationServiceProvider { get; private set; }

        private ConcurrentQueue<Message> _lattestMessages;
        private TimeSpan DefaultTimeout { get; set; }

        private IMessagingHubClient Tester { get; set; }
        public IStoppable Application { get; private set; }

        private ConcurrentQueue<Message> LattestMessages
        {
            get { return _lattestMessages; }
            set { _lattestMessages = value; }
        }


        private string TestingIdentifier { get; set; }
        private string TestingPassword { get; set; }

        private string TesterIdentifier { get; set; }
        private string TesterPassword { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Application ApplicationConfig { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="applicationTester"></param>
        public ApplicationTester(ApplicationTesterOptions options)
        {
            _options = options;
            StartAsync().Wait();
        }


        private async Task StartAsync()
        {
            ApplyOptions();
            LoadApplicationSettings();
            await CreateTestingAccountsAsync();
            PatchApplication();
            DiscardReceivedMessages();
            if (_options.InstantiateApplication)
            {
                await StartApplicationAsync();
            }
            InstantiateTestClient();
            RegisterTestClientMessageReceivers();
            await StartTestClientAsync();
        }

        private void ApplyOptions()
        {
            DefaultTimeout = _options.DefaultTimeout == default(TimeSpan) ? TimeSpan.FromSeconds(30) : _options.DefaultTimeout;

            if (_options.EnableConsoleListener)
                EnableConsoleTraceListener(_options.UseErrorStream);
        }

        private async Task CreateTestingAccountsAsync()
        {
            if (_options.UseSeparateTestingAccount)
            {
                //TODO: Testing account should be a total clone of the application account

                var testingAccountManager = new TestingAccountManager(ApplicationConfig, DefaultTimeout);

                TesterPassword = TestingPassword = (ApplicationConfig.AccessKey ?? ApplicationConfig.Password).FromBase64();

                TestingIdentifier = ApplicationConfig.Identifier + "$testing";
                await testingAccountManager.CreateAccountAsync(TestingIdentifier, TestingPassword);

                TesterIdentifier = ApplicationConfig.Identifier + "$tester";
                if (_options.TesterAccountIndex > 0)
                {
                    TesterIdentifier = $"{TesterIdentifier}{_options.TesterAccountIndex}";
                }
                await testingAccountManager.CreateAccountAsync(TesterIdentifier, TestingPassword);
            }
            else
            {
                TesterIdentifier = ApplicationConfig.Identifier;
                TesterPassword = ApplicationConfig.AccessKey;
            }
        }

        private void PatchApplication()
        {
            ApplicationConfig.Instance = Guid.NewGuid().ToString();
            ApplicationConfig.Identifier = TestingIdentifier;
            ApplicationConfig.Password = TestingPassword;
            ApplicationConfig.AccessKey = null;

            if (ApplicationConfig.ServiceProviderType != null)
            {
                ValidateApplicationServiceProviderType(ApplicationConfig.ServiceProviderType);
                var applicationServiceProviderType = ParseTypeName(ApplicationConfig.ServiceProviderType);

                ApplicationServiceProvider = ApplicationServiceProvider ?? (IServiceProvider)Activator.CreateInstance(applicationServiceProviderType);
            }

            if (_options.TestServiceProviderType != null)
            {
                ValidateTestServiceProviderType(_options.TestServiceProviderType);
                ApplicationConfig.ServiceProviderType = _options.TestServiceProviderType.Name;
            }
        }

        private static void ValidateApplicationServiceProviderType(string applicationServiceProviderTypeName)
        {
            var serviceProviderInterfaceType = typeof(IServiceProvider);
            var applicationServiceProviderType = ParseTypeName(applicationServiceProviderTypeName);
            if (!serviceProviderInterfaceType.IsAssignableFrom(applicationServiceProviderType))
                throw new ArgumentException(
                    $"{applicationServiceProviderTypeName} must be a subtype of {serviceProviderInterfaceType.FullName}");
        }

        private static void ValidateTestServiceProviderType(Type testServiceProviderType)
        {
            var baseTestServiceProviderType = typeof(IServiceProvider);
            if (!baseTestServiceProviderType.IsAssignableFrom(testServiceProviderType))
                throw new ArgumentException(
                    $"{testServiceProviderType.Name} must be a subtype of {baseTestServiceProviderType.FullName}");
        }

        private static void EnableConsoleTraceListener(bool useErrorStream)
        {
            _listener = new ConsoleTraceListener(useErrorStream);
            Trace.Listeners.Add(_listener);
        }

        [Obsolete("Must use the same method exposed by the TypeUtil")]
        private static Type ParseTypeName(string typeName)
        {
            return TypeUtil
                .GetAllLoadedTypes()
                .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) ??
                       Type.GetType(typeName, true, true);
        }

        private void DiscardReceivedMessages()
        {
            _lattestMessages = new ConcurrentQueue<Message>();
        }

        private async Task StopSmartContactAsync()
        {
            await Application.StopAsync();
        }

        private async Task StartTestClientAsync()
        {
            await Tester.StartAsync();
            if (_options.EnableMutualDelegation)
            {
                var domain = ApplicationConfig.Domain ?? "msging.net";
                var testerIdentity = Identity.Parse($"{TesterIdentifier}@{domain}");
                var delegationExtension = new DelegationExtension(GetService<IMessagingHubSender>());
                await delegationExtension.DelegateAsync(testerIdentity);
            }
        }

        private async Task StopTestClientAsync()
        {
            await Tester.StopAsync();
        }

        private async Task StartApplicationAsync()
        {
            Application = await Bootstrapper.StartAsync(ApplicationConfig);
            if (_options.EnableMutualDelegation)
            {
                var domain = ApplicationConfig.Domain ?? "msging.net";
                var testingIdentity = Identity.Parse($"{TestingIdentifier}@{domain}");
                var delegationExtension = new DelegationExtension(Tester);
                await delegationExtension.DelegateAsync(testingIdentity);
            }
        }

        private void LoadApplicationSettings()
        {
            var assemblyFile = Assembly.GetExecutingAssembly().Location;
            var assemblyDir = new FileInfo(assemblyFile).DirectoryName;
            var appJsonFullName = $"{assemblyDir}\\application.json";
            LoadApplicationJson(appJsonFullName);
            ConfigureWorkingDirectory(appJsonFullName);
        }

        [Obsolete("Bootstrapper should do this automatically")]
        private static void ConfigureWorkingDirectory(string applicationFileName)
        {
            var path = Path.GetDirectoryName(applicationFileName);
            if (!string.IsNullOrWhiteSpace(path))
            {
                Directory.SetCurrentDirectory(path);
            }
            else
            {
                path = Environment.CurrentDirectory;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                var assemblyName = new AssemblyName(eventArgs.Name);
                var filePath = Path.Combine(path, $"{assemblyName.Name}.dll");
                return File.Exists(filePath) ? Assembly.LoadFile(filePath) : null;
            };
        }

        private void InstantiateTestClient()
        {
            var builder = new MessagingHubClientBuilder()
                .UsingPassword(TesterIdentifier, TesterPassword)
                .WithSendTimeout(DefaultTimeout)
                .WithMaxConnectionRetries(1);

            if (ApplicationConfig.SessionEncryption.HasValue)
                builder = builder.UsingEncryption(ApplicationConfig.SessionEncryption.Value);

            if (ApplicationConfig.SessionCompression.HasValue)
                builder = builder.UsingCompression(ApplicationConfig.SessionCompression.Value);

            if (!string.IsNullOrWhiteSpace(ApplicationConfig.HostName))
                builder = builder.UsingHostName(ApplicationConfig.HostName);

            if (!string.IsNullOrWhiteSpace(ApplicationConfig.Domain))
                builder = builder.UsingDomain(ApplicationConfig.Domain);

            Tester = builder.Build();
        }

        private void RegisterTestClientMessageReceivers()
        {
            Tester.AddMessageReceiver((m, c) =>
            {
                LattestMessages.Enqueue(m);
                return Task.CompletedTask;
            });
        }

        private void LoadApplicationJson(string appJson)
        {
            var appJsonContent = File.ReadAllText(appJson);
            ApplicationConfig = JsonConvert.DeserializeObject<Application>(appJsonContent);
        }

        private async Task StopAsync()
        {
            if (_options.InstantiateApplication)
            {
                await StopSmartContactAsync();
            }
            await StopTestClientAsync();
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _listener?.Dispose();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static async Task SleepAsync(int seconds = 1)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message)
        {
            await Tester.SendMessageAsync(message, ApplicationConfig.Identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(Document message)
        {
            await Tester.SendMessageAsync(message, ApplicationConfig.Identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<Message> ReceiveMessageAsync(TimeSpan timeout = default(TimeSpan))
        {
            try
            {
                timeout = timeout == default(TimeSpan) ? DefaultTimeout : timeout;
                using (var cts = new CancellationTokenSource(timeout))
                {
                    while (!cts.IsCancellationRequested)
                    {
                        Message lastMessage;
                        if (LattestMessages.TryDequeue(out lastMessage))
                            return lastMessage;
                        await Task.Delay(10, cts.Token);
                    }
                    return null;
                }
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task IgnoreMessageAsync(TimeSpan timeout = default(TimeSpan))
        {
            try
            {
                timeout = timeout == default(TimeSpan) ? DefaultTimeout : timeout;
                using (var cts = new CancellationTokenSource(timeout))
                {
                    while (!cts.IsCancellationRequested)
                    {
                        Message lastMessage;
                        LattestMessages.TryDequeue(out lastMessage);
                        if (lastMessage != null)
                            break;
                        await Task.Delay(10, cts.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>()
        {
            return (T)ApplicationServiceProvider.GetService(typeof(T));
        }
    }
}
