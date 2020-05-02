using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Service.WebNotification.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Service.WebNotification
{
    public class Startup
    {
        WebNotificationConfig clsConfig { get; set; }
        FileInfo fi { get; set; }
        DirectoryInfo di { get; set; }
        string exePath { get; set; }
        public void ConfigureServices(IServiceCollection services)
        {
            fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
            di = fi.Directory.Parent.Parent;
            exePath = di.FullName;
            clsConfig = new WebNotificationConfig().GetConfig(exePath);
            services.AddCors(options => options.AddPolicy("CorsPolicy",
                        b =>
                        {
                            foreach(var client in clsConfig.WebClient.Split(',').ToArray())
                            {
                                b.AllowAnyMethod().AllowAnyHeader()
                                   .WithOrigins(client.ToString())
                                   .AllowCredentials();
                            }
                        }));
            services.AddSignalR();
            //services.AddHostedService<Worker>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var dependency = new WebNotificationDependency(app.ApplicationServices.GetRequiredService<IHubContext<WebNotificationHub>>());
            dependency.Start();
            app.UseCors("CorsPolicy");
            app.UseSignalR((routes) =>
            {
                routes.MapHub<WebNotificationHub>("/webNotificationHub");
            });
        }
    }
}
