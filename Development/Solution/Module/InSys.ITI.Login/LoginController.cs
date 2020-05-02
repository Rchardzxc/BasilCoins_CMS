using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Menu;
using InSys.ITI.Models.Models;
using LZStringCSharp;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.ITI.Login
{
    [Route("api/[controller]/[action]")]
    [EndRequestCompressor]
    public class LoginController : CoreController
    {
        public LoginController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }
        #region Request
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<Result> Login() => await TaskResult(r =>
        {
            var login = new Login(Parameter, Session);
            r.ResultSet = login.Authenticate();
            return r;
        });
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<Result> ServerSession() => await TaskResult(r =>
        {
            string loginname = "";
            try
            {
                var tmpSession = new BrowserSession() { ID_User = Parameter["ID_User"].ToInt32() };
                using (var db = new LoginDb(tmpSession))
                {
                    var user = db.Single<tUsers>("select * from dbo.tUsers where ID = {0}", Parameter["ID_User"].ToInt32());
                    loginname = user.LogInName;
                    AddSession(Parameter["ID_User"].ToInt32(), Parameter["ID_Company"].ToInt32(), tmpSession);
                    var bSession = new ServerSession()
                    {
                        ID_Company = Parameter["ID_Company"].ToInt32() == 0 ? CompanyAccessRights.GetDefaultCompany(user.ID_Roles) : Parameter["ID_Company"].ToInt32(),
                        ID_Employee = user.ID_Employee,
                        ID_Roles = user.ID_Roles,
                        ID_Users = user.ID
                    };
                    foreach (var prop in bSession.GetType().GetProperties())
                    {
                        Ctx.Session.Set(prop.Name, bSession.GetType().GetProperty(prop.Name).GetValue(bSession));
                    }
                    user.SessionID = Ctx.Session.Id.ToString();
                    Ctx.Session.Set("Session", user);

                    var token = antiForgery.GetAndStoreTokens(HttpContext).RequestToken;
                    Ctx.Session.Set("CurrentToken", token);
                    Response.Cookies.Append("XSRF-REQUEST-TOKEN", token, new CookieOptions
                    {
                        HttpOnly = false
                    });
                }

                Config.bundleUpdateCount += 1;
                var insysBundle = new InSysBundles(hostingEnvironment);
                insysBundle.Bundle(Config.bundle, InSysBundles.InSysSystem.HRMS);
                MenuCollection.Init();

                r.ResultSet = new ReturnSet() { Message = "Redirecting to dashboard.", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "ServerSession", loginname, "InSys.ITI.Login");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
            return r;
        });
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<Result> SaveAccount() => await TaskResult(r => //Para sa FirstLogin
        {
            var login = new Login(Parameter, Session);
            r.ResultSet = login.SaveFirstLog();
            return r;
        });
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<Result> SaveAccountExpired() => await TaskResult(r => //Para sa naexpired na password
        {
            var login = new Login(Parameter, Session);
            r.ResultSet = login.SaveAccountExpired();
            return r;
        });
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<Result> LoadCompany() => await TaskResult(r =>
        {
            var login = new Login(Parameter, Session);
            r.ResultSet = login.LoadCompany();
            return r;
        });
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<Result> LoadQuestion() => await TaskResult(r =>
        {
            var login = new Login(Parameter, Session);
            r.ResultSet = login.LoadQuestion();
            return r;
        });
        #endregion

        private int AddSession(int ID_User, int ID_Company, BrowserSession tmpSession)
        {
            int ID_Session = 0;
            string LoginName = null;
            try
            {
                using (var db = new LoginDb(tmpSession))
                {
                    LoginName = db.Single<tUsers>("select * from dbo.tUsers where ID = {0}", ID_User)?.LogInName;
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            tSessionUser user = new tSessionUser()
                            {
                                ID_User = ID_User,
                                ID_Company = ID_Company,
                                IPAddress = IpAddress.GetIP().IsNull("").ToString(),
                                StartDateTime = DateTime.Now
                            };
                            db.Add(user);
                            db.SaveChanges(false);

                            ID_Session = user.ID;

                            tran.Commit();
                        }
                        catch (Exception dbEx)
                        {
                            tran.Rollback();
                            string message = (dbEx.InnerException ?? dbEx).Message;
                            Logger.LogError(ref message, "AddSession", LoginName, "InSys.ITI.Login");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "AddSession", LoginName, "InSys.ITI.Login");
            }

            return ID_Session;
        }
    }
}
