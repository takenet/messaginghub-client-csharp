using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Takenet.MessagingHub.Client.Host;

namespace Takenet.MessagingHub.ServiceActivator
{
    public partial class MessagingHubActivatorService : ServiceBase
    {
        private IDisposable _activator;

        public MessagingHubActivatorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var basePath = ConfigurationManager.AppSettings["basePath"] ?? Environment.CurrentDirectory;            
            TimeSpan waitForActivationDelay;
            if (!TimeSpan.TryParse(ConfigurationManager.AppSettings["waitForActivationDelay"], out waitForActivationDelay))
            {
                waitForActivationDelay = TimeSpan.FromSeconds(5);
            }

            _activator = new ApplicationActivator(basePath, waitForActivationDelay);
        }

        protected override void OnStop()
        {
            _activator.Dispose();
        }
    }
}
