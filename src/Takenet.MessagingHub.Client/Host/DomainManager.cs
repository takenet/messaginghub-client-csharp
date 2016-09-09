using Lime.Protocol;
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
                var file = File.CreateText(Path.Combine(logPath, "Output.txt"));
                Trace.AutoFlush = true;

                Trace.Listeners.Clear();
                _file = new TextWriterTraceListener(file);
                Trace.Listeners.Add(_file);

                foreach (var item in new DirectoryInfo(_path).GetFiles("*.dll"))
                {
                    var binaries = File.ReadAllBytes(Path.Combine(_path, item.Name));
                    Assembly.Load(binaries);
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
    }
}
