using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Common
{
    public class CommonHelperDb : InSysContext
    {
        public CommonHelperDb(BrowserSession session) : base(session)
        {
            this.SessionContext = session;
        }

        public DbQuery<CountData> countData { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
        public DbQuery<vApprovalHistory> vApprovalHistory { get; set; }
        public DbSet<tApprovalHistory> tApprovalHistory { get; set; }
    }
}
