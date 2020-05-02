using InSys.Helper;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.ITI.Controller
{
    [Route("api/[controller]/[action]")]
    public class DownloadController : CoreController
    {
        public DownloadController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> DownloadFile() => await TaskResult(r =>
        {
            try
            {
                var fldr = Parameter["folder"].ToString();
                var fname = Parameter["fileName"].ToString();

                using (var strg = new Storage.Storage())
                {
                    var Container = strg.Container(fldr);

                    using (var gg = new MemoryStream())
                    {
                        if (!strg.Exists(Container, fname))
                            r.ResultSet = new ReturnSet() { Message = "File not found.", Type = ReturnType.Error };
                        else
                            r.ResultSet = new ReturnSet() { Type = ReturnType.Result };
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "DownloadFile", Helpers.CurrentUser(Session), "InSys.ITI.Controller");
                r.ResultSet = new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FetchFile()
        {
            var fldr = Parameter["folder"].ToString();
            var fname = Parameter["fileName"].ToString();
            var fdisplayname = Parameter["displayName", fname].ToString();
            var contentType = Parameter["contentType", fname.GetContentType(".zip")].ToString();

            using (var strg = new Storage.Storage())
            {
                var Container = strg.Container(fldr);

                using (var gg = new MemoryStream())
                {
                    strg.DownloadToStream(Container, fname, gg);
                    gg.Seek(0, SeekOrigin.Begin);

                    return File(gg.ToByteArray(), contentType, $"{fdisplayname}{Path.GetExtension(fname)}");
                }
            }
        }
    }
}
