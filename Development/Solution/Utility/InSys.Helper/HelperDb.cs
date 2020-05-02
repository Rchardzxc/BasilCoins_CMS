using InSys.Context;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.Helper
{
    public class HelperDb : InSysContext
    {
        public HelperDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbSet<tUsers> tUsers { get; set; }
        public DbQuery<CountData> countDatas { get; set; }
        public DbQuery<TableSchema> tableSchemas { get; set; }
        public DbSet<tCompanyRefNum> tCompanyRefNum { get; set; }
        public DbSet<tCompany> tCompanies { get; set; }
        public DbSet<tCompanyAccessRights> tCompanyAccessRights { get; set; }

    }
}
