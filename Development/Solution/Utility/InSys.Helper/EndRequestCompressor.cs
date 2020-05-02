using LZStringCSharp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.Helper
{
    public class EndRequestCompressor
    {
        private readonly RequestDelegate _next;
        private readonly EndRequestConfiguration Conf;
        public const string EndRequestActionKey = "EndRequestActionKey";

        public EndRequestCompressor(RequestDelegate next, EndRequestConfiguration conf)
        {
            _next = next;
            this.Conf = conf;
        }

        public async Task Invoke(HttpContext context)
        {
            Stream originalBody = context.Response.Body;

            using (var memStream = new MemoryStream())
            {
                context.Response.Body = memStream;

                await _next(context);

                memStream.Position = 0;
                string responseBody = new StreamReader(memStream).ReadToEnd();

                if (context.Request.Headers.ContainsKey(EndRequestActionKey))
                {
                    context.Request.Headers.Remove(EndRequestActionKey);
                    context.Response.Headers["Content-Type"] = "text/plain; charset=utf-8";

                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        responseBody = Uri.EscapeDataString(LZString.CompressToBase64(Convert.ToBase64String(Encoding.UTF8.GetBytes(responseBody))));
                        //responseBody = Uri.EscapeDataString(Convert.ToBase64String(Encoding.UTF8.GetBytes(CryptoJS.Encrypt(responseBody, Conf.Key, Conf.Salt))));
                    }

                    memStream.Position = 0;
                    var bt = Encoding.UTF8.GetBytes(responseBody);
                    memStream.Write(bt, 0, bt.Length);
                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                    context.Response.Body = originalBody;
                }
                else
                {
                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);
                    context.Response.Body = originalBody;
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EndRequestCompressorAttribute : TypeFilterAttribute
    {
        public EndRequestCompressorAttribute() : base(typeof(EndRequestFilter))
        {
        }
    }

    public class EndRequestFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            context.HttpContext.Request.Headers.Add(EndRequestCompressor.EndRequestActionKey, Guid.NewGuid().ToString());
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {

        }
    }

    public class EndRequestConfiguration
    {
        public string Key { get; set; }
        public string Salt { get; set; }
    }

    public static class EndRequestCompressorExtensions
    {
        public static IApplicationBuilder UseEndRequestCompressor(this IApplicationBuilder builder)
        {
            return builder
                .UseWhen(context => !context.Request.Path.StartsWithSegments("/api/Download"), appBuilder =>
                {
                    appBuilder.Use(async (context, next) =>
                    {
                        context.Request.EnableRewind();
                        await next();
                    }).UseMiddleware<EndRequestCompressor>();
                });
        }

        public static IServiceCollection ConfigureEndRequest(this IServiceCollection serviceCollection, Action<EndRequestConfiguration> conf)
        {
            return serviceCollection.AddSingleton(new Func<IServiceProvider, EndRequestConfiguration>(x =>
            {
                var g = new EndRequestConfiguration();
                conf(g);
                return g;
            }));
        }
    }
}
