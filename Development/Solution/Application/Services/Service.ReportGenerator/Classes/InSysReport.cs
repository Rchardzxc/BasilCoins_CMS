using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.ReportGenerator.Classes
{
    public class InSysReport
    {
        public int ID { get; set; }
        public string DataSource { get; set; }
        public string ReportFile { get; set; }
        public DbConnection DbConnection { get; set; }
        public ReportType Type { get; set; }
        public List<SubReport> SubReports { get; set; } = new List<SubReport>();
        public List<ReportParameter> ReportParameters { get; set; } = new List<ReportParameter>();
    }
    public class SubReport
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string DataSource { get; set; }
        public DbConnection DbConnection { get; set; }
        public List<ReportParameter> ReportParameters { get; set; } = new List<ReportParameter>();
    }
    public class ReportParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public enum DbConnection : int
    {
        HRMS = 1,
        KIOSK = 2
    }
    public enum ReportType: int
    {
        PDF = 1,
        XLS = 2
    }
}
