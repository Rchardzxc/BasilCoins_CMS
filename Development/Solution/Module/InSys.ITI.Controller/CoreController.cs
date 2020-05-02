using InSys.Context;
using InSys.Helper;
using InSys.ITI.Menu;
using InSys.ITI.Models.Models;
using LZStringCSharp;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using z.Data;

namespace InSys.ITI.Controller
{
    [InvariantCulture]
    public abstract class CoreController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected readonly IHostingEnvironment hostingEnvironment;
        public Pair Parameter { get; set; }
        public IDbConnection Sql { get; set; }
        protected Exception Error { get; set; }
        public HttpContext Ctx { get; private set; }
        public static BrowserSession Session { get; set; } = null;
        public BaseModule ClassModule { get; set; } = null;
        public string WebRootPath { get; set; }
        public CoreParameters CoreParams { get; set; }
        public string AssemblyPath { get; set; } = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public IAntiforgery antiForgery;
        public void LoadMenuClass()
        {
            Type type = LoadAssembly();
            if(type != null) ClassModule = Activator.CreateInstance(type) as BaseModule;
        }
        private Type LoadAssembly()
        {
            if (Parameter["MenuCode"].IsNull("").ToString() != "")
            {
                if (Session == null) throw new Exception("Session Expired.");
                var Menu = MenuCollection.AllMenu.Where(x => x.ID_Company == Session.ID_Company && x.Code == Parameter["MenuCode"].ToString()).FirstOrDefault();
                if (Menu == null) throw new Exception($"Could not find menu {Parameter["MenuCode"].ToString()}.");
                if (!System.IO.File.Exists(Path.Combine(AssemblyPath, $"{Menu.AssemblyName}.dll"))) throw new Exception($"Could not find file {Menu.AssemblyName}.");

                var Asm = System.Reflection.Assembly.LoadFrom(Path.Combine(AssemblyPath, $"{Menu.AssemblyName}.dll"));
                Type type = Asm.GetTypes().Where(x => x.Name == Menu.ClassName).FirstOrDefault();
                if (type == null) throw new Exception($"Could not find {Menu.ClassName} in {Menu.AssemblyName}");
                else return type;
            }
            else
                return null;
        }

        protected CoreController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery)
        {
            try
            {
                this.WebRootPath = hostingEnvironment.WebRootPath;
                this.Parameter = new Pair(StringComparer.OrdinalIgnoreCase);
                this.hostingEnvironment = hostingEnvironment;
                this.antiForgery = _antiForgery;
                CoreParams = new CoreParameters();
                CoreParams.SetParameter(this.Parameter);
                CoreParams.SetSession(Session);
            }
            catch (Exception ex)
            {
                Error = ex;
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "CoreController Constructor", Helpers.CurrentUser(Session), "InSys.Controller");
            }
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                Ctx = context.HttpContext;
                Session = Ctx.Session.Get("Session") != null ? Ctx.Session.Get<BrowserSession>("Session") : null;
                var bodyStr = "";
                var req = context.HttpContext.Request;

                if (context.HttpContext.Request.ContentLength > 0)
                {
                    if (context.HttpContext.Request.HasFormContentType || context.HttpContext.Request.ContentLength > 1e+8)
                    {
                        base.OnActionExecuting(context);
                        return;
                    }
                    using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
                    {
                        bodyStr = reader.ReadToEnd();
                    }
                    var nQ = CryptoJS.Decrypt(LZString.DecompressFromBase64(Uri.UnescapeDataString(bodyStr)), Config.Encryption.Key, Config.Encryption.Salt).ToObject<Pair>();
                    if (nQ == null)
                    {
                        string errMsg = "Parameter is not valid";
                        Logger.LogError(ref errMsg, "CoreController OnActionExecuting", "internal", "InSys.Controller");
                        throw new Exception("Parameter is not valid");
                    }

                    nQ.CopyTo(Parameter);

                    nQ?.Dispose();
                }
                else if (context.HttpContext.Request.Method == "GET")
                {
                    foreach (var dd in context.HttpContext.Request.Query)
                        Parameter.Add(dd.Key, dd.Value);
                }
                LoadMenuClass();
                base.OnActionExecuting(context);
            }
            catch (Exception ex)
            {
                //ex.Error(ex.Message);
                Error = ex;
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "CoreController OnActionExecuting", "internal", "InSys.Controller");
            }
        }

        #region Protected

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected virtual async Task<Result> TaskResult(Func<Result, Result> action, Action Finally = null)
        {
            return await Task.Run(() =>
            {
                var r = new Result();

                if (Error != null)
                {
                    r.Status = 1;
                    r.ErrorMsg = Error.Message;
                }
                else
                    try
                    {
                        r.Status = 0;
                        r = action(r);
                        r.MethodName = nameof(action);
                        r.ResultSet = Convert.ToBase64String(Encoding.UTF8.GetBytes(CryptoJS.Encrypt(r.ResultSet.ToJson(), Config.Encryption.Key, Config.Encryption.Salt)));

                        Ctx.Response.Headers.Remove("X-Frame-Options");
                        Ctx.Response.Headers.Add("X-Frame-Options", "deny");                        
                    }
                    catch (Exception ex)
                    {
                        r.MethodName = new StackTrace(ex).GetFrame(0).GetMethod().Name;
                        r.Status = 1;
                        r.ErrorMsg = (ex.InnerException ?? ex).Message;
                        string errmsg = r.ErrorMsg;
                        Logger.LogError(ref errmsg, r.MethodName, "internal", "InSys.Controller");
                    }
                    finally
                    {
                        Finally?.Invoke();
                    }
                return r;
            });
        }
        protected virtual async Task<Result> TaskVoid(Action action) => await TaskResult(r =>
        {
            action();
            return r;
        });

        #endregion

    }
}
