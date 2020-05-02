using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.ITI.LeaveParameter
{
    [Route("api/[controller]/[action]")]
    public class LeaveParameterController: BaseController
    {
        public LeaveParameterController(IHostingEnvironment env, IAntiforgery _antiForgery) : base(env, _antiForgery)
        { }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> GetLeaveParameterDetails() => await TaskResult(r => {

            var ID_LeaveParameter = Parameter["ID_LeaveParameter"].ToObject<int>();
            try
            {
                using (var db = new LeaveParameterDb(Session))
                {
                    var record = db.ExecQuery<tLeaveParameterItem>("SELECT * FROM dbo.tLeaveParameterItem lpi"
                                        + " WHERE lpi.ID_LeaveParameter = {0}", ID_LeaveParameter).ToList();
                    r.ResultSet = new ReturnSet() { Data = record, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                r.ResultSet = new ReturnSet() { Message = ex.Message, Type = ReturnType.Error };
            }

            return r;

        });
    }
}
