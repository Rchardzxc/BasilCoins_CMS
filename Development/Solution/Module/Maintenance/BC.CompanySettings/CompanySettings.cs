using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.CompanySettings
{
    public class CompanySettings : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new CompanySettings(_Session, _Parameter);
        }

        public CompanySettings()
        {

        }

        public CompanySettings(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new CompanySettingsDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<tCompanySettings>("(SELECT * FROM tCompanySettings)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListCompanySettings", Helpers.CurrentUser(Session), "BC.CompanySettings");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new CompanySettingsDB(Session))
                {
                    var data = db.Single<tCompanySettings>("SELECT * FROM tCompanySettings WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tCompanySettings()),
                            Schema = Helpers.GetSchema("tCompanySettings")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormCompanySettings", Helpers.CurrentUser(Session), "BC.CompanySettings");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new CompanySettingsDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {

                    var data = Parameter["Data"].ToObject<tCompanySettings>();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tCompanySettings.Any(x => x.ID == data.ID))
                            db.Update(data);
                        else
                        {
                            db.Add(data);
                        }
                    }
                    db.SaveChanges();
                    db.Database.CommitTransaction();

                    return new ReturnSet() { Data = GetOne(db, data.ID), Type = ReturnType.Result };
                }
                catch (Exception ex)
                {
                    string message = (ex.InnerException ?? ex).Message;
                    Logger.LogError(ref message, "SaveCompanySettings", Helpers.CurrentUser(Session), "BC.CompanySettings");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new CompanySettingsDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tCompanySettings>("SELECT * FROM dbo.tCompanySettings where ID = {0}", id);
                        if (record != null) db.Remove(record);
                    }
                    db.SaveChanges();

                    return new ReturnSet()
                    {
                        Message = "Record deleted.",
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteRecordCompanySettings", Helpers.CurrentUser(Session), "BC.CompanySettings");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tCompanySettings GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tCompanySettings>("SELECT * FROM tCompanySettings WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class CompanySettingsDB : InSysContext
    {
        public CompanySettingsDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tCompanySettings> tCompanySettings { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
