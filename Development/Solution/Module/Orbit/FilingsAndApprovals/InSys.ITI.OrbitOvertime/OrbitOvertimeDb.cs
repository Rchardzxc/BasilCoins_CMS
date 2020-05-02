using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.OrbitOvertime
{
    public class OrbitOvertimeDb: InSysContext
    {
        public OrbitOvertimeDb(BrowserSession session) : base(session)
        {
            this.SessionContext = session;
        }

        public DbQuery<StringReturn> StringReturn { get; set; }
        public DbQuery<CountData> countData { get; set; }
        public DbQuery<vOrbitOvertimeList> vOrbitOvertimeList { get; set; }
        public DbSet<tOvertime> tOvertime { get; set; }
    }
}
