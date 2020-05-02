using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using InSys.Worker.Library.Classes;
using Service.ReportGenerator.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using z.Data;

namespace Service.ReportGenerator
{
    public class ReportController: ApiController
    {
        private EventLog rptLog { get; set; }
        private BaseConfig bConfig { get; set; }
        private ReportGeneratorConfig rptConfig { get; set; }
        private FileInfo fi { get; set; }
        private DirectoryInfo di { get; set; }
        private string exePath { get; set; }
        public ReportController() {
            string LogName = "BAGWIS";
            string SourceName = "Report Generator Service";
            if (!EventLog.SourceExists(SourceName)) EventLog.CreateEventSource(SourceName, LogName);
            rptLog = new EventLog(LogName);
            rptLog.Source = SourceName;
            rptLog.Log = LogName;

            fi = new FileInfo(AppDomain.CurrentDomain.BaseDirectory);
            di = fi.Directory.Parent.Parent;
            exePath = di.FullName;

            bConfig = new BaseConfig().GetBaseConfig(exePath, rptLog);
            rptConfig = new ReportGeneratorConfig().GetConfig(exePath, rptLog);
        }

        [HttpPost]
        public ReturnSet loadPdf()
        {
            var ret = new ReturnSet();
            using (UNCAccessWithCredentials unc = new UNCAccessWithCredentials())
            {
                if (unc.NetUseWithCredentials(rptConfig.ReportPath, bConfig.DUsername, bConfig.DomainName, bConfig.DPassword))
                {
                    Logger.LogMessage("Generating Report", "loadPdf", rptLog, EventLogEntryType.Information);
                    var tmpFileName = Path.GetTempFileName() + ".rpt";
                    try
                    {
                        var pData = Request.Content.ReadAsAsync<PostData>().Result;
                        var data = Config.GetTable("select *, null SubReports, null ReportParameters from dbo.tReportDataSource where ID = @ID and SessionID = @ID2", bConfig.GetSqlConfig(), pData.ID.ToInt32(), pData.sID.ToString().Replace("-", "")).Single();
                        if (data == null)
                            throw new Exception("Could not find report source.");
                        var report = new InSysReport()
                        {
                            DataSource = data["DataSource"].ToString(),
                            ID = data["ID"].ToInt32(),
                            ReportFile = data["ReportFile"].ToString(),
                            DbConnection = data["DbConnection"].ToObject<DbConnection>(),
                            Type = data["ReportType"].ToObject<ReportType>(),
                        };
                        report.SubReports = Config.GetTable("select *, null ReportParameters from dbo.tReportDataSource_SubReport where ID_ReportDataSource = @ID", bConfig.GetSqlConfig(), pData.ID.ToInt32()).ToJson().ToObject<List<SubReport>>();
                        foreach (var sr in report.SubReports)
                        {
                            sr.ReportParameters = Config.GetTable("select * from dbo.tReportDataSource_ReportParameterField where ID_ReportDataSource_SubReport = @ID", bConfig.GetSqlConfig(), sr.ID).ToJson().ToObject<List<ReportParameter>>();
                        }
                        report.ReportParameters = Config.GetTable("select * from dbo.tReportDataSource_ReportParameterField where ID_ReportDataSource = @ID", bConfig.GetSqlConfig(), pData.ID.ToInt32()).ToJson().ToObject<List<ReportParameter>>();

                        if (report.ReportFile != null && report.ReportFile != "")
                        {
                            var file = new FileInfo(Path.Combine(rptConfig.ReportPath, report.ReportFile + ".rpt"));
                            file.CopyTo(tmpFileName, true);

                            using (ReportDocument rep = new ReportDocument())
                            {
                                rep.Load(tmpFileName);

                                if (report.SubReports != null)
                                    foreach (var sr in report.SubReports)
                                    {
                                        foreach (var rp in sr.ReportParameters)
                                        {
                                            rep.SetParameterValue(rp.Name, rp.Value, sr.Name);
                                        }
                                        rep.OpenSubreport(sr.Name).SetDataSource(Config.GetTable(sr.DataSource, sr.DbConnection == DbConnection.HRMS ? bConfig.GetSqlConfig() : rptConfig.GetKioskSqlConfig()));
                                    }
                                else
                                    foreach (ReportDocument sr in rep.Subreports)
                                        foreach (IConnectionInfo dsc in sr.DataSourceConnections)
                                            if (report.DbConnection == DbConnection.HRMS)
                                                dsc.SetConnection(bConfig.Server, bConfig.Database, bConfig.UserID, bConfig.Password);
                                            else
                                                dsc.SetConnection(rptConfig.KioskServer, rptConfig.KioskDatabase, rptConfig.KioskUserID, rptConfig.KioskPort);

                                foreach (var rp in report.ReportParameters)
                                {
                                    rep.SetParameterValue(rp.Name, rp.Value);
                                }

                                if (report.DbConnection == DbConnection.HRMS)
                                    rep.SetDatabaseLogon(bConfig.UserID, bConfig.Password, bConfig.Server, bConfig.Database);
                                else
                                    rep.SetDatabaseLogon(rptConfig.KioskUserID, rptConfig.KioskPassword, rptConfig.KioskServer, rptConfig.KioskDatabase);
                                rep.SetDataSource(Config.GetTable(report.DataSource, report.DbConnection == DbConnection.HRMS ? bConfig.GetSqlConfig() : rptConfig.GetKioskSqlConfig()));

                                if (report.Type == ReportType.PDF)
                                    using (var stream = rep.ExportToStream(ExportFormatType.PortableDocFormat))
                                    {
                                        var buffer = stream.ToByteArray();
                                        ret = new ReturnSet() { Data = $"data:application/pdf;base64,{Convert.ToBase64String(buffer)}", Type = ReturnType.Result };
                                    }
                                else
                                    using (var stream = rep.ExportToStream(ExportFormatType.Excel))
                                    {
                                        var buffer = stream.ToByteArray();
                                        ret = new ReturnSet() { Data = $"data:application/vnd.ms-excel;base64,{Convert.ToBase64String(buffer)}", Type = ReturnType.Result };
                                    }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = (ex.InnerException ?? ex).Message;
                        Logger.LogError(ex, "loadPdf", rptLog);
                        ret = new ReturnSet() { Message = msg, Type = ReturnType.Error };
                    }
                    finally
                    {
                        if (File.Exists(tmpFileName))
                            File.Delete(tmpFileName);
                    }
                }
            }
            Logger.LogMessage("Generating Report Finished.", "loadPdf", rptLog, EventLogEntryType.Information);
            return ret;
        }
    }
    
}
