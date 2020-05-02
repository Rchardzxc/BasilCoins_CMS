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
using System.Web.Http.SelfHost;
using System.Web.Http.Cors;

namespace Service.ReportGenerator
{
    public partial class ReportGeneratorService : ServiceBase
    {
        HttpSelfHostConfiguration config { get; set; }
        HttpSelfHostServer server { get; set; }
        FileInfo fi { get; set; }
        DirectoryInfo di { get; set; }
        string exePath { get; set; }
        EventLog rptLog { get; set; }
        ReportGeneratorConfig clsConfig { get; set; }
        BaseConfig baseConfig { get; set; }
        public ReportGeneratorService()
        {
            InitializeComponent();
            fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
            di = fi.Directory.Parent.Parent;
            exePath = di.FullName;

            string LogName = "BAGWIS";
            string SourceName = "Report Generator Service";
            if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            rptLog = new EventLog(LogName);
            rptLog.Source = SourceName;
            rptLog.Log = LogName;
            InitConfig();
        }

        protected override void OnStart(string[] args)
        {
            InitConfig();

            Logger.LogMessage("Service started.", "StartService", rptLog, EventLogEntryType.Information);
            if (server != null) { server.OpenAsync().Wait(); }
        }
        protected override void OnStop()
        {
            Logger.LogMessage("Service stopped.", "StopService", rptLog, EventLogEntryType.Information);
            if (server != null) server.CloseAsync();
        }
        protected void InitConfig()
        {
            clsConfig = new ReportGeneratorConfig().GetConfig(exePath, rptLog);
            baseConfig = new BaseConfig().GetBaseConfig(exePath);

            if (clsConfig == null) Logger.LogError("ReportConfig is null", "ReportGeneratorService", rptLog);
            if (baseConfig == null) Logger.LogError("BaseConfig is null", "ReportGeneratorService", rptLog);

            if (clsConfig != null && baseConfig != null)
            {
                try
                {
                    config = new HttpSelfHostConfiguration($"http://localhost:{clsConfig.Port}");
                    config.MapHttpAttributeRoutes();

                    foreach (var client in clsConfig.WebClient.Split(',').ToArray())
                    {
                        var cors = new EnableCorsAttribute(client, "*", "*");
                        config.EnableCors(cors);
                    }

                    config.Routes.MapHttpRoute( 
                        name: "ReportGenerator", 
                        routeTemplate: "api/{controller}/{action}/{id}", 
                        defaults: new { id = RouteParameter.Optional } 
                        );
                    server = new HttpSelfHostServer(config);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "InitConfig", rptLog);
                }
            }
        }
    }
}
