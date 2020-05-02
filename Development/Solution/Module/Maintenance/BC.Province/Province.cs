using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.Province
{
    public class Province : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Province(_Session, _Parameter);
        }

        public Province()
        {

        }

        public Province(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new ProvinceDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vProvince>("(SELECT * FROM vProvince)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListProvince", Helpers.CurrentUser(Session), "BC.Province");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new ProvinceDB(Session))
                {
                    var data = db.Single<vProvince>("SELECT * FROM vProvince WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tProvince()),
                            Schema = Helpers.GetSchema("tProvince")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormProvince", Helpers.CurrentUser(Session), "BC.Province");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new ProvinceDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {

                    var data = Parameter["Data"].ToObject<tProvince>();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tProvince.Any(x => x.ID == data.ID))
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
                    Logger.LogError(ref message, "SaveProvince", Helpers.CurrentUser(Session), "BC.Province");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new ProvinceDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tProvince>("SELECT * FROM dbo.tProvince where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordProvince", Helpers.CurrentUser(Session), "BC.Province");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadLookup()
        {
            string lookupName = Parameter["LookupName"].ToString().ToLower();
            switch (lookupName)
            {
                case "country":
                    return LoadCountry();
                default:
                    throw new Exception("Method not found.");
            }
        }

        public virtual ReturnSet LoadCountry()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new ProvinceDB(Session))
                {
                    var data = db.QueryTable<tCountry>("(SELECT * FROM tCountry WHERE IsActive = 1)a", to);
                    return new ReturnSet()
                    {
                        Data = new { Total = data.Count, Rows = data.Data.ToList() },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadCountry", Helpers.CurrentUser(Session), "BC.Province");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tProvince GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tProvince>("SELECT * FROM tProvince WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class ProvinceDB : InSysContext
    {
        public ProvinceDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tProvince> tProvince { get; set; }
        public DbQuery<vProvince> vProvince { get; set; }
        public DbSet<tCountry> tCountry { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
