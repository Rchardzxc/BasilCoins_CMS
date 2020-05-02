using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.LeaveType
{
    public class LeaveTypeDb: InSysContext
    {
        public LeaveTypeDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbQuery<CountData> countData { get; set; }
        public DbSet<tLeaveType> tLeaveType { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
        public DbQuery<vLeaveType> vLeaveType { get; set; }

    }
}
