using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Citizenship
{
    public class CitizenshipDb : InSysContext
    {
        public CitizenshipDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbQuery<CountData> CountData { get; set; }
        public DbSet<tCitizenship> tCitizenship { get; set; }
    }
}
