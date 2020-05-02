using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using WebOptimizer;

namespace InSys.Helper
{
    public sealed class Config
    {
        public static int PortableServerPort { get; set; }
        public static string SQLConnection { get; set; }
        public static Encryption Encryption { get; set; }
        public static int SessionTimeOut { get; set; }
        public static string TimeZone { get; set; }
        public static bool ShowSystemError { get; set; }
        public static string Environment { get; set; }
        public static string KioskConnection { get; set; }
        public static bool IsBrokerEnabled { get; set; } = false;
        public static string WebNotificationServer { get; set; } = "";
        public static string[] WebNotificationClient { get; set; }
        public static bool UseReportServer { get; set; } = false;
        public static string ReportServer { get; set; } = "";
        public static IMvcBuilder mvcBuilder { get; set; }
        public static IAssetPipeline bundle { get; set; }
        public static int bundleUpdateCount { get; set; } = 0;
        public static string[] WebsiteAssemblies { get; set; }
        public static string StorageConnection { get; set; }
        public static string RootContainer { get; set; }
        public static void LoadSection(IConfigurationSection Section)
        {
            PortableServerPort = Section.GetValue<int>("PortableServerPort");
            SQLConnection = Section.GetValue<string>("SQLConnection");
            Encryption = Section.GetSection("Encryption").Get<Encryption>();
            SessionTimeOut = Section.GetValue<int>("SessionTimeOut");
            TimeZone = Section.GetValue<string>("TimeZone");
            ShowSystemError = Section.GetValue<bool>("ShowSystemError");
            Environment = Section.GetValue<string>("Environment");
            KioskConnection = Section.GetValue<string>("KioskConnection");
            WebNotificationServer = Section.GetValue<string>("WebNotificationServer");
            WebNotificationClient = Section.GetSection("WebNotificationClient").Get<string[]>();
            UseReportServer = Section.GetValue<bool>("UseReportServer");
            ReportServer = Section.GetValue<string>("ReportServer");
            WebsiteAssemblies = Section.GetSection("WebsiteAssemblies").Get<string[]>();
            StorageConnection = Section.GetValue<string>("StorageConnection");
            RootContainer = Section.GetValue<string>("RootContainer");
        }
    }

    public class Encryption
    {
        public string Key { get; set; }
        public string Salt { get; set; }
        public string UID { get; set; }
    }
}
