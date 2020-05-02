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
    public class ETFConfig
    {
        public string CheckInterval { get; set; }
        public string ExcelTemplatePath { get; set; }

        public ETFConfig GetConfig(string exePath, EventLog elog = null)
        {
            try
            {
                var clsConfig = new ETFConfig();
                var cfg = ConfigurationManager.OpenExeConfiguration(Path.Combine(exePath, "InSys.Worker.exe"));
                AppSettingsSection section = cfg.GetSection("ETF") as AppSettingsSection;
                foreach (var prop in clsConfig.GetType().GetProperties())
                {
                    if (section.Settings[prop.Name] != null) {

                        if (prop.Name == "ExcelTemplatePath"
                        && (!Directory.Exists(Path.Combine(section.Settings[prop.Name].Value))))
                        {
                            Directory.CreateDirectory(Path.Combine(exePath, section.Settings[prop.Name].Value));
                            Directory.CreateDirectory(Path.Combine(exePath, section.Settings[prop.Name].Value + "\\New"));
                            Directory.CreateDirectory(Path.Combine(exePath, section.Settings[prop.Name].Value + "\\Processing"));
                            Directory.CreateDirectory(Path.Combine(exePath, section.Settings[prop.Name].Value + "\\Successful"));
                            Directory.CreateDirectory(Path.Combine(exePath, section.Settings[prop.Name].Value + "\\Unprocess"));
                        }

                        clsConfig.SetValue(prop.Name, section.Settings[prop.Name].Value);

                    }
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
    }
}
