using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using z.Data;

namespace InSys.ITI.InitialMigration
{
    public class InitialMigration : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new InitialMigration(_Session, _Parameter);
        }
        public InitialMigration() { }
        public InitialMigration(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();

                using (var db = new InitialMigrationDb(Session))
                {
                    var data = db.QueryTable<vEtfMigration>($"({Helpers.GetSqlQuery("vEtfMigration")})a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }

        }

        public override ReturnSet Save()
        {
            int RecordId = 0;
            var etf = Parameter["Data"].ToObject<tEtfMigration>();
            int etfRequestId = etf.ID;

            try
            {
                using (var db = new InitialMigrationDb(Session))
                {
                    //New
                    if (etfRequestId == 0)
                    {
                        etf.DateTimeCreated = DateTime.Now;
                        etf.ID_UserCreatedBy = Session.ID_User;
                        db.Add(etf);
                        db.SaveChanges();
                        RecordId = etf.ID;
                        using (var strg = new Storage.Storage())
                        {
                            string fldr = "ETF/Unprocess";
                            var Container = strg.Container(fldr);
                            var newContainer = strg.Container("ETF/New");
                            using (var memoryStream = new MemoryStream())
                            {
                                if (strg.Exists(Container, etf.FileName))
                                {
                                    strg.DownloadToStream(Container, etf.FileName, memoryStream);
                                    memoryStream.Seek(0, SeekOrigin.Begin);

                                    strg.Upload(newContainer, etf.FileName, memoryStream);
                                    strg.Delete(Container, etf.FileName);
                                }
                            }
                        }
                    }
                    //Edit
                    else
                    {
                        var etfToUpdate = db.Single<tEtfMigration>("select * from dbo.tEtfMigration where ID = {0}", etfRequestId);

                        if (etfToUpdate != null)
                        {
                            etf.DateTimeCreated = DateTime.Now;
                            db.Update(etf);
                            db.SaveChanges();
                            RecordId = etf.ID;
                            using(var strg = new Storage.Storage())
                            {
                                string fldr = "ETF/Unprocess";
                                var Container = strg.Container(fldr);
                                var newContainer = strg.Container("ETF/New");
                                using (var memoryStream = new MemoryStream())
                                {
                                    if(strg.Exists(Container, etf.FileName))
                                    {
                                        strg.DownloadToStream(Container, etf.FileName, memoryStream);
                                        memoryStream.Seek(0, SeekOrigin.Begin);

                                        strg.Upload(newContainer, etf.FileName, memoryStream);
                                        strg.Delete(Container, etf.FileName);
                                    }
                                }
                            }
                        }
                        else return new ReturnSet() { Type = ReturnType.PageNotFound };
                    }
                }
                return new ReturnSet() { Data = RecordId, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new InitialMigrationDb(Session))
                {
                    var data = db.Single<vEtfMigration>($"{Helpers.GetSqlQuery("vEtfMigration")} where te.ID = {{0}}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tEtfMigration()),
                            Schema = Helpers.GetSchema("tEtfMigration")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new InitialMigrationDb(Session))
                {

                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var etfToDelete = db.Single<tEtfMigration>("select * from dbo.tEtfMigration where ID = {0}", id);
                        db.Remove(etfToDelete);
                    }
                    db.SaveChanges();
                }

                return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet ExecuteAction()
        {
            try
            {
                string MethodName = Parameter["MethodName"]?.ToString();
                switch (MethodName.ToLower())
                {
                    case "downloadetf":
                        return DownloadETF();
                    default:
                        throw new Exception("Method not found");
                }
            }catch(Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet DownloadETF()
        {
            try
            {
                var etf = new ETF();
                string fileName = etf.GenerateTemplate(Session.ID_Company.ToInt32());
                return new ReturnSet() { Data = fileName, Type = ReturnType.Result };
                //return new ReturnSet();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public virtual ReturnSet UploadETF()
        //{
        //    try
        //    {
        //        var etf = new ETF.ETF();
        //        ExcelUploadReturn uploadReturn = etf.UploadETF(param["FileName_GUID"].ToString(), param["FileName"].ToString());

        //        if (uploadReturn.FileName.IsNull("").ToString() == "") return new ReturnSet() { Data = new { FileName_GUID = "", Sheets = uploadReturn.ExcelSheets }, Type = ReturnType.Result };
        //        else return new ReturnSet() { Data = new { FileName_GUID = uploadReturn.FileName, FileName = $"{param["FileName"].ToString().Replace(".xlsx", "")}_WithError.xlsx" }, Message = "Please check the errors at the last column.", Type = ReturnType.Error };
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = (ex.InnerException ?? ex).Message;
        //        Logger.LogError(ref msg, "UploadETF", HelperDb.CurrentUser(Ctx), "InSys.ITI.People");
        //        return new ReturnSet() { Message = msg, Type = ReturnType.Error };
        //    }
        //}
    }

    public class InitialMigrationDb : InSysContext
    {
        public InitialMigrationDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tEtfMigration> tEtfMigrations { get; set; }
        public DbQuery<vEtfMigration> vEtfMigrations { get; set; }
        public DbQuery<CountData> CountDatas { get; set; }
    }
}
