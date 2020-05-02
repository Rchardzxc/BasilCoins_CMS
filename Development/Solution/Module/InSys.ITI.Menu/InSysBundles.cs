using InSys.Context;
using InSys.Helper;
using InSys.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUglify;
using NUglify.JavaScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebOptimizer;

namespace InSys.ITI.Menu
{
    public class InSysBundles
    {
        public enum InSysSystem
        {
            HRMS = 1,
            KIOSK = 2
        }
        private IHostingEnvironment host;
        private string JSFolder { get; set; } = "Scripts/References/";
        private string CSSFolder { get; set; } = "Styles/References/";
        public InSysBundles()
        {

        }

        public InSysBundles(IHostingEnvironment hostingEnvironment)
        {
            this.host = hostingEnvironment;
        }

        public void Bundle(IAssetPipeline bundles, InSysSystem type = InSysSystem.HRMS, string BasePath = "Base")
        {
            Config.bundle = bundles;
            
            if (Directory.Exists(Path.Combine(host.WebRootPath, BasePath))) BasePath = $"{BasePath}";
            else BasePath = "Base";

            bundles.AddJavaScriptBundle($"/bundles/{BasePath}-angular-scripts-{Config.bundleUpdateCount}", SetAngularScripts(type, $"{BasePath}/")).Concatenate();
            bundles.AddJavaScriptBundle($"/bundles/{BasePath}-generic-scripts-{Config.bundleUpdateCount}", SetGenericScripts(type, $"{BasePath}/")).Concatenate();
            //bundles.AddJavaScriptBundle($"/bundles/{BasePath}-independent-scripts", SetIndependentScripts(type, $"{BasePath}/")).Concatenate();
            if (SetControllerScripts().Length > 0) bundles.AddJavaScriptBundle($"/bundles/{BasePath}-controller-scripts-{Config.bundleUpdateCount}", SetControllerScripts(type, $"{BasePath}/")).Concatenate();
            if (SetRouteScripts().Length > 0) bundles.AddJavaScriptBundle($"/bundles/{BasePath}-route-scripts-{Config.bundleUpdateCount}", SetRouteScripts(type, $"{BasePath}/")).Concatenate();
            bundles.AddCssBundle($"/bundles/{BasePath}-reference-styles-{Config.bundleUpdateCount}", SetCSSStyles(type, $"{BasePath}/")).MinifyCss();

            if (type == InSysSystem.HRMS)
            {
                if (!Config.bundle.Assets.Where(x => x.Route == $"/bundles/{BasePath}-MasterCss").Any())
                {
                    bundles.AddCssBundle($"/bundles/{BasePath}-MasterCss", $"{BasePath}/Styles/System/Master.min.css");
                    bundles.AddCssBundle($"/bundles/{BasePath}-DarkMasterCss", $"{BasePath}/Styles/System/Dark/Dark_Master.min.css");
                }
            }
            else
            {
                if (!Config.bundle.Assets.Where(x => x.Route == $"/bundles/{BasePath}-MasterCss").Any()) bundles.AddCssBundle($"/bundles/{BasePath}-MasterCss", new string[] { $"{BasePath}/Styles/System/SiteMaster.min.css", $"{BasePath}Styles/System/main.css" });
            }

            bundles.MinifyJsFiles(new CodeSettings() { MinifyCode = false });
            if (Config.Environment.ToLower() == "production")
                minifyTemplates();
        }

        private string[] SetGenericScripts(InSysSystem type = InSysSystem.HRMS, string BasePath = "Base/")
        {
            var ret = new string[0];
            var commonScripts = new string[]
            {
                $"{BasePath + JSFolder}jquery.min.js",
                $"{BasePath + JSFolder}linq.min.js",
                $"{BasePath + JSFolder}lz-string.min.js",
                $"{BasePath + JSFolder}moment.min.js",
                $"{BasePath + JSFolder}moment-range.min.js",
                $"{BasePath + JSFolder}nickcrypt.js",
                $"{BasePath + JSFolder}base64js.min.js",
                $"{BasePath + JSFolder}input-module.js"
            };
            if (type == InSysSystem.HRMS)
            {
                var hrmsScripts = new string[] {
                    $"{BasePath + JSFolder}bootstrap.bundle.min.js",
                    $"{BasePath + JSFolder}ResizeSensor.min.js",
                    $"{BasePath + JSFolder}ElementQueries.min.js",
                };
                ret = new string[commonScripts.Length + hrmsScripts.Length];
                commonScripts.CopyTo(ret, 0);
                hrmsScripts.CopyTo(ret, commonScripts.Length);
            }
            else
            {
                var kioskScripts = new string[] {
                    $"{BasePath + JSFolder}bootstrap-3.3.7.min.js",
                    $"{BasePath + JSFolder}bootstrap-datetimepicker.min.js",
                    $"{BasePath + JSFolder}jquery.ui-1.12.1.min.js"
                };
                ret = new string[commonScripts.Length + kioskScripts.Length];
                commonScripts.CopyTo(ret, 0);
                kioskScripts.CopyTo(ret, commonScripts.Length);
            }
            return ret;
        }

        private string[] SetAngularScripts(InSysSystem type = InSysSystem.HRMS, string BasePath = "Base/")
        {
            var ret = new string[0];
            var commonScripts = new string[] {
            $"{BasePath + JSFolder}angular.min.js",
            $"{BasePath + JSFolder}angular-cookies.min.js",
            $"{BasePath + JSFolder}angular-sanitize.min.js",
            $"{BasePath + JSFolder}angular-ui-router.min.js",
            $"{BasePath + JSFolder}ui-bootstrap-tpls.min.js",
            };
            if (type == InSysSystem.HRMS)
            {
                var hrmsScripts = new string[] {
                    $"{BasePath + JSFolder}angular-animate.min.js",
                    $"{BasePath + JSFolder}textAngular-rangy.min.js",
                    $"{BasePath + JSFolder}textAngular-sanitize.min.js",
                    $"{BasePath + JSFolder}textAngular.min.js",
                    $"{BasePath + JSFolder}angular-ui-notification.js",
                    $"{BasePath + JSFolder}angular-rb-grid.min.js",
                    $"{BasePath + JSFolder}contextMenu.min.js"
                };
                ret = new string[commonScripts.Length + hrmsScripts.Length];
                commonScripts.CopyTo(ret, 0);
                hrmsScripts.CopyTo(ret, commonScripts.Length);
            }
            else
            {
                var kioskScripts = new string[] {
                    $"{BasePath + JSFolder}growl-notifications.js"
                };
                ret = new string[commonScripts.Length + kioskScripts.Length];
                commonScripts.CopyTo(ret, 0);
                kioskScripts.CopyTo(ret, commonScripts.Length);
            }
            return ret;
        }

        private string[] SetCSSStyles(InSysSystem type = InSysSystem.HRMS, string BasePath = "Base/")
        {
            var ret = new string[0];
            if (type == InSysSystem.HRMS)
            {
                var hrmsStyles = new string[] {
                    $"{BasePath + CSSFolder}bootstrap.min.css",
                    $"{BasePath + CSSFolder}fontawesome.min.css",
                    $"{BasePath + CSSFolder}brands.min.css",
                    $"{BasePath + CSSFolder}solid.min.css",
                    $"{BasePath + CSSFolder}angular-ui-notification.css",
                };
                ret = new string[hrmsStyles.Length];
                hrmsStyles.CopyTo(ret, 0);
            }
            else
            {
                var kioskStyles = new string[] {
                    $"{BasePath + CSSFolder}bootstrap.min.css",
                    $"{BasePath + CSSFolder}font-awesome.min.css",
                    $"{BasePath + CSSFolder}bootstrap-datetimepicker.min.css"
                };
                ret = new string[kioskStyles.Length];
                kioskStyles.CopyTo(ret, 0);
            }
            return ret;
        }

        private string[] SetControllerScripts(InSysSystem type = InSysSystem.HRMS, string BasePath = "Base/")
        {
            DirectoryInfo objDirectoryInfo = new DirectoryInfo(Path.Combine(host.WebRootPath, $"{BasePath}Web/Controllers/"));
            string[] allFiles = objDirectoryInfo.GetFiles("*.js", SearchOption.AllDirectories).Select(x => x.FullName.Replace(host.WebRootPath, "")).ToArray();

            return allFiles;
        }

        private string[] SetRouteScripts(InSysSystem type = InSysSystem.HRMS, string BasePath = "Base/")
        {
            DirectoryInfo objDirectoryInfo = new DirectoryInfo(Path.Combine(host.WebRootPath, $"{BasePath}Web/Routes/"));
            string[] allFiles = objDirectoryInfo.GetFiles("*.js", SearchOption.AllDirectories).Select(x => x.FullName.Replace(host.WebRootPath, "")).ToArray();

            return allFiles;
        }

        public void minifyTemplates(string BasePath = "Base/")
        {
            deleteOldTemplates();

            var templateFolders = new string[] { "Web/Views/", "Web/Directives/", "Web/Dialogs/" };

            var buildFolder = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string WebRootPath = Path.Combine(host.WebRootPath, BasePath);

            while (Directory.Exists(Path.Combine(Path.Combine(WebRootPath, "BuildTemplate"), buildFolder)))
                buildFolder = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            
            foreach (var folder in templateFolders)
            {
                if (!Directory.Exists(Path.Combine(WebRootPath, "BuildTemplate")))
                    Directory.CreateDirectory(Path.Combine(WebRootPath, "BuildTemplate"));

                if (!Directory.Exists(Path.Combine(Path.Combine(WebRootPath, "BuildTemplate"), buildFolder)))
                    Directory.CreateDirectory(Path.Combine(Path.Combine(WebRootPath, "BuildTemplate"), buildFolder));

                if (!Directory.Exists(Path.Combine(Path.Combine(Path.Combine(WebRootPath, "BuildTemplate"), buildFolder), folder)))
                {
                    Directory.CreateDirectory(Path.Combine(Path.Combine(Path.Combine(WebRootPath, "BuildTemplate"), buildFolder), folder));

                    foreach (string dirPath in Directory.GetDirectories(Path.Combine(WebRootPath, folder), "*", SearchOption.AllDirectories))
                        Directory.CreateDirectory(dirPath.Replace(Path.Combine(WebRootPath, folder), Path.Combine(Path.Combine(Path.Combine(WebRootPath, "BuildTemplate"), buildFolder), folder)));

                    foreach (string file in Directory.GetFiles(Path.Combine(WebRootPath, folder), "*.html", SearchOption.AllDirectories))
                    {
                        using (var sr = new StreamReader(file))
                        {
                            var content = sr.ReadToEnd();
                            var result = Uglify.Html(content);
                            using (var sr2 = new StreamWriter(file.Replace(Path.Combine(WebRootPath, folder), Path.Combine(Path.Combine(Path.Combine(WebRootPath, "BuildTemplate"), buildFolder), folder))))
                                sr2.WriteLine(result.Code);
                        }
                    }
                }
            }
        }
        //private string[] SetIndependentScripts(InSysSystem type = InSysSystem.HRMS)
        //{
        //    var ret = new string[0];
        //    var commonScripts = new string[]
        //    {};
        //    if (type == InSysSystem.HRMS)
        //    {
        //        var hrmsScripts = new string[] {};
        //        ret = new string[commonScripts.Length + hrmsScripts.Length];
        //        commonScripts.CopyTo(ret, 0);
        //        hrmsScripts.CopyTo(ret, commonScripts.Length);
        //    }
        //    else
        //    {
        //        var kioskScripts = new string[] {};
        //        ret = new string[commonScripts.Length + kioskScripts.Length];
        //        commonScripts.CopyTo(ret, 0);
        //        kioskScripts.CopyTo(ret, commonScripts.Length);
        //    }
        //    return ret;
        //}
        private void deleteOldTemplates(string BasePath = "Base/")
        {
            string WebRootPath = Path.Combine(host.WebRootPath, BasePath);
            if (Directory.Exists(Path.Combine(WebRootPath, "BuildTemplate")))
            {
                var dInfo = new DirectoryInfo(Path.Combine(WebRootPath, "BuildTemplate"));
                var directories = dInfo.EnumerateDirectories().OrderByDescending(x => x.CreationTime).Select(x => x.Name).ToList();
                var indx = 0;
                foreach (var dr in directories)
                {
                    if (indx > 1) Directory.Delete(Path.Combine(Path.Combine(WebRootPath, "BuildTemplate"), dr), true);
                    indx += 1;
                }
            }
        }
    }
}