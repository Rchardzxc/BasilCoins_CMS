using System;
using System.Diagnostics;
using System.IO;
using InSys.Assembly;
using InSys.Helper;
using InSys.ITI.Menu;
using InSys.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace InSys.Suite
{
    public class Startup
    {
        public string WebRootPath { get; }
        public IConfiguration Configuration { get; }

        private IHostingEnvironment HostingEnv;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnv = env;
            WebRootPath = env.ContentRootPath;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddMvc();
            Logger.WebRootPath = HostingEnv.WebRootPath;
            services.AddSingleton<IActionDescriptorChangeProvider>(InSysActionDescriptorChangeProvider.Instance);
            services.AddSingleton(InSysActionDescriptorChangeProvider.Instance);

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

            StorageSetting.Init(Config.StorageConnection, Config.RootContainer);
            services.AddSingleton<IStorage, Storage.Storage>();
            
            Config.mvcBuilder = builder;
            //CompanyPolicy.Load_Policies();
            //AssemblyManager.Initiate();
            //AssemblyManager.RegisterController(builder);
            //AssemblyManager.CreateFilesFromAssembly(HostingEnv.WebRootPath);
            MenuCollection.Init();
            SystemSettings.LoadSystemSettings();
            services.AddWebOptimizer(pipeline => new InSysBundles(HostingEnv).Bundle(pipeline));
            if (HostingEnv.IsDevelopment()) services.AddWebOptimizer(false, false);

            services.AddSession(opt =>
            {
                opt.IdleTimeout = TimeSpan.FromMinutes(Config.SessionTimeOut);
                opt.Cookie.HttpOnly = true;
                opt.Cookie.Name = "InSys.Suite";
            });
            services.ConfigureEndRequest(x =>
            {
                x.Key = Config.Encryption.Key;
                x.Salt = Config.Encryption.Salt;
            });
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
            });
            if (Debugger.IsAttached)
                services.AddDirectoryBrowser();
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }
            if (!StorageSetting.IsCloudPath)
            {
                var fProvider = new PhysicalFileProvider(Path.Combine(StorageSetting.StoragePath, StorageSetting.RootContainer));
                var rPath = PathString.FromUriComponent(Path.Combine(StorageSetting.RequestPath, StorageSetting.RootContainer).Replace('\\', '/'));
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = fProvider,
                    RequestPath = rPath
                });

                if (Debugger.IsAttached)
                    app.UseDirectoryBrowser(new DirectoryBrowserOptions
                    {
                        FileProvider = fProvider,
                        RequestPath = rPath
                    });
            }
            JSReport.Initiate(env);

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings.Add(".properties", "text/plain");

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });
            app.UseCookiePolicy();
            app.UseSession();
            app.UseWebOptimizer();
            app.UseEndRequestCompressor();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Route", action = "Index" }
                    );
            });
        }
    }
}
