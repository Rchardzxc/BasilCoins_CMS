using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.OrbitLeave
{
    public class OrbitLeaveDb : InSysContext
    {
        public OrbitLeaveDb(BrowserSession session) : base(session)
        {
            this.SessionContext = session;
        }

        public DbQuery<CountData> countData { get; set; }
        public DbQuery<vOrbitLeaveList> vOrbitLeaveList { get; set; }
        public DbSet<tLeave> tLeave { get; set; }
        public DbSet<tLeave_Detail> tLeave_Detail { get; set; }
    }
}
