using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using z.Data;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using NUglify;
using InSys.Storage;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using jsreport.Types;
using jsreport.Local;
using jsreport.Binary;
using Newtonsoft.Json.Converters;

namespace InSys.Helper
{
    public class JSReport : IDisposable
    {
        public static IHostingEnvironment HostingEnvironment { get; set; }
        public static ILocalUtilityReportingService JSReportService;

        public JSReport()
        {
            //log4net.ThreadContext.Properties["Class"] = "JSReport";
        }
        public static void Initiate(IHostingEnvironment env)
        {
            HostingEnvironment = env;
            JSReportService = new LocalReporting()
               .UseBinary(JsReportBinary.GetBinary())
                .Configure(cfg =>
                {
                    cfg.AllowedLocalFilesAccess().FileSystemStore().BaseUrlAsWorkingDirectory();
                    cfg.Chrome = new ChromeConfiguration()
                    {
                        Timeout = 100000
                    };
                    return cfg;
                })
               .KillRunningJsReportProcesses()
               .AsUtility()
               .Create();
        }

        public void Dispose()
        {
            //if (File.Exists(ReportFile))
            //    File.Delete(ReportFile);

            GC.Collect();
            GC.SuppressFinalize(this);
        }

    }
    public class TemporaryTemplate
    {
        public string _id { get; set; }
        public string Shortid { get; set; }
        public string Content { get; set; }
        public string Helpers { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Engine? Engine { get; set; }
        public string Name { get; set; }
        public Chrome Chrome { get; set; }
        public HtmlToXlsx HtmlToXlsx { get; set; }
        public Phantom Phantom { get; set; }
        public Wkhtmltopdf Wkhtmltopdf { get; set; }
        public XlsxTemplate XlsxTemplate { get; set; }
        public IList<Script> Scripts { get; set; }
        public IList<PdfOperation> PdfOperations { get; set; }
    }
    public class ReportViewer : JSReport, IDisposable
    {
        private string ReportName { get; set; }
        private List<Dictionary<string, object>> SubReportSources { get; set; } = new List<Dictionary<string, object>>();
        private Dictionary<string, object> DataSource { get; set; }
        private string tmpFileName { get; set; }
        private string OutFile { get; set; }

        public ReportViewer(string rptName)
        {
            ReportName = rptName;
        }
        public ReportViewer SetData(object Data)
        {
            DataSource = new Dictionary<string, object>();
            DataSource.Add("DataSource", Data);
            return this;
        }
        public SubReport AddSubReportData(Dictionary<string, object> Data)
        {
            var rvmethod = new SubReport(this);
            rvmethod.AddSubReportData(Data);
            return rvmethod;
        }
        public ReturnSet LoadReport()
        {
            try
            {
                using (var strg = new Storage.Storage())
                {
                    var rptContainer = strg.Container("Reports");
                    tmpFileName = Path.GetTempFileName();

                    strg.DownloadToFile(rptContainer, (ReportName + ".json").ToUrlSlug(), tmpFileName);

                    try
                    {
                        var tmpRpt = File.ReadAllText(tmpFileName).ToObject<TemporaryTemplate>();
                        var rpt = new Template()
                        {
                            Recipe = Recipe.ChromePdf,
                            Content = tmpRpt.Content,
                            _id = tmpRpt._id,
                            Shortid = tmpRpt.Shortid,
                            Helpers = tmpRpt.Helpers,
                            Engine = tmpRpt.Engine,
                            Name = tmpRpt.Name,
                            Chrome = tmpRpt.Chrome,
                            HtmlToXlsx = tmpRpt.HtmlToXlsx,
                            PdfOperations = tmpRpt.PdfOperations,
                            Phantom = tmpRpt.Phantom,
                            Scripts = tmpRpt.Scripts,
                            Wkhtmltopdf = tmpRpt.Wkhtmltopdf,
                            XlsxTemplate = tmpRpt.XlsxTemplate
                        };

                        var rg = new RenderRequest();

                        rg.Template = rpt;

                        if (DataSource.Count == 0)
                            throw new Exception("Report doesn't contain any data");

                        rg.Data = DataSource.Count == 0 ? "{}" : DataSource.ToJson();

                        var report = JSReportService.RenderAsync(rg).Result;

                        byte[] buffer = report.Content.ToByteArray();
                        return new ReturnSet() { Data = $"data:application/pdf;base64,{Convert.ToBase64String(buffer)}", Type = ReturnType.Result };
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Cannot render report.", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "LoadReport", "internal", "InSys.Helpers");
                return new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
        }
        public ReturnSet LoadExcel()
        {
            try
            {
                using (var strg = new Storage.Storage())
                {
                    var rptContainer = strg.Container("Reports");
                    tmpFileName = Path.GetTempFileName();
                    var xlsName = "JSR-" + Guid.NewGuid().ToString().Replace("-", "") + ".xlsx";

                    while (strg.Exists(rptContainer, xlsName))
                    {
                        xlsName = "JSR-" + Guid.NewGuid().ToString().Replace("-", "") + ".xlsx";
                    }
                    OutFile = Path.Combine(Path.GetTempPath(), xlsName);
                    strg.DownloadToFile(rptContainer, (ReportName + ".json").ToUrlSlug(), tmpFileName);

                    try
                    {
                        var tmpRpt = File.ReadAllText(tmpFileName).ToObject<TemporaryTemplate>();
                        var rpt = new Template()
                        {
                            Recipe = Recipe.HtmlToXlsx,
                            Content = tmpRpt.Content,
                            _id = tmpRpt._id,
                            Shortid = tmpRpt.Shortid,
                            Helpers = tmpRpt.Helpers,
                            Engine = tmpRpt.Engine,
                            Name = tmpRpt.Name,
                            Chrome = tmpRpt.Chrome,
                            HtmlToXlsx = tmpRpt.HtmlToXlsx,
                            PdfOperations = tmpRpt.PdfOperations,
                            Phantom = tmpRpt.Phantom,
                            Scripts = tmpRpt.Scripts,
                            Wkhtmltopdf = tmpRpt.Wkhtmltopdf,
                            XlsxTemplate = tmpRpt.XlsxTemplate
                        };

                        var rg = new RenderRequest();

                        rg.Template = rpt;

                        if (DataSource.Count == 0)
                            throw new Exception("Report doesn't contain any data");

                        rg.Data = DataSource.Count == 0 ? "{}" : DataSource.ToJson();

                        var report = JSReportService.RenderAsync(rg).Result;

                        using (var fs = File.Create(OutFile))
                        {
                            report.Content.CopyTo(fs);
                        }

                        using (var stream = File.OpenRead(OutFile))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            var container = strg.Container("Files");
                            strg.Upload(container, Path.GetFileName(OutFile), stream);
                        }

                        return new ReturnSet() { Data = Path.GetFileName(OutFile), Type = ReturnType.Result };
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Cannot render report.", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "LoadReport", "internal", "InSys.Helpers");
                return new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
        }
        public new void Dispose()
        {
            if (File.Exists(tmpFileName))
                File.Delete(tmpFileName);
            if (File.Exists(OutFile))
                File.Delete(OutFile);
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
    public class SubReport
    {
        private ReportViewer rpt { get; set; }
        private List<Dictionary<string, object>> SubReportSources { get; set; }

        public SubReport(ReportViewer _rpt)
        {
            rpt = _rpt;
        }
        public ReportViewer AddSubReportData(Dictionary<string, object> Data)
        {
            rpt.AddSubReportData(Data);
            return rpt;
        }
    }
}
