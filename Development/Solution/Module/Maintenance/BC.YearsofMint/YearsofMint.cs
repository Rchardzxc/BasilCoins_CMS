using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.YearsofMint
{
    public class YearsofMint : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new YearsofMint(_Session, _Parameter);
        }

        public YearsofMint()
        {

        }

        public YearsofMint(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new YearsofMintDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<tYearsofMint>("(SELECT * FROM tYearsofMint)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListYearsofMint", Helpers.CurrentUser(Session), "BC.YearsofMint");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new YearsofMintDB(Session))
                {
                    var data = db.Single<tYearsofMint>("SELECT * FROM tYearsofMint WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tYearsofMint()),
                            Schema = Helpers.GetSchema("tYearsofMint")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormYearsofMint", Helpers.CurrentUser(Session), "BC.YearsofMint");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new YearsofMintDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {

                    var data = Parameter["Data"].ToObject<tYearsofMint>();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tYearsofMint.Any(x => x.ID == data.ID))
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
                    Logger.LogError(ref message, "SaveYearsofMint", Helpers.CurrentUser(Session), "BC.YearsofMint");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new YearsofMintDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tYearsofMint>("SELECT * FROM dbo.tYearsofMint where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordYearsofMint", Helpers.CurrentUser(Session), "BC.YearsofMint");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tYearsofMint GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tYearsofMint>("SELECT * FROM tYearsofMint WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class YearsofMintDB : InSysContext
    {
        public YearsofMintDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tYearsofMint> tYearsofMint { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
