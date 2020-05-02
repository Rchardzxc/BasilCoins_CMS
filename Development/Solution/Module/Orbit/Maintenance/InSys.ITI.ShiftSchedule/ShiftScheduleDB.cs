using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.ShiftSchedule
{
    public class ShiftScheduleDB : InSysContext
    {
        public ShiftScheduleDB(BrowserSession Session) : base(Session)
        {
        }
        public DbQuery<CountData> countData { get; set; }
        public DbSet<tShiftSchedule> tShiftSchedules { get; set; }
        public DbSet<tShiftSchedule_Detail> tShiftSchedule_Details { get; set; }
        public DbQuery<vShiftSchedule> vShiftSchedules { get; set; }
        public DbQuery<vShiftSchedule_Detail> vShiftSchedule_Details { get; set; }
        public DbQuery<vCompany> vCompanies { get; set; }
        public DbQuery<vHourType> vHourTypes { get; set; }
    }
}
