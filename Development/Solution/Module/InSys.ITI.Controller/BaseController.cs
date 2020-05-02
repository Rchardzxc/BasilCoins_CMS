using InSys.Helper;
using InSys.ITI.Menu;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using z.Data;
using System.Text.RegularExpressions;
using InSys.Storage;
using Microsoft.AspNetCore.Antiforgery;

namespace InSys.ITI.Controller
{
    [Route("api/[controller]/[action]")]
    [EndRequestCompressor]
    public class BaseController: CoreController
    {
        public BaseController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<Result> ReloadSystem() => TaskResult(r =>
        {

            try
            {
                Config.bundleUpdateCount += 1;
                var insysBundle = new InSysBundles(hostingEnvironment);
                insysBundle.Bundle(Config.bundle, InSysBundles.InSysSystem.HRMS);
                MenuCollection.Init();
                SystemSettings.LoadSystemSettings();

                r.ResultSet = new ReturnSet() { Message = "Success!", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "ReloadSystem", Helpers.CurrentUser(Session), "InSys.Controller");
                r.ResultSet = new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
            return r;
        });

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public Task<Result> LoadImage() => TaskResult(r =>
        {
            try
            {
                var strg = new Storage.Storage();

                var cntr = strg.Container(Parameter["Container", "Images"].ToString());
                var tmpfile = Parameter["ImgFile", "some.png"].ToString();

                LocalBlob blob;

                if (Parameter["Path"] != null)
                {
                    var dr = cntr.GetDirectoryReference(strg.ToURLSlug(Parameter["Path"].ToString()));
                    blob = strg.ListBlob(dr).Where(x => x.Name == strg.ToURLSlug(tmpfile)).SingleOrDefault(); //dr.Prefix +
                }
                else
                {
                    blob = strg.ListBlob(cntr).Where(x => x.Name == strg.ToURLSlug(tmpfile)).SingleOrDefault();
                }

                if (blob != null)
                {
                    var sasToken = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
                    {
                        Permissions = SharedAccessBlobPermissions.Read,
                        SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7),

                    }, new SharedAccessBlobHeaders { ContentType = tmpfile.GetContentType() });
                    r.ResultSet = new ReturnSet() { Data = blob.AbsoluteUri + sasToken, Type = ReturnType.Result };
                }
                else
                {
                    r.ResultSet = new ReturnSet() { Data = "", Type = ReturnType.Result };
                }
            }catch(Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "LoadImage", Helpers.CurrentUser(Session), "InSys.ITI.Controller");
                r.ResultSet = new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
            return r;
        });


        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadList() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadList();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadForm() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadForm();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> SaveForm() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.SaveForm();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> DeleteRecord() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.DeleteRecord();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> DeleteDetail() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.DeleteDetail();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> Approve() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.Approve();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> Disapprove() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.Disapprove();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> Cancel() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.Cancel();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> Submit() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.Submit();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadLookup() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadLookup();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadDetail() => await TaskResult(r =>
        {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadDetail();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadHistory() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadHistory();
            return r;
        
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> Post() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.Post();
            return r;

        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> UnPost() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.UnPost();
            return r;

        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> ExecuteAction() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.ExecuteAction();
            return r;

        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> Lock() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.Lock();
            return r;
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> UnLock() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.UnLock();
            return r;
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadApprovalDetail() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadApprovalDetail();
            return r;
        });
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadFilingForm() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadFilingForm();
            return r;
        });


        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadPDF() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadPDF();
            return r;

        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadApprovalHistory() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadApprovalHistory();
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadExcel() => await TaskResult(r => {
            var menuObj = ClassModule.Initialize(Session, Parameter);
            r.ResultSet = menuObj.LoadExcel();
            return r;

        });
    }
}
