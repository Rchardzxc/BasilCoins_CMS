using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.LeaveParameter
{
    public class LeaveParameterDb: InSysContext
    {
        public DbQuery<CountData> countData { get; set; }
        public LeaveParameterDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbSet<tLeaveParameter> tLeaveParameter { get; set; }
        public DbQuery<vLeaveParameterList> vLeaveParameterList { get; set; }
        public DbSet<tLeaveParameterItem> tLeaveParameterItem { get; set; }
        public DbSet<tLeaveParameterItemReferenceDate> tLeaveParameterItemReferenceDate { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
        public DbQuery<StringReturn> StringReturn { get; set; }
    }
}
