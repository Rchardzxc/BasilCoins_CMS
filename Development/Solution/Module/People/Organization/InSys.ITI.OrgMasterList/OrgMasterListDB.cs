using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.OrgMasterList
{
    public class OrgMasterListDB: InSysContext
    {
        public OrgMasterListDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbQuery<CountData> countData { get; set; }
        public DbSet<tOrgMasterList> tOrgMasterList { get; set; }
        public DbQuery<vOrgType> vOrgType { get; set; }
        public DbQuery<vOrgMasterList> vOrgMasterList { get; set; }
    }
}
