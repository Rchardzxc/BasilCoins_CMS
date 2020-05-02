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

namespace InSys.ITI.EmployeeRecord
{
    [Route("api/[controller]/[action]")]
    public class EmployeeRecordController: BaseController
    {
        public EmployeeRecordController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery) { }

        public virtual async Task<Result> LoadPersonaWithoutEmployment() => await TaskResult(r => {
            try
            {
                //provide lookup
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<vPersonaLookUp>("SELECT p.ID, p.Name, p.LastName, p.FirstName "
                                                    + " FROM tPersona p LEFT OUTER JOIN tEmployee e ON e.ID_Persona = p.ID "
                                                    + " WHERE e.ID IS NULL").ToList();
                    r.ResultSet = new ReturnSet() { Data = new { Total = data.Count, Rows = data }, Type = ReturnType.Result };
                }
            } catch (Exception ex) {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "GetManPowerPlanDetail", "Internal", "InSys.ITI.EmployeeRecord");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
            return r;
        });

        public virtual async Task<Result> LoadPersonaSchema() => await TaskResult(r => {
            try {
                string Schema = Parameter["LookupName"].ToString();
                r.ResultSet = new ReturnSet()
                {
                    Data = new
                    {
                        Schema = Helpers.GetSchema(Schema)
                    },
                    Type = ReturnType.Result
                };
            } catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadPersonaSchema", Helpers.CurrentUser(Session), "InSys.ITI.EmployeeRecord");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Result };
            }
            return r;
        });
    }
}
