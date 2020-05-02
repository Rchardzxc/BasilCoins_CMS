using InSys.Context;
using InSys.Helper;
using InSys.HRMS.Models;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.EmployeeRecord
{
    public class EmployeeRecordDb : InSysContext
    {
        public EmployeeRecordDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbQuery<tLookUpData> tLookUpDatas { get; set; }
        public DbQuery<CountData> countDatas { get; set; }
        //tables
        public DbSet<tPersona> tPersonas { get; set; }
        public DbSet<tPersonaAddress> tPersonaAddresses { get; set; }
        public DbSet<tPersonaEmployment> tPersonaEmployments { get; set; }
        public DbSet<tPersonaEducationalBackGround> tPersonaEducationalBackGrounds { get; set; }
        public DbSet<tPersonaCharacterReference> tPersonaCharacterReferences { get; set; }
        public DbSet<tPersonaLicensesAndCertificates> tPersonaLicensesAndCertificates { get; set; }
        public DbSet<tEmployee> tEmployees { get; set; }
        //views
        public DbQuery<vPersonaEducationalBackGround> vPersonaEducationalBackGrounds { get; set; }
        public DbQuery<vPersonaAddress> vPersonaAddresses { get; set; }
        public DbQuery<vEmployee> vEmployees { get; set; }
        public DbQuery<vPersonaLookUp> vPersonaLookUp { get; set; }
        public DbQuery<vEmployeeRecordList> vEmployeeRecordList { get; set; }
        public DbQuery<vPersona> vPersonas { get; set; }
        public DbQuery<vWeeklySchedule> vWeeklySchedules { get; set; }
        public DbQuery<StringReturn> StringReturn { get; set; }
    }
}
