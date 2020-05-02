using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSys.Worker.Library.Classes
{
    public class ReportGeneratorConfig
    {
        public string Port { get; set; }
        public string ReportPath { get; set; }
        public string WebClient { get; set; }
        public string KioskServer { get; set; }
        public string KioskDatabase { get; set; }
        public string KioskUserID { get; set; }
        public string KioskPassword { get; set; }
        public string KioskPort { get; set; }

        public ReportGeneratorConfig GetConfig(string exePath, EventLog elog = null)
        {
            try
            {
                var clsConfig = new ReportGeneratorConfig();
                var cfg = ConfigurationManager.OpenExeConfiguration(Path.Combine(exePath, "InSys.Worker.exe"));
                AppSettingsSection section = cfg.GetSection("Report") as AppSettingsSection;
                foreach (var prop in clsConfig.GetType().GetProperties())
                {
                    if (section.Settings[prop.Name] != null) clsConfig.SetValue(prop.Name, section.Settings[prop.Name].Value);
                    else clsConfig.SetValue(prop.Name, "");
                }
                return clsConfig;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetConfig", elog);
                return null;
            }

        }
        public string GetKioskSqlConfig()
        {
            return $"Server={KioskServer},{KioskPort};Database={KioskDatabase};User ID={KioskUserID};Password={KioskPassword};Trusted_Connection=False;MultipleActiveResultSets=True;Connection Timeout=60;Max Pool Size=1000;";
        }
    }
}
