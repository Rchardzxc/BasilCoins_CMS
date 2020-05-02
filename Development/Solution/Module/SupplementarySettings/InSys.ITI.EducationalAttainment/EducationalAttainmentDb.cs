using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.EducationalAttainment
{
    public class EducationalAttainmentDb : InSysContext
    {
        public EducationalAttainmentDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbQuery<CountData> CountData { get; set; }
        public DbSet<tEducationalAttainment> tEducationalAttainment { get; set; }
    }
}
