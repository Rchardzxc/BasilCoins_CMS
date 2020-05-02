using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.ITI.Index
{
    [Route("api/[controller]/[action]")]
    [EndRequestCompressor]
    public class IndexController : CoreController
    {
        public IndexController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //dating ValidateAntiForgeryToken
        public async Task<Result> BrowserSession() => await TaskResult(r =>
        {
            try
            {
                using (var db = new IndexDb(null))
                {
                    var user = db.Single<BrowserSession>($"{Helpers.GetSqlQuery("BrowserSession")}" +
                        " where tu.ID = {1}", Ctx.Session.Get<int>("ID_Company"), Ctx.Session.Get<int>("ID_Users"));
                    user.ID_Company = Ctx.Session.Get<int>("ID_Company");
                    user.Company = GetCompanyName(Ctx.Session.Get<int>("ID_Company"));
                    user.WebNotificationServer = Config.WebNotificationServer;
                    user.SessionID = Ctx.Session.Id.ToString();
                    Ctx.Session.Set("Session", user);
                    r.ResultSet = new ReturnSet() { Data = user, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "BrowserSession", Helpers.CurrentUser(Ctx), "InSys.ITI.Index");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
            return r;
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadCompany() => await TaskResult(r =>
        {
            var index = new Index(Parameter, Session);
            r.ResultSet = index.LoadCompany();
            return r;
        });

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<Result> Logoff() => await TaskResult(r =>
        {
            try
            {
                Ctx.Session.Clear();
                r.ResultSet = new ReturnSet() { Type = ReturnType.Result };
                return r;
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "Logoff", Helpers.CurrentUser(Session), "InSys.ITI.Index");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadProfile() => await TaskResult(r =>
        {
            var index = new Index(Parameter, Session);
            r.ResultSet = index.LoadProfile();
            return r;
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadNotification() => await TaskResult(r =>
        {
            var index = new Index(Parameter, Session);
            r.ResultSet = index.LoadNotification();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> SeeNotification() => await TaskResult(r =>
        {
            var index = new Index(Parameter, Session);
            r.ResultSet = index.SeeNotification();
            return r;
        });

        private static string GetCompanyName(int ID_Company)
        {
            string name = "";
            using (var db = new IndexDb(null))
            {
                name = db.Single<tCompany>("select * from dbo.tCompany where ID = {0}", ID_Company)?.Name;
            }
            return name;
        }
    }
}
