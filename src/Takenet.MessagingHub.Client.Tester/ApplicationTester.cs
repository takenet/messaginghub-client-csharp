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
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Listener;

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

        private IMessagingHubClient TestClient { get; set; }
        public IStoppable SmartContact { get; private set; }

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
        public Application Application { get; private set; }

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
                await StartSmartContactAsync();
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

                var testingAccountManager = new TestingAccountManager(Application, DefaultTimeout);

                TesterPassword = TestingPassword = (Application.AccessKey ?? Application.Password).FromBase64();

                TestingIdentifier = Application.Identifier + "$testing";
                await testingAccountManager.CreateAccountAsync(TestingIdentifier, TestingPassword);

                TesterIdentifier = Application.Identifier + "$tester";
                if (_options.TesterAccountIndex > 0)
                {
                    TesterIdentifier = $"{TesterIdentifier}{_options.TesterAccountIndex}";
                }
                await testingAccountManager.CreateAccountAsync(TesterIdentifier, TestingPassword);
            }
            else
            {
                TesterIdentifier = Application.Identifier;
                TesterPassword = Application.AccessKey;
            }
        }

        private void PatchApplication()
        {
            Application.Instance = Guid.NewGuid().ToString();
            Application.Identifier = TestingIdentifier;
            Application.Password = TestingPassword;
            Application.AccessKey = null;

            if (Application.ServiceProviderType != null)
            {
                ValidateApplicationServiceProviderType(Application.ServiceProviderType);
                var applicationServiceProviderType = ParseTypeName(Application.ServiceProviderType);

                ApplicationServiceProvider = ApplicationServiceProvider ?? (IServiceProvider)Activator.CreateInstance(applicationServiceProviderType);
            }

            if (_options.TestServiceProviderType != null)
            {
                ValidateTestServiceProviderType(_options.TestServiceProviderType);
                Application.ServiceProviderType = _options.TestServiceProviderType.Name;
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
            await SmartContact.StopAsync();
        }

        private async Task StartTestClientAsync()
        {
            await TestClient.StartAsync();
        }

        private async Task StopTestClientAsync()
        {
            await TestClient.StopAsync();
        }

        private async Task StartSmartContactAsync()
        {
            SmartContact = await Bootstrapper.StartAsync(Application);
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

            if (Application.SessionEncryption.HasValue)
                builder = builder.UsingEncryption(Application.SessionEncryption.Value);

            if (Application.SessionCompression.HasValue)
                builder = builder.UsingCompression(Application.SessionCompression.Value);

            if (!string.IsNullOrWhiteSpace(Application.HostName))
                builder = builder.UsingHostName(Application.HostName);

            if (!string.IsNullOrWhiteSpace(Application.Domain))
                builder = builder.UsingDomain(Application.Domain);

            TestClient = builder.Build();
        }

        private void RegisterTestClientMessageReceivers()
        {
            TestClient.AddMessageReceiver((m, c) =>
            {
                LattestMessages.Enqueue(m);
                return Task.CompletedTask;
            });
        }

        private void LoadApplicationJson(string appJson)
        {
            var appJsonContent = File.ReadAllText(appJson);
            Application = JsonConvert.DeserializeObject<Application>(appJsonContent);
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
            await TestClient.SendMessageAsync(message, Application.Identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(Document message)
        {
            await TestClient.SendMessageAsync(message, Application.Identifier);
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
