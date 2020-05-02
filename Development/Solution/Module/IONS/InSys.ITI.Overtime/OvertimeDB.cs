using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.ITI.Overtime
{
    public class OvertimeDB : InSysContext
    {
        public OvertimeDB(BrowserSession Session) : base(Session)
        {
        }
        public DbQuery<CountData> CountData { get; set; }
        public DbSet<tOvertime> Overtimes { get; set; }
        public DbQuery<vOvertime> vOvertimes { get; set; }
        public DbQuery<vWorkCredit> WorkCredits { get; set; }
        public DbSet<tWorkCredit> Workcredits { get; set; }
    }
}
