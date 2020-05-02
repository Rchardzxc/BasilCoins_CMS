using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.Category
{
    public class Category : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Category(_Session, _Parameter);
        }

        public Category()
        {

        }

        public Category(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new CategoryDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vCategory>("(SELECT * FROM vCategory)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListCategory", Helpers.CurrentUser(Session), "BC.Category");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new CategoryDB(Session))
                {
                    var data = db.Single<vCategory>("SELECT * FROM vCategory WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tCategory()),
                            Schema = Helpers.GetSchema("tCategory")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormCategory", Helpers.CurrentUser(Session), "BC.Category");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new CategoryDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {

                    var data = Parameter["Data"].ToObject<tCategory>();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tCategory.Any(x => x.ID == data.ID))
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
                    Logger.LogError(ref message, "SaveCategory", Helpers.CurrentUser(Session), "BC.Category");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new CategoryDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tCategory>("SELECT * FROM dbo.tCategory where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordCategory", Helpers.CurrentUser(Session), "BC.Category");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }
        public override ReturnSet LoadLookup()
        {
            string lookupName = Parameter["LookupName"].ToString().ToLower();
            switch (lookupName)
            {
                case "category":
                    return LoadCategory();
                case "yearsofmint":
                    return LoadProductBrand();
                default:
                    throw new Exception("Method not found.");
            }
        }

        public virtual ReturnSet LoadCategory()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new CategoryDB(Session))
                {
                    var data = db.QueryTable<tCategory>("(SELECT * FROM tCategory WHERE IsActive = 1)a", to);
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
                Logger.LogError(ref message, "LoadCategory", Helpers.CurrentUser(Session), "BC.Category");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadProductBrand()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new CategoryDB(Session))
                {
                    var data = db.QueryTable<tYearsofMint>("(SELECT * FROM dbo.tYearsofMint WHERE IsActive = 1)a", to);
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
                Logger.LogError(ref message, "LoadYearsofMint", Helpers.CurrentUser(Session), "BC.Category");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tCategory GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tCategory>("SELECT * FROM tCategory WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class CategoryDB : InSysContext
    {
        public CategoryDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tCategory> tCategory { get; set; }
        public DbQuery<vCategory> vCategory { get; set; }
        public DbSet<tYearsofMint> tYearsofMint { get; set; }
        public DbQuery<CountData> CountData { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
    }
}
