using InSys.Context;
using InSys.Helper;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;


namespace InSys.ITI.ManPowerPlan
{
    public class ManPowerPlanDb: InSysContext
    {
        public ManPowerPlanDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbQuery<CountData> CountData { get; set; }
        public DbQuery<vManPowerPlans> vManPowerPlans { get; set; }
        public DbQuery<vManPowerPlanDetailsValidateResult> vManPowerPlanDetailsValidateResult { get; set; }

        public DbSet<tManPowerPlans> tManPowerPlans { get; set; }
        public DbSet<tManPowerPlanDetails> tManPowerPlanDetails { get; set; }
    }
}
