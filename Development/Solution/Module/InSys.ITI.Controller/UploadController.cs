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
    [EndRequestCompressor]
    public class UploadController: CoreController
    {
        public UploadController(IHostingEnvironment hostingEnvironment, IAntiforgery _antiForgery) : base(hostingEnvironment, _antiForgery)
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> UploadFile() => await TaskResult(r =>
        {
            try
            {
                var fldr = Ctx.Request.Form["folder"].ToString() ?? "Files";
                var fname = Guid.NewGuid().ToString();
                var gh = new List<object>();

                using (var strg = new Storage.Storage())
                {
                    var Container = strg.Container(fldr);

                    foreach (var fle in Ctx.Request.Form.Files)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            var tmp = Path.GetFileNameWithoutExtension(fle.FileName).Replace(".CDITI.", "").Replace(",", "").Replace(" ", "");
                            fname = $"{tmp}.CDITI.{ Guid.NewGuid().ToString().Replace("-", "") }{ Path.GetExtension(fle.FileName) }";
                            while (strg.Exists(Container, fname))
                            {
                                fname = $"{tmp}.CDITI.{ Guid.NewGuid().ToString().Replace("-", "") }{ Path.GetExtension(fle.FileName) }";
                            }
                            fle.CopyTo(memoryStream);

                            memoryStream.Seek(0, SeekOrigin.Begin);

                            strg.Upload(Container, fname, memoryStream);

                            gh.Add(new { FileName = fname, FileSize = memoryStream.Length });
                        }
                    }
                    r.ResultSet = new ReturnSet() { Data = gh, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "UploadFile", Helpers.CurrentUser(Session), "InSys.ITI.Controller");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
            return r;
        });
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<Result> CheckFiles() => await TaskResult(r =>
        {
            try
            {
                var fldr = Parameter["folder"].ToString() ?? "Files";
                var files = Parameter["files"].ToObject<List<string>>();
                var gh = new List<object>();

                using (var strg = new Storage.Storage())
                {
                    var Container = strg.Container(fldr);

                    foreach (var fle in files)
                    {
                        using (var ms = new MemoryStream())
                        {
                            if (strg.Exists(Container, fle))
                            {
                                strg.DownloadToStream(Container, fle, ms);
                                ms.Seek(0, SeekOrigin.Begin);
                                gh.Add(new { OrigFile = fle, FileName = fle.Split(".CDITI.")[0], FileSize = ms.Length, FileExt = Path.GetExtension(fle).Replace(".", "") });
                                ms.Close();
                            }
                            else
                            {
                                //gh.Add(new { OrigFile = "", FileName = "File does not exist.", FileSize = 0, FileExt = "" });
                                gh.Add(new { OrigFile = fle, FileName = fle.Split(".CDITI.")[0], FileSize = 0, FileExt = Path.GetExtension(fle).Replace(".", "") });
                            }
                        }
                    }
                }
                r.ResultSet = new ReturnSet() { Data = gh, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "CheckFiles", Helpers.CurrentUser(Session), "InSys.ITI.Controller");
                r.ResultSet = new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
            return r;
        });
    }
}
