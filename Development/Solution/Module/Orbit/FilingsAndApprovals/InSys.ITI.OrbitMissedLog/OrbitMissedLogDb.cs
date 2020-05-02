using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.OrbitMissedLog
{
    public class OrbitMissedLogDb : InSysContext
    {
        public OrbitMissedLogDb(BrowserSession session) : base(session)
        {
            this.SessionContext = session;
        }

        public DbQuery<CountData> countData { get; set; }
        public DbQuery<vOrbitMissedLogList> vOrbitMissedLogList { get; set; }
        public DbSet<tMissedLog> tMissedLog { get; set; }
        public DbSet<tMissedLog_Detail> tMissedLog_Detail { get; set; }
    }
}
