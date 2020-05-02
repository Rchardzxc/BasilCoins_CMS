using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Service.WebNotification.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;

namespace Service.WebNotification
{
    public class WebNotificationService: ServiceBase
    {
        public EventLog wnLog { get; set; }
        IWebHost wHost { get; set; }
        static WebNotificationConfig clsConfig { get; set; }
        FileInfo fi { get; set; }
        DirectoryInfo di { get; set; }
        string exePath { get; set; }
        public WebNotificationService()
        {
            string LogName = "BAGWIS";
            string SourceName = "Web Notification Service";
            if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            wnLog = new EventLog(LogName);
            wnLog.Source = SourceName;
            wnLog.Log = LogName;

            fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
            di = fi.Directory.Parent.Parent;
            exePath = di.FullName;
            clsConfig = new WebNotificationConfig().GetConfig(exePath, wnLog);
        }
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            wHost = BuildWebHost(args);
            wHost.RunAsync();
            Logger.LogMessage("Service Started.", "OnStart", wnLog, EventLogEntryType.Information);
        }

        protected override async void OnStop()
        {
            base.OnStop();
            Logger.LogMessage("Service stopped.", "OnStop", wnLog, EventLogEntryType.Information);
            if (wHost != null) {
                await wHost.StopAsync();
                wHost.Dispose();
            }
        }

        public static IWebHost BuildWebHost(string[] args, string CurrentDirectory = null)
        {
            if (CurrentDirectory == null)
                CurrentDirectory = Directory.GetCurrentDirectory();

            var builder = new ConfigurationBuilder()
                         .SetBasePath(CurrentDirectory);
            //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var Configuration = builder.Build();

            var iWeb = WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(CurrentDirectory)
                .UseIISIntegration()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    //config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .UseUrls($"http://127.0.0.1:{clsConfig.Port.ToString()}") //;https://127.0.0.1:{Config.PortableServerPort + 1}
                .UseStartup<Startup>()
                .Build();

            return iWeb;
        }
    }
}
