using Lime.Protocol;
using Lime.Protocol.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Listener;

namespace Takenet.MessagingHub.Client.Host
{
    public class DomainManager : MarshalByRefObject, IDomainManager
    {
        private IStoppable _stoppable;
        private string _path;
        private TextWriterTraceListener _file;

        public AppDomain Domain
        {
            get
            {
                return AppDomain.CurrentDomain;
            }
        }

        public void Start(string logPath, string assemblyPath)
        {
            try
            {
                _path = assemblyPath;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                var file = File.CreateText(Path.Combine(logPath, "Output.txt"));
                Trace.AutoFlush = true;

                Trace.Listeners.Clear();
                _file = new TextWriterTraceListener(file);
                Trace.Listeners.Add(_file);

                var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var item in new DirectoryInfo(_path).GetFiles("*.dll"))
                {
                    if (allAssemblies.FirstOrDefault(x => x.GetName().Name == item.Name.Replace(".dll", string.Empty)) == null)
                    {
                        var binaries = File.ReadAllBytes(Path.Combine(_path, item.Name));
                        Assembly.Load(binaries);
                    }
                }

                var type = typeof(Document);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetTypes())
                        .Where(p => type.IsAssignableFrom(p)).ToArray();

                foreach (var item in types)
                {
                    try
                    {
                        var methodInfo = typeof(TypeUtil).GetMethod("RegisterDocument", BindingFlags.Public | BindingFlags.Static);
                        var generic = methodInfo.MakeGenericMethod(item);
                        generic.Invoke(null, null);
                    }
                    catch (Exception ex) { }
                }

                var application = Application.ParseFromJsonFile(Path.Combine(_path, Bootstrapper.DefaultApplicationFileName));
                _stoppable = Bootstrapper.StartAsync(application, loadAssembliesFromWorkingDirectory: false).Result;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        public void Stop()
        {
            _file.DisposeIfDisposable();
            _stoppable.StopAsync().Wait();
        }

        public void Dispose()
        {
            Stop();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (var assembly in loadedAssemblies)
                {
                    if (assembly.FullName == args.Name)
                    {
                        return assembly;
                    }
                }

                var binaries = File.ReadAllBytes(Path.Combine(_path, args.Name.Split(',')[0]) + ".dll");
                return Assembly.Load(binaries);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return null;
            }
        }

        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception)e.ExceptionObject;
            Trace.TraceError(exception.ToString());
        }
    }
}
