using InSys.Context;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.Helper
{
    public class InSysReport : IDisposable
    {
        private int ID { get; set; }
        private string DataSource { get; set; }
        private string ReportFile { get; set; }
        private DbConnection DbConnection { get; set; }
        private ReportType Type { get; set; }
        public List<InSysSubReport> SubReports { get; private set; } = new List<InSysSubReport>();
        private List<InSysReportParameter> ReportParameters { get; set; } = new List<InSysReportParameter>();
        private static readonly HttpClient client = new HttpClient();

        public InSysReport(string Query, string rptFile, DbConnection dbCon, ReportType type = ReportType.PDF)
        {
            this.DataSource = Query;
            this.ReportFile = rptFile;
            this.DbConnection = dbCon;
            this.Type = type;
        }
        public InSysSubReport AddSubReport(string SubReportName, string Query, DbConnection dbCon)
        {
            var subReport = new InSysSubReport(this)
            {
                Name = SubReportName,
                DataSource = Query,
                DbConnection = dbCon
            };
            this.SubReports.Add(subReport);
            return subReport;
        }
        public InSysReport AddReportParameter(string ParameterName, string ParameterValue)
        {
            var reportParameter = new InSysReportParameter()
            {
                Name = ParameterName,
                Value = ParameterValue
            };
            this.ReportParameters.Add(reportParameter);
            return this;
        }
        public ReturnSet CreateReport(BrowserSession Session)
        {
            int ID = 0;
            try
            {
                using (var db = new InSysReportDB(Session))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var rpt = new tReportDataSource()
                            {
                                DataSource = this.DataSource,
                                ReportFile = this.ReportFile,
                                SessionID = Session.SessionID.Replace("-", ""),
                                DbConnection = this.DbConnection.ToInt32(),
                                ReportType = this.Type.ToInt32()
                            };
                            db.Add(rpt);
                            db.SaveChanges(true);
                            ID = rpt.ID;

                            foreach (var sr in this.SubReports)
                            {
                                var subRpt = new tReportDataSource_SubReport()
                                {
                                    DataSource = sr.DataSource,
                                    ID_ReportDataSource = rpt.ID,
                                    Name = sr.Name,
                                    DbConnection = this.DbConnection.ToInt32()
                                };
                                db.Add(subRpt);
                                db.SaveChanges(true);

                                int ID_SubReport = subRpt.ID;
                                foreach (var rp in sr.ReportParameters)
                                {
                                    var param = new tReportDataSource_ReportParameterField()
                                    {
                                        Name = rp.Name,
                                        Value = rp.Value,
                                        ID_ReportDataSource_SubReport = ID_SubReport
                                    };
                                    db.Add(param);
                                }
                            }

                            foreach (var rp in this.ReportParameters)
                            {
                                var param = new tReportDataSource_ReportParameterField()
                                {
                                    ID_ReportDataSource = ID,
                                    Name = rp.Name,
                                    Value = rp.Value
                                };
                                db.Add(param);
                            }

                            db.SaveChanges();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw ex;
                        }
                    }
                }
                return new ReturnSet() { Data = new { ID = ID, UseReportServer = true, Url = Config.ReportServer + "/api/Report/loadPdf" }, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }

    public enum DbConnection : int
    {
        HRMS = 1,
        KIOSK = 2
    }
    public class InSysSubReport
    {
        private InSysReport Report { get; set; }
        public InSysSubReport(InSysReport rpt)
        {
            Report = rpt;
        }
        public int? ID_ReportDataSource { get; set; }
        public string Name { get; set; }
        public string DataSource { get; set; }
        public DbConnection DbConnection { get; set; }
        public List<InSysReportParameter> ReportParameters { get; set; } = new List<InSysReportParameter>();
        public InSysSubReport AddReportParameter(string ParameterName, string ParameterValue)
        {
            var reportParameter = new InSysReportParameter()
            {
                Name = ParameterName,
                Value = ParameterValue
            };
            this.ReportParameters.Add(reportParameter);
            return this;
        }
        public InSysSubReport AddSubReport(string SubReportName, string Query, DbConnection dbCon)
        {
            var subReport = new InSysSubReport(Report)
            {
                Name = SubReportName,
                DataSource = Query,
                DbConnection = dbCon
            };
            Report.SubReports.Add(subReport);
            return subReport;
        }
        public InSysReport CreateSubReport()
        {
            return Report;
        }
    }
    public class InSysReportParameter
    {
        public int? ID_ReportDataSource { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int? ID_ReportDataSource_SubReport { get; set; }
    }
    public class InSysReportDB : InSysContext
    {
        public InSysReportDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tReportDataSource> tReportDataSources { get; set; }
        public DbSet<tReportDataSource_ReportParameterField> tReportDataSource_ReportParameterFields { get; set; }
        public DbSet<tReportDataSource_SubReport> tReportDataSource_SubReports { get; set; }
    }
    public enum ReportType: int
    {
        PDF = 1,
        XLS = 2
    }
}
