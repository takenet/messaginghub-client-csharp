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

        public AppDomain Domain
        {
            get
            {
                return AppDomain.CurrentDomain;
            }
        }

        public void Start(string logPath, string assemblyPath)
        {
            _path = assemblyPath;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var file = File.CreateText(Path.Combine(logPath, "Output.txt"));
            Trace.AutoFlush = true;

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(file));

            foreach (var item in new DirectoryInfo(_path).GetFiles("*.dll"))
            {
                var binaries = File.ReadAllBytes(Path.Combine(_path, item.Name));
                Assembly.Load(binaries);
            }

            var application = Application.ParseFromJsonFile(Path.Combine(_path, Bootstrapper.DefaultApplicationFileName));
            _stoppable = Bootstrapper.StartAsync(application).Result;
        }

        public void Stop()
        {
            _stoppable.StopAsync().Wait();
        }

        public void Dispose()
        {
            Stop();
        }

        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var binaries = File.ReadAllBytes(Path.Combine(_path, args.Name.Split(',')[0]) + ".dll");
            return Assembly.Load(binaries);
        }
    }
}
