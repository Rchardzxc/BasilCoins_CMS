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

namespace InSys.ITI.Company
{
    public class CompanyController : BaseController
    {
        public CompanyController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery) { }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> GetChildOrg() => await TaskResult(r =>
        {
            var company = new Company(Session,Parameter);
            r.ResultSet = company.GetChildOrg();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadCompanyNewDefaultOrg() => await TaskResult(r =>
        {
            var company = new Company(Session, Parameter);
            r.ResultSet = company.LoadCompanyNewDefaultOrg();
            return r;
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadCompanyOrg() => await TaskResult(r =>
        {
            var company = new Company(Session, Parameter);
            r.ResultSet = company.LoadCompanyOrg();
            return r;
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> SaveHeirarchy() => await TaskResult(r =>
        {
            var company = new Company(Session, Parameter);
            r.ResultSet = company.SaveHeirarchy();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> GetLastHeirarchyLevel() => await TaskResult(r =>
        {
            var company = new Company(Session, Parameter);
            r.ResultSet = company.GetLastHeirarchyLevel();
            return r;
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> DeleteSelectedHeirarch() => await TaskResult(r =>
        {
            var company = new Company(Session, Parameter);
            r.ResultSet = company.DeleteSelectedHeirarch();
            return r;
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> SaveCompanyOrgType() => await TaskResult(r =>
        {
            var company = new Company(Session, Parameter);
            r.ResultSet = company.SaveCompanyOrgType();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> DeleteCompanyOrgLevel() => await TaskResult(r =>
        {
            var company = new Company(Session, Parameter);
            r.ResultSet = company.DeleteCompanyOrgLevel();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> LoadJobClass() => await TaskResult(r =>
        {
            var company = new Company(Session, Parameter);
            r.ResultSet = company.LoadJobClass();
            return r;
        });
    }
}
