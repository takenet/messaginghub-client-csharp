using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Listener;
using Topshelf;

namespace Takenet.MessagingHub.Client.Host
{
    public class HostService : ServiceControl
    {
        private IStoppable _stoppable;

        
        public bool Start(HostControl hostControl)
        {            
            var applicationFileName = Program.GetApplicationFileName(new string[0]);
            if (!File.Exists(applicationFileName))
            {
                throw new Exception($"Could not find the {applicationFileName} file");                
            }

            _stoppable = Program.StartAsync(applicationFileName).Result;
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                _stoppable.StopAsync(cts.Token).Wait();
            }
            return true;
        }
    }
}
