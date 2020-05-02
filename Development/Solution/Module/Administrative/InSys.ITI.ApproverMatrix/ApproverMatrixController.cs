using InSys.ITI.Controller;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.ITI.ApproverMatrix
{
    public class ApproverMatrixController : BaseController
    {
        public ApproverMatrixController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadApproverSchema() => await TaskResult(r => {
            var approverMatrix = new ApproverMatrix(Session, Parameter);
            r.ResultSet = approverMatrix.LoadApproverSchema();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadEmployees() => await TaskResult(r => {
            var approverMatrix = new ApproverMatrix(Session, Parameter);
            r.ResultSet = approverMatrix.LoadEmployees();
            return r;
        });
    }
}
