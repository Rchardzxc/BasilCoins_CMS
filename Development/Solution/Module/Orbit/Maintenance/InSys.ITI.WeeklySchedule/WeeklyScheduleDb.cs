using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.WeeklySchedule
{
    public class WeeklyScheduleDb : InSysContext
    {
        public WeeklyScheduleDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tWeeklySchedule> tWeeklySchedules { get; set; }
        public DbQuery<vWeeklySchedule> vWeeklySchedules { get; set; }
        public DbQuery<CountData> countDatas { get; set; }
        public DbQuery<tLookUpData> tLookUpDatas { get; set; }
    }
}
