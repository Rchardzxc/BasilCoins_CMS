using InSys.Worker.Library.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.SelfHost;

namespace Service.WebNotification
{
    public partial class Service1 : ServiceBase
    {
        HttpSelfHostConfiguration config { get; set; }
        FileInfo fi { get; set; }
        DirectoryInfo di { get; set; }
        string exePath { get; set; }
        EventLog wnLog { get; set; }
        WebNotificationConfig clsConfig { get; set; }
        BaseConfig baseConfig { get; set; }
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
            di = fi.Directory.Parent.Parent;
            exePath = di.FullName;

            string LogName = "BAGWIS";
            string SourceName = "Web Notification Service";
            if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            wnLog = new EventLog(LogName);
            wnLog.Source = SourceName;
            wnLog.Log = LogName;

            InitConfig();

            Logger.LogMessage("Service started.", "StartService", wnLog, EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            Logger.LogMessage("Service stopped.", "StopService", wnLog, EventLogEntryType.Information);
            Startup.StopServer();
        }
        protected void InitConfig()
        {
            clsConfig = new WebNotificationConfig().GetConfig(exePath, wnLog);
            baseConfig = new BaseConfig().GetBaseConfig(exePath);

            if (clsConfig == null) Logger.LogError("WebNotificationConfig is null", "WebNotificationService", wnLog);
            if (baseConfig == null) Logger.LogError("BaseConfig is null", "WebNotificationService", wnLog);

            if (clsConfig != null && baseConfig != null)
            {
                try
                {
                    Startup.StartServer(baseConfig, clsConfig, wnLog);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "InitConfig", wnLog);
                }
            }
        }
    }
}
