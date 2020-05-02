using InSys.ITI.Controller;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.ITI.OrgMasterList
{
    public class OrgMasterListController : BaseController
    {
        public OrgMasterListController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery) { }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadOrgType() => await TaskResult(r =>
        {
            var OrgMasterList = new OrgMasterList(Session, Parameter);
            r.ResultSet = OrgMasterList.LoadOrgType();
            return r;
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadOrgMasterList() => await TaskResult(r =>
        {
            var OrgMasterList = new OrgMasterList(Session, Parameter);
            r.ResultSet = OrgMasterList.LoadOrgMasterList();
            return r;
        });
    }
}
