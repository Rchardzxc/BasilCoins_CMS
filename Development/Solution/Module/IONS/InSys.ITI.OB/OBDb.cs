using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.OB
{
    public class OBDb: InSysContext
    {
        public OBDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tOB> tOBs { get; set; }
        public DbSet<tOB_Detail> tOBs_Detail { get; set; }
        public DbQuery<CountData> CountData { get; set; }
        public DbQuery<vOB> vOBs { get; set; }
        public DbQuery<tLookUpData> tLookUpDatas { get; set; }
    }
}
