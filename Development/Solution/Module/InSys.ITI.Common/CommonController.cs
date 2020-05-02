using InSys.Helper;
using InSys.ITI.Controller;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.ITI.Common
{
    public class CommonController : BaseController
    {
        public CommonController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<Result> LoadLookup() => await TaskResult(r =>
        {
            ReturnSet res = new ReturnSet();
            try
            {
                string lookupName = Parameter["LookupName"].ToString().ToLower();
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                res = CommonHelper.GetLookUp(lookupName, to, Session);

                r.ResultSet = res;
            }
            catch (Exception ex) {
                r.ResultSet = new ReturnSet() { Message = ex.Message, Type = ReturnType.Error };
            }
            
            return r;
        });


        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<Result> LoadApprovalHistory() => await TaskResult(r =>
        {
            string MenuCode = Parameter["MenuCode"].ToString();
            int ID_Reference = Parameter["ID"].ToInt32();
            r.ResultSet = CommonHelper.GetApprovalHistory(MenuCode, ID_Reference, Session);
            return r;
        });

    }
}
