using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Leave
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

        public DbQuery<vLeave_Detail> vLeave_Detail { get; set; }

        public DbQuery<CountData> CountData { get; set; }
        public DbSet<tPayrollItem> tPayrollItem { get; set; }
        public DbSet<tFilingStatus> tFilingStatus { get; set; }
        public DbQuery<vLeaveDayType> vLeaveDayType { get; set; }
        public DbQuery<vLeaveCredit> vLeaveCredit { get; set; }
        public DbQuery<vEmployeeRestDay> vEmployeeRestDay { get; set; }
        public DbSet<tPersona> tPersona { get; set; }
        public DbSet<tEmployee> tEmployee { get; set; }
    }
}
