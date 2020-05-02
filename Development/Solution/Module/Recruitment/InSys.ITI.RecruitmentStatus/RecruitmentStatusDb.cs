using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.RecruitmentStatus
{
    public class RecruitmentStatusDb: InSysContext
    {
        public RecruitmentStatusDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbQuery<CountData> countDatas { get; set; }
        public DbSet<tRecruitmentStatus> tRecruitmentStatus { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
    }
}
