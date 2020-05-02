using System;
using System.Collections.Generic;
using System.Text;
using InSys.ITI.Controller;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Antiforgery;

namespace InSys.ITI.WeeklySchedule
{
    public class WeeklyScheduleController : BaseController
    {
        public WeeklyScheduleController(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment, Microsoft.AspNetCore.Antiforgery.IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }
    }
}
