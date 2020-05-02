using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.LeaveCredit
{
    public class LeaveCreditDb : InSysContext
    {
        public LeaveCreditDb(BrowserSession session) : base(session)
        {
            this.SessionContext = session;
        }

        public DbQuery<CountData> countData { get; set; }
        public DbSet<tLeaveCredit> tLeaveCredit { get; set; }
        public DbQuery<vLeaveCreditList> vLeaveCreditList { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
        public DbSet<tLeaveCredit_Detail> tLeaveCredit_Detail { get; set; }
        public DbQuery<vLeaveCredit_Detail> vLeaveCredit_Detail { get; set; }


    }
}
