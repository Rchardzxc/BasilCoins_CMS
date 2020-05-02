using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.CostCenter
{
    public class CostCenterDB:InSysContext
    {
        public CostCenterDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbQuery<CountData> countDatas { get; set; }
        public DbSet<tCostCenter> tCostCenter { get; set; }
    }
}
