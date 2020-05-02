using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Service.WebNotification.Classes
{
    public class BaseConfig
    {
        public string Database { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string UserID { get; set; }
        public string Password { get; set; }
        public string DUsername { get; set; }
        public string DPassword { get; set; }
        public string DomainName { get; set; }

        public BaseConfig GetBaseConfig(string exePath, EventLog elog = null)
        {
            try
            {
                var baseCfg = new BaseConfig();
                if (!File.Exists(Path.Combine(exePath, "InSys.Worker.exe"))) {
                    if (elog != null)
                    {
                        elog.LogEntry($"Cant find InSys.Worker in directory {exePath}", EventLogEntryType.Information);
                        return new BaseConfig();
                    }
                }

                var cfg = ConfigurationManager.OpenExeConfiguration(Path.Combine(exePath, "InSys.Worker.exe"));
                foreach (var prop in baseCfg.GetType().GetProperties())
                {
                    if (cfg.AppSettings.Settings[prop.Name] != null) baseCfg.SetValue(prop.Name, cfg.AppSettings.Settings[prop.Name].Value);
                    else baseCfg.SetValue(prop.Name, "");
                }
                return baseCfg;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "GetConfig", elog);
                return null;
            }

        }
        public string GetSqlConfig()
        {
            return $"Server={Server},{Port};Database={Database};User ID={UserID};Password={Password};Trusted_Connection=False;MultipleActiveResultSets=True;Connection Timeout=60;Max Pool Size=1000;";
        }
    }
}
