using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.MissedLog
{
    public class MissedLogDb: InSysContext
    {
        public MissedLogDb(BrowserSession Session): base(Session) {
            this.SessionContext = Session;
        }
        public DbSet<tMissedLog> tMissedLog { get; set; }
        public DbSet<tLogType> tLogType { get; set; }
        public DbSet<tMissedLog_Detail> tMissedLogDetail { get; set; }
        public DbQuery<CountData> CountData { get; set; }
        public DbQuery<vMissedLog> vMissedLog { get; set; }
    }
}
