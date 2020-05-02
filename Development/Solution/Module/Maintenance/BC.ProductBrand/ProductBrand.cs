using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.ProductBrand
{
    public class ProductBrand : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new ProductBrand(_Session, _Parameter);
        }

        public ProductBrand()
        {

        }

        public ProductBrand(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new ProductBrandDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<tProductBrand>("(SELECT * FROM tProductBrand)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListProductBrand", Helpers.CurrentUser(Session), "BC.ProductBrand");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new ProductBrandDB(Session))
                {
                    var data = db.Single<tProductBrand>("SELECT * FROM tProductBrand WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tProductBrand()),
                            Schema = Helpers.GetSchema("tProductBrand")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormProductBrand", Helpers.CurrentUser(Session), "BC.ProductBrand");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new ProductBrandDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {

                    var data = Parameter["Data"].ToObject<tProductBrand>();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tProductBrand.Any(x => x.ID == data.ID))
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
                    Logger.LogError(ref message, "SaveProductBrand", Helpers.CurrentUser(Session), "BC.ProductBrand");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new ProductBrandDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tProductBrand>("SELECT * FROM dbo.tProductBrand where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordProductBrand", Helpers.CurrentUser(Session), "BC.ProductBrand");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tProductBrand GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tProductBrand>("SELECT * FROM tProductBrand WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class ProductBrandDB : InSysContext
    {
        public ProductBrandDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tProductBrand> tProductBrand { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
