using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.ResumeBankAndSearch
{
    public class ResumeBankAndSearchDB: InSysContext
    {
        public ResumeBankAndSearchDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbQuery<CountData> countDatas { get; set; }
        public DbQuery<vApplicationForm> vApplicationForm { get; set; }
        public DbSet<tApplicationFormExamination> tApplicationFormExamination { get; set; }
        public DbSet<tApplicationFormInterviews> tApplicationFormInterviews { get; set; }
        //public DbSet<tPersonaAddress> tPersonaAddress { get; set; }
        public DbSet<tApplicationForm> tApplicationForm { get; set; }
        public DbSet<tEmployee> tEmployee { get; set; }
        public DbSet<tPersona> tPersona { get; set; }
        public DbSet<tMRF> tMRF { get; set; }
        public DbSet<tApprover_Employee_Module> tApprover_Employee_Module { get; set; }
        public DbSet<tEmployee_JobOffer> tEmployee_JobOffer { get; set; }

        public DbQuery<tEndorseLookUpData> tEndorseLookUpData { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
        public DbQuery<RecruitmentStatusCount> RecruitmentStatusCount { get; set; }
        public DbQuery<vApplicationForm2> vApplicationForm2 { get; set; }
        public DbQuery<vApplicationForm_Address> vApplicationForm_Address { get; set; }
        public DbQuery<vApplicationForm_CharacterReference> vApplicationForm_CharacterReference { get; set; }
        public DbQuery<vApplicationForm_Educational> vApplicationForm_Educational { get; set; }
        public DbQuery<vApplicationForm_Employment> vApplicationForm_Employment { get; set; }
        public DbQuery<vApplicationForm_LicensesAndCertificates> vApplicationForm_LicensesAndCertificates { get; set; }
        public DbQuery<TableSchema> TableSchema { get; set; }
        public DbQuery<ApproverEmployeeModuleData> ApproverEmployeeModuleData { get; set; }
        public DbQuery<vApplicationFormInterviews> vApplicationFormInterviews { get; set; }
        public DbQuery<vApplicationFormExamination> vApplicationFormExamination { get; set; }
        public DbQuery<vEmployee_JobOffer> vEmployee_JobOffer { get; set; }
        public DbQuery<MRFLookup> MRFLookup { get; set; }
        public DbQuery<IntReturn> IntReturn { get; set; }
        public DbSet<tApprover> tApprover { get; set; }
        //public DbQuery<vDesignation> vDesignation { get; set; }

    }
}
