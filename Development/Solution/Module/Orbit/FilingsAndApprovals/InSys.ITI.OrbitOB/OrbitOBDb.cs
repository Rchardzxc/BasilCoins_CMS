using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.OrbitOB
{
    public class OrbitOBDb : InSysContext
    {
        public OrbitOBDb(BrowserSession session) : base(session)
        {
            this.SessionContext = session;
        }

        public DbQuery<CountData> countData { get; set; }
        public DbQuery<vOrbitOBList> vOrbitOBList { get; set; }
        public DbSet<tOB> tOB { get; set; }
        public DbSet<tOB_Detail> tOB_Detail { get; set; }

    }
}
