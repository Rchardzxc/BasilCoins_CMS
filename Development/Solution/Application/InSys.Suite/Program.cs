using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using InSys.Context;
using InSys.Helper;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InSys.Suite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                BuildWebHost(args).Run();
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "void Main", "internal", "InSys.HRMS");
            }
        }

        public static IWebHost BuildWebHost(string[] args, string CurrentDirectory = null)
        {
            if (CurrentDirectory == null)
                CurrentDirectory = Directory.GetCurrentDirectory();

            var builder = new ConfigurationBuilder()
                         .SetBasePath(CurrentDirectory)
                         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var Configuration = builder.Build();

            Config.LoadSection(Configuration.GetSection("AppSetting"));

            ContextConfig.AddSQLConnection(Config.SQLConnection);
            ContextConfig.AddKioskConnection(Config.KioskConnection);

            var iWeb = WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(CurrentDirectory)
                .UseIISIntegration()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .UseUrls($"http://+:{Config.PortableServerPort}") //;https://127.0.0.1:{Config.PortableServerPort + 1}
                .UseStartup<Startup>()
                //.ConfigureKestrel(options =>
                //{
                //    //options.Limits.MaxRequestBodySize = 52428800; //50MB
                //    //options.Listen(IPAddress.Loopback, Config.PortableServerPort);
                //})
                .Build();

            return iWeb;
        }
    }
}
