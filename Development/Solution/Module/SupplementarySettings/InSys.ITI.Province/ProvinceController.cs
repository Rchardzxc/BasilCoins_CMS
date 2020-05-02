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

namespace InSys.ITI.Province
{
    [Route("api/[controller]/[action]")]
    public class ProvinceController : BaseController
    {

        public ProvinceController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery) { }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> LoadCity() => await TaskResult(r => {
            string message = "";
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ProvinceDb(Session))
                {
                    var data = db.QueryTable<tCity>("(select * from dbo.tCity where ID_Province = {0})a", to, Parameter["ID_Province"].ToInt32());
                    r.ResultSet = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadCity", Helpers.CurrentUser(Session), "InSys.ITI.Province");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<Result> DeleteCity() => await TaskResult(r => {
            try
            {
                using (var db = new ProvinceDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tCity>("select * from dbo.tCity where ID = {0} and ID > 1", id);
                        if (record != null) db.Remove(record);
                    }
                    db.SaveChanges();
                    r.ResultSet = new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteCity", Helpers.CurrentUser(Session), "InSys.ITI.Province");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        });   

    }
}
