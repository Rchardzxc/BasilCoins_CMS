using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.DashBoard
{
    public class LeaveDb: InSysContext
    {
        public LeaveDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tLeave> tLeave { get; set; }
        public DbQuery<vLeave> vLeave { get; set; }
        public DbSet<tLeave_Detail> tLeave_Detail { get; set; }
        public DbQuery<CountData> CountData { get; set; }
        public DbSet<tFilingStatus> tFilingStatuses { get; set; }
        public DbSet<tPayrollItem> tPayrollItems { get; set; }
    }
}
