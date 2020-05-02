using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.DashBoard
{
    public class OBDb: InSysContext
    {
        public OBDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tOB> tOB { get; set; }
        public DbSet<tOB_Detail> tOB_Detail { get; set; }
        public DbQuery<CountData> CountData { get; set; }
        public DbQuery<vOB> vOB { get; set; }
    }
}
