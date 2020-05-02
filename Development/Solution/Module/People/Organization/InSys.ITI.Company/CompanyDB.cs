using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Company
{
    public class CompanyDB : InSysContext
    {
        public CompanyDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        //DBSet
        public DbSet<tCompany> tCompany { get; set; }
        public DbSet<tOrg> tOrg { get; set; }
        public DbSet<tCompanyOrgType> tCompanyOrgType {get; set;}
        public DbSet<tOrgType> tOrgType { get; set; }
        public DbSet<tJobClassApproverCandidates> tJobClassApproverCandidates { get; set; }

        //DBQuery
        public DbQuery<fOrg> fOrg { get; set; }
        public DbQuery<vJobClassApproverCandidates> vJobClassApproverCandidates { get; set; }
        public DbQuery<vCompany> vCompany { get; set; }
        public DbQuery<CountData> countData { get; set; }
        public DbQuery<vOrg> vOrg { get; set; }
        public DbQuery<vOrgMasterList> vOrgMasterList{ get; set; }
        public DbQuery<vCompanyOrgType> vCompanyOrgType { get; set; }
    }
}
