using InSys.Worker.Library.Classes;
using Service.ETF.Classes;
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
using System.Timers;
using z.Data;

namespace Service.ETF
{
    public partial class ETFService : ServiceBase
    {
        Timer timer { get; set; }
        EventLog etfLog { get; set; }
        ETFConfig etfConfig { get; set; }
        BaseConfig baseConfig { get; set; }
        bool IsETFDone { get; set; } = true;
        FileInfo fi { get; set; }
        DirectoryInfo di { get; set; }
        string exePath { get; set; }

        public ETFService()
        {

            etfConfig = new ETFConfig();
            baseConfig = new BaseConfig();
            
            /*
                For Testing Purposes Only
                etfConfig.ExcelTemplatePath = "C:\\Users\\Public\\Projects\\InSys.Suite\\Development\\Solution\\Application\\InSys.Worker\\bin\\Debug\\Services\\ETF\\";
                baseConfig.Server = "10.12.50.40";
                baseConfig.Port = "1433";
                baseConfig.UserID = "sa";
                baseConfig.Password = "dev123sql$%^";
                baseConfig.Database = "InSys_HRMS_NEWORG_TRAINING";
            
                using (var etfProcess = new Classes.ETF(etfConfig.ExcelTemplatePath, baseConfig, etfLog))
                {
                IsETFDone = etfProcess.StartProcessing();
                Logger.LogMessage("Process done, waiting for next interval.", "StartETF", etfLog, EventLogEntryType.Information);
                }
            */

            InitializeComponent();
            fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
            di = fi.Directory.Parent.Parent;
            exePath = di.FullName;

            string LogName = "BAGWIS";
            string SourceName = "ETF Service";
            if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            etfLog = new EventLog(LogName);
            etfLog.Source = SourceName;
            etfLog.Log = LogName;

            InitConfig();
            if (etfConfig != null && baseConfig != null)
            {
                timer = new Timer();

                timer.Enabled = false;
                timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
                timer.Interval = (etfConfig.CheckInterval.ToInt32() * 60) * 1000;
            }
        }
        protected void InitConfig()
        {
            etfConfig = new ETFConfig().GetConfig(exePath, etfLog);
            baseConfig = new BaseConfig().GetBaseConfig(exePath);

            if (etfConfig == null) Logger.LogError("ETFConfig is null", "ETFService", etfLog);
            if (baseConfig == null) Logger.LogError("BaseConfig is null", "ETFService", etfLog);
        }
        protected override void OnStart(string[] args)
        {
            InitConfig();
            Logger.LogMessage("Service started.", "StartService", etfLog, EventLogEntryType.Information);

            if (timer != null)
            {
                StartETF();
                timer.Enabled = true;
            }
            else Logger.LogError("Timer is null", "OnStart", etfLog);
        }

        protected override void OnStop()
        {
            Logger.LogMessage("Service stopped.", "StopService", etfLog, EventLogEntryType.Information);
            IsETFDone = true;
            if (timer != null) timer.Enabled = false;
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            StartETF();
        }
        private void StartETF()
        {
            try
            {
                Task.Run(() =>
                {
                    if (IsETFDone)
                    {
                        IsETFDone = false;
                        //using (UNCAccessWithCredentials unc = new UNCAccessWithCredentials())
                        //{
                        //    if (unc.NetUseWithCredentials(etfConfig.ExcelTemplatePath, baseConfig.DUsername, baseConfig.DomainName, baseConfig.DPassword))
                        //    {
                            using (var etfProcess = new Classes.ETF(etfConfig.ExcelTemplatePath, baseConfig, etfLog))
                            {
                                IsETFDone = etfProcess.StartProcessing();
                                Logger.LogMessage("Process done, waiting for next interval.", "StartETF", etfLog, EventLogEntryType.Information);
                            }
                        //    }
                        //}

                    }
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "TimerElapse", etfLog);
                IsETFDone = true;
            }
        }
    }
}
