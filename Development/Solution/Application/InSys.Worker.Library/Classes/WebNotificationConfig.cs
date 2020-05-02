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
    public class WebNotificationConfig
    {
        public string Port { get; set; }
        public string WebClient { get; set; }

        public WebNotificationConfig GetConfig(string exePath, EventLog elog = null)
        {
            try
            {
                var clsConfig = new WebNotificationConfig();
                var cfg = ConfigurationManager.OpenExeConfiguration(Path.Combine(exePath, "InSys.Worker.exe"));
                AppSettingsSection section = cfg.GetSection("WebNotification") as AppSettingsSection;
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
    }
}
