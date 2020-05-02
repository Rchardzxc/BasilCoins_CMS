using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.ApproverMatrix
{
    public class ApproverMatrixDB : InSysContext
    {
        public ApproverMatrixDB(BrowserSession Session) : base(Session)
        {
        }
        public DbQuery<CountData> countdata { get; set; }
        public DbSet<tApprover> tApprovers { get; set; }
        public DbQuery<vApprover_Module> vApprover_Module { get; set; }
        public DbQuery<vApprover> vApprover { get; set; }
        public DbQuery<vApprover_Default> vApprover_Default { get; set; }
        public DbQuery<EmployeeList> EmployeeList { get; set; }
        public DbSet<tApprover_Default> tApprover_Default { get; set; }
        public DbSet<tApprover_Module> tApprover_Module { get; set; }

        public DbQuery<vApprover_Employees> vApprover_Employees { get; set; }
        public DbSet<tApprover_Employees> tApprover_Employee { get; set; }
        public DbSet<tApprover_Employee_Module> tApprover_Employee_Module { get; set;}
        public DbSet<tApprover_Employee_Module_Approvers> tApprover_Employee_Module_Approvers { get; set; }
        public DbQuery<vCompanyOrgType> vCompanyOrgType { get; set; }
        public DbQuery<vApprover_OrgList> vApprover_OrgList { get; set; }
        public DbSet<tOrg> tOrg { get; set; }

        public DbQuery<vFilingModuleApprovers> vFilingModuleApprovers { get; set; }
        public DbSet<tFilingModules> tFilingModules { get; set; }
        public DbSet<tApprover> tApprover { get; set; }
        public DbQuery<vFilingModules> vFilingModules { get; set; }
        public DbQuery<vEmployeeRecordList> vEmployeeRecordList { get; set; }
        public DbSet<tFilingModuleApprovers> tFilingModuleApprovers { get; set; }
        public DbSet<tFIlingModuleEmployees> tFIlingModuleEmployees { get; set; }
    }
}
