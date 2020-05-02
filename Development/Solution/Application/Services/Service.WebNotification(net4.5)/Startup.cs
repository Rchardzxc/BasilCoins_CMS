using InSys.Worker.Library.Classes;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;
using Service.WebNotification;
using Service.WebNotification.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

[assembly: OwinStartup(typeof(Startup))]
namespace Service.WebNotification
{
    public class Startup
    {
        static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static BaseConfig bCfg { get; set; }
        private static WebNotificationConfig clsConfig { get; set; }
        private static EventLog wnLog { get; set; }

        // Your startup logic
        public static void StartServer(BaseConfig _bcfg, WebNotificationConfig _clsConfig, EventLog _wnLog)
        {
            bCfg = _bcfg;
            clsConfig = _clsConfig;
            wnLog = _wnLog;

            var cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(RunSignalRServer, TaskCreationOptions.LongRunning
                                  , cancellationTokenSource.Token);
        }
        private static void RunSignalRServer(object task)
        {
            string url = $"http://localhost:{clsConfig.Port}";
            WebApp.Start(url);

            Logger.LogMessage("SignalR started.", "RunSignalRServer", wnLog, EventLogEntryType.Information);
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<WebNotificationHub>();
            var dependency = new WebNotificationDependency(hubContext, bCfg, wnLog);
            dependency.Start();
            Console.ReadLine();
        }
        public static void StopServer()
        {
            _cancellationTokenSource.Cancel();
            Logger.LogMessage("SignalR stopped.", "RunSignalRServer", wnLog, EventLogEntryType.Information);
        }
        public void Configuration(IAppBuilder app)
        {
            var policy = new CorsPolicy()
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                SupportsCredentials = true
            };
            foreach (var client in clsConfig.WebClient.Split(',').ToArray())
            {
                policy.Origins.Add(client);
            }
            app.UseCors(new CorsOptions()
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context => Task.FromResult(policy)
                }
            });
            app.MapSignalR("/webNotificationHub", new HubConfiguration() { });
        }
    }
}
