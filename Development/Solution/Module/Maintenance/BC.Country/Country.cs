using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.Country
{
    public class Country : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Country(_Session, _Parameter);
        }

        public Country()
        {

        }

        public Country(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new CountryDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<tCountry>("(SELECT * FROM tCountry)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListCountry", Helpers.CurrentUser(Session), "BC.Country");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new CountryDB(Session))
                {
                    var data = db.Single<tCountry>("SELECT * FROM tCountry WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tCountry()),
                            Schema = Helpers.GetSchema("tCountry")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormCountry", Helpers.CurrentUser(Session), "BC.Country");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new CountryDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {

                    var data = Parameter["Data"].ToObject<tCountry>();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tCountry.Any(x => x.ID == data.ID))
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
                    Logger.LogError(ref message, "SaveCountry", Helpers.CurrentUser(Session), "BC.Country");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new CountryDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tCountry>("SELECT * FROM dbo.tCountry where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordCountry", Helpers.CurrentUser(Session), "BC.Country");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tCountry GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tCountry>("SELECT * FROM tCountry WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class CountryDB : InSysContext
    {
        public CountryDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tCountry> tCountry { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
