using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.ServiceProcess;

namespace Service.WebNotification
{
    public class Program
    {
        static void Main(string[] args)
        {
            ServiceBase.Run(new WebNotificationService());
            //BuildWebHost(args).Run();
        }
    }
}
