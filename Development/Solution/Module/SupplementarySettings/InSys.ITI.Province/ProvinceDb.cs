using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace InSys.ITI.Province
{
    public class ProvinceDb: InSysContext
    {
        public ProvinceDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbQuery<CountData> CountData { get; set; }
        public DbSet<tProvince_old> tBloodType { get; set; }
        public DbSet<tCity> tCity { get; set; }
    }
}
