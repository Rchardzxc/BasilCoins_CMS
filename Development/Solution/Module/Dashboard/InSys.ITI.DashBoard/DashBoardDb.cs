using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.DashBoard
{
    public class DashBoardDb: InSysContext
    {
        public DashBoardDb(BrowserSession Session) : base(Session)
        {
            //this.SessionContext = Session;
        }
        public DbQuery<CountData> countData { get; set; }
        public DbQuery<vAnnouncements> vAnnouncements { get; set; }
        public DbQuery<vUpcommingEvents> vUpcommingEvents { get; set; }
        public DbQuery<vEmployeeLeaveDetails> vEmployeeLeaveDetails { get; set; }
        public DbQuery<vEmployeeTimeKeepingSummary> vEmployeeTimeKeepingSummary { get; set; }
        public DbQuery<vFilingTypeCount> vFilingTypeCount { get; set; }

    }
}
