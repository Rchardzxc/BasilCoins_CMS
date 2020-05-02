using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.MRF
{
    public class MRFDb: InSysContext
    {
        public MRFDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbQuery<CountData> CountData { get; set; }
        public DbQuery<vMRF> vMRF { get; set; }
        public DbQuery<vMRF2> vMRF2 { get; set; }
        public DbQuery<MRFEmployeeName> MRFEmployeeName { get; set; }
        public DbQuery<vManPowerPlanDetailsIntoMRF> vManPowerPlanDetailsIntoMRF { get; set; }
        public DbQuery<vApprovalHistory> vApprovalHistory { get; set; }
        public DbQuery<vApprovalHistory> vMRFApproval { get; set; }

        public DbSet<tMRF> tMRF { get; set; }
        public DbSet<tApprovalHistory> tApprovalHistory { get; set; }
        public DbQuery<tApproverMatrixPerLevel> tApproverMatrixPerLevel { get; set; }
        public DbQuery<IntReturn> IntReturn { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
        public DbSet<tCompanyRefNum> tCompanyRefNum { get; set; }



    }
}
