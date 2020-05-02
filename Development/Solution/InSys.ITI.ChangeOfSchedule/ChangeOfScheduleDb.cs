using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.ChangeOfSchedule
{
    public class ChangeOfScheduleDb : InSysContext
    {
        public ChangeOfScheduleDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tChangeOfSchedule> tChangeOfSchedule { get; set; }
        public DbSet<tChangeOfSchedule_Detail> tChangeOfSchedule_Detail { get; set; }
        public DbQuery<vChangeOfSchedule_Detail> vChangeOfSchedule_Detail { get; set; }
        public DbQuery<CountData> CountData { get; set; }
        public DbQuery<vChangeOfSchedule> vChangeOfSchedule { get; set; }
        public DbQuery<tLookUpData> tLookUpDatas { get; set; }
        public DbSet<tWeeklySchedule> tWeeklySchedule { get; set; }
        public DbQuery<vGetWeeklySchedule> vGetWeeklySchedule { get; set; }
    }
}
