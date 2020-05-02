using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.City
{
    public class CityDb : InSysContext
    {
        public CityDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbQuery<CountData> CountData { get; set; }
        public DbSet<tCity> tCity { get; set; }
        public DbSet<tBarangay> tBarangays { get; set; }
        
    }
}
