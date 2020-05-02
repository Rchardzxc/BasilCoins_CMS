using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.NatureOfRequest
{
    public class NatureOfRequestDb: InSysContext
    {
        public NatureOfRequestDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbQuery<CountData> countDatas { get; set; }
        public DbSet<tNatureOfRequests> tNatureOfRequests { get; set; }
    }
}
