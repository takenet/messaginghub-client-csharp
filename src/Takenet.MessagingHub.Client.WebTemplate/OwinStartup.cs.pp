using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Owin;
using Takenet.MessagingHub.Client.Host;

[assembly: OwinStartup(typeof($rootnamespace$.OwinStartup))]

namespace $rootnamespace$
{
    public class OwinStartup
    {
        /// <summary>
        /// OWIN configuration. 
        /// DO NOT CHANGE THIS METHOD!
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configuration(IAppBuilder app)
        {
            app.Run(async context =>
            {
                Trace.TraceInformation("Starting application...");

                var applicationPath = Path.Combine(
                    AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    "application.json");
                var binPath = $"{AppDomain.CurrentDomain.SetupInformation.ApplicationBase}bin";
                var application = Application.ParseFromJsonFile(applicationPath);
                var stopabble = await Bootstrapper.StartAsync(application, true, binPath);
                var properties = new AppProperties(app.Properties);
                properties.OnAppDisposing.Register(() =>
                {
                    Trace.TraceInformation("Stopping application...");
                    stopabble.StopAsync().Wait();
                    Trace.TraceInformation("Application stopped.");
                });
                Trace.TraceInformation("Application started.");
            });
        }
    }
}
