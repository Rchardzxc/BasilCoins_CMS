using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InSys.Helper;
using InSys.ITI.Menu;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InSys.Suite.Controllers
{
    public class RouteController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected readonly IHostingEnvironment Host;
        protected IHttpContextAccessor Accessor;
        private tCompany company { get; set; }
        public RouteController(IHostingEnvironment hostingEnvironment, IHttpContextAccessor accessor)
        {
            this.Host = hostingEnvironment;
            this.Accessor = accessor;
            //string url = Url.Content("~");
        }

        public IActionResult Index()
        {
            string BasePath = "Base";
            if (HttpContext.Session != null && HttpContext.Session.Keys.Contains("Session"))
            {
                company = MenuCollection.CompanyCollection.Where(x => x.ID == HttpContext.Session.Get<int>("ID_Company")).FirstOrDefault();
                BasePath = "Base";

                if (Directory.Exists(Path.Combine(Host.WebRootPath, company.Code))) BasePath = $"{company.Code}";
                HttpContext.Session.Set("BasePath", company == null ? "Base" : $"{company.Code}");

                if (!Config.bundle.Assets.Where(x => x.Route == $"/bundles/{BasePath}-angular-scripts-{Config.bundleUpdateCount}").Any())
                {
                    var insysBundle = new InSysBundles(Host);
                    insysBundle.Bundle(Config.bundle, InSysBundles.InSysSystem.HRMS, BasePath);
                }

                return InSys("Index", BasePath);
            }
            else
                return InSys("Login", BasePath);
        }

        public IActionResult Error()
        {
            var nPage = Path.Combine($"{Path.Combine(Host.WebRootPath, HttpContext.Session.Get<string>("BasePath"))}", "Error.html");
            var cPage = System.IO.File.ReadAllText(nPage);
            return new ContentResult
            {
                Content = Regex.Replace(cPage, @"^\s+", "", RegexOptions.Multiline | RegexOptions.Compiled),
                ContentType = "text/html",
                StatusCode = 200
            };
        }

        protected IActionResult InSys(string page, string BasePath)
        {
            var nPage = Path.Combine($"{Path.Combine(Host.WebRootPath, BasePath)}", "InSys.html");
            var cPage = System.IO.File.ReadAllText(nPage);

            var token = EncryptionHandler.CreateClientToken(Request, Accessor);

            cPage = ReplaceCode(cPage, "EncryptionString", token);
            cPage = ReplaceCode(cPage, "env", (!Debugger.IsAttached).ToString().ToLower());
            cPage = ReplaceCode(cPage, "timeOut", Config.SessionTimeOut.ToString());
            cPage = ReplaceCode(cPage, "showSystemError", Config.ShowSystemError.ToString());
            cPage = ReplaceCode(cPage, "TemplatePath", $"/{BasePath}/");

            var gPage = Path.Combine($"{Path.Combine(Host.WebRootPath, BasePath)}", "Web/Views", $"{ page }.html");
            cPage = ReplaceCode(cPage, "RenderBody", System.IO.File.ReadAllText(gPage));

            List<string> Dependency = new List<string>();
            if (Config.Environment.ToLower() == "production")
            {
                Dependency.Add($"<script src=\"{BasePath}/Scripts/System/Out/corebundle.min.js\"></script>");
            }
            else
            {
                Dependency.Add($"<script src=\"{BasePath}/Scripts/System/app.js\"></script>");
                Dependency.Add($"<script src=\"{BasePath}/Scripts/System/Config.js\"></script>");
                Dependency.Add($"<script src=\"{BasePath}/Scripts/System/Controllers.js\"></script>");
                Dependency.Add($"<script src=\"{BasePath}/Scripts/System/DataServices.js\"></script>");
                Dependency.Add($"<script src=\"{BasePath}/Scripts/System/Directives.js\"></script>");
            }
            cPage = ReplaceCode(cPage, "GenericScripts", $"<script src=\"/bundles/{BasePath}-generic-scripts-{Config.bundleUpdateCount.ToString()}\"></script>");
            cPage = ReplaceCode(cPage, "AngularScripts", $"<script src=\"/bundles/{BasePath}-angular-scripts-{Config.bundleUpdateCount.ToString()}\"></script>");
            cPage = ReplaceCode(cPage, "ReferenceStyles", $"<link href=\"/bundles/{BasePath}-reference-styles-{Config.bundleUpdateCount.ToString()}\" rel=\"stylesheet\"\"/>");
            if (page == "Login")
            {
                cPage = ReplaceCode(cPage, "SessionID", "");
                cPage = ReplaceCode(cPage, "RenderController", "");
                cPage = ReplaceCode(cPage, "RenderRoutes", "");
            }
            else
            {
                Dependency.Add($"<script src=\"{BasePath}/Scripts/System/signalr/dist/browser/signalr.min.js\"></script>");
                cPage = ReplaceCode(cPage, "SessionID", HttpContext.Session.Id);
                cPage = ReplaceCode(cPage, "RenderController", $"<script src=\"/bundles/{BasePath}-controller-scripts-{Config.bundleUpdateCount.ToString()}\"></script>");
                cPage = ReplaceCode(cPage, "RenderRoutes", $"<script src=\"/bundles/{BasePath}-route-scripts-{Config.bundleUpdateCount.ToString()}\"></script>");
            }

            cPage = ReplaceCode(cPage, "RenderDependency", string.Join(Environment.NewLine, Dependency));

            return new ContentResult
            {
                Content = Regex.Replace(cPage, @"^\s+", "", RegexOptions.Multiline | RegexOptions.Compiled),
                ContentType = "text/html",
                StatusCode = 200
            };
        }

        protected string ReplaceCode(string body, string key, string repvalue)
        {
            RegexOptions options = RegexOptions.Multiline;

            Regex regex = new Regex($@"\[\[\b{ key }\b\]\]", options);
            return regex.Replace(body, repvalue);
        }
    }
}
