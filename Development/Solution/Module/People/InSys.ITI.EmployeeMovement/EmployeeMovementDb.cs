using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.EmployeeMovement
{
    public class EmployeeMovementDb : InSysContext
    {
        public EmployeeMovementDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        //public DbSet<tMovement> tMovements { get; set; }
        //public DbSet<tMovement_Employee> tMovement_Employees { get; set; }
        //public DbSet<tMovement_Employee_Detail> tMovement_Employee_Details { get; set; }
        //public DbQuery<vMovement> vMovements { get; set; }
        //public DbQuery<vMovement_Employee> vMovement_Employees { get; set; }
        //public DbQuery<vMovement_Employee_Detail> vMovement_Employee_Details { get; set; }

        public DbQuery<CountData> countDatas { get; set; }
        public DbSet<tEmployee> tEmployees { get; set; }
        public DbQuery<tLookUpData> tLookUpDatas { get; set; }
        public DbQuery<DecimalReturn> decimalReturns { get; set; }
        public DbQuery<IntReturn> intReturns { get; set; }
        public DbSet<tParameter> tParameters { get; set; }
        public DbQuery<DateTimeReturn> dateTimeReturns { get; set; }
        public DbQuery<StringReturn> StringReturn { get; set; }
        public DbQuery<vEmployeeMovementOldValues> vEmployeeMovementOldValues { get; set; }

        public DbSet<tEmployeeMovementFields> tEmployeeMovementFields { get; set; }
        public DbQuery<vEmployeeMovementField> vEmployeeMovementField { get; set; }
        public DbSet<tEmployeeMovement> tEmployeeMovements { get; set; }
        public DbSet<tEmployeeMovementDetails> tEmployeeMovementDetails { get; set; }
        public DbQuery<vEmployeeMovementList> vEmployeeMovementList { get; set; }
        public DbQuery<vEmployeeMovementForm> vEmployeeMovementForm { get; set; }
        public DbQuery<vEmployeeMovementFormDetail> vEmployeeMovementFormDetail { get; set; }
    }
}
