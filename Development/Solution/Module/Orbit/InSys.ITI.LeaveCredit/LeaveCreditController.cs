using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.ITI.LeaveCredit
{
    public class LeaveCreditController: BaseController
    {
        public LeaveCreditController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }

        public virtual async Task<Result> GetLeaveCreditDetails() => await TaskResult(r => {
            try {
                var ID_LeaveCredit = Parameter["ID_LeaveCredit"].ToInt32();
                using (var db = new LeaveCreditDb(Session)){
                    var record = db.ExecQuery<vLeaveCredit_Detail>(@"SELECT ld.Id, ld.ID_LeaveCredit, ld.ID_Employee, ld.ID_LeaveType, ld.EffectiveDate, ld.Value,
	                                                                    ld.Remarks, ld.CreatedAt, ld.ID_CreatedBy, ld.ModifiedAt, ld.ID_ModifiedBy,
	                                                                    e.EmployeeName, lt.Name AS LeaveType
                                                                    FROM tLeaveCredit_Detail ld
	                                                                    INNER JOIN vEmployees e ON e.ID_Employee = ld.ID_Employee
	                                                                    INNER JOIN tLeaveType lt ON lt.ID = ld.ID_LeaveType
                                                                    WHERE ld.ID_LeaveCredit= {0}", ID_LeaveCredit).ToList();
                    //var LeaveCreditSchema = Helpers.GetSchema("tLeaveCredit_Detail");

                    r.ResultSet = new ReturnSet() { Data = new { Data = record }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex) {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "GetLeaveCreditDetails", Helpers.CurrentUser(Session), "InSys.ITI.LeaveCredit");
                r.ResultSet = new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }

            return r;
        
        });
    }
}
