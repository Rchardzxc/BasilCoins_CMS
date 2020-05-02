using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.OrbitChangeOfSchedule
{
    public class OrbitChangeOfScheduleDb : InSysContext
    {
        public OrbitChangeOfScheduleDb(BrowserSession session) : base(session)
        {
            this.SessionContext = session;
        }

        public DbQuery<CountData> countData { get; set; }
        public DbQuery<vOrbiChangeOfScheduleList> vOrbiChangeOfScheduleList { get; set; }
        public DbSet<tChangeOfSchedule> tChangeOfSchedule { get; set; }
        public DbSet<tChangeOfSchedule_Detail> tChangeOfSchedule_Detail { get; set; }
    }
}
