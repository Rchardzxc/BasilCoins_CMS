using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.RecruitmentReport
{
    class RecruitmentReportDb: InSysContext
    {
        public RecruitmentReportDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tApplicationForm> tApplicationForm { get; set; }
        public DbQuery<CountData> countDatas { get; set; }
        public DbQuery<vKioskMRFStatus> vKioskMRFStatus { get; set; }
        public DbQuery<vKioskMRFStatusDetailCount> vKioskMRFStatusDetailCount { get; set; }
        public DbQuery<vKioskMRFStatusDetails> vKioskMRFStatusDetails { get; set; }
    }
}
