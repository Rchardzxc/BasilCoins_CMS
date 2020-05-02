using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.ProductsRecommended
{
    public class ProductsRecommended : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new ProductsRecommended(_Session, _Parameter);
        }

        public ProductsRecommended()
        {

        }

        public ProductsRecommended(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new ProductsRecommendedDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<tProductsRecommended>("(SELECT * FROM tProductsRecommended)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListProductsRecommended", Helpers.CurrentUser(Session), "BC.ProductsRecommended");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new ProductsRecommendedDB(Session))
                {
                    var data = db.Single<tProductsRecommended>("SELECT * FROM tProductsRecommended WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tProductsRecommended()),
                            Schema = Helpers.GetSchema("tProductsRecommended")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormProductsRecommended", Helpers.CurrentUser(Session), "BC.ProductsRecommended");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new ProductsRecommendedDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {
                    var data = Parameter["Data"].ToObject<tProductsRecommended>();
                    var detail = Parameter["Detail"].ToObject<List<tProductsRecommended_Detail>>();
                    var _detail = db.tProductsRecommended_Detail.Where(x => x.ID_ProductsRecommended == data.ID).AsNoTracking().ToList();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tProductsRecommended.Any(x => x.ID == data.ID))
                            db.Update(data);
                        else
                        {
                            db.Add(data);
                        }
                    }
                    db.SaveChanges();

                    //Delete Detail
                    foreach (var d in _detail)
                    {
                        var _data = detail.Where(x => x.ID == d.ID).FirstOrDefault();
                        if (_data == null) db.Remove(d);
                    }

                    //Add or Update Detail
                    foreach (var d in detail)
                    {
                        d.ID = d.ID == 0 ? null : d.ID; // Use to track changes                             
                        var _data = _detail.Where(x => x.ID == d.ID).FirstOrDefault();
                        d.ID_ProductsRecommended = data.ID;

                        if (_data != null)
                        {
                            db.Update(d);
                        }
                        else db.Add(d);
                    }

                    db.SaveChanges();
                    db.Database.CommitTransaction();

                    return new ReturnSet() { Data = GetOne(db, data.ID), Type = ReturnType.Result };
                }
                catch (Exception ex)
                {
                    string message = (ex.InnerException ?? ex).Message;
                    Logger.LogError(ref message, "SaveProductsRecommended", Helpers.CurrentUser(Session), "BC.ProductsRecommended");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new ProductsRecommendedDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tProductsRecommended>("SELECT * FROM dbo.tProductsRecommended where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordProductsRecommended", Helpers.CurrentUser(Session), "BC.ProductsRecommended");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        //Load Product List
        public virtual ReturnSet LoadProductList()
        {
            var ret = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ProductsRecommendedDB(Session))
                {
                    var data = db.QueryTable<vProducts>(@"(SELECT * FROM dbo.vProducts)a", to);
                    ret = new ReturnSet()
                    {
                        Data = new
                        {
                            Total = data.Count,
                            Rows = data.Data.ToList()
                        },
                        Type = ReturnType.Result
                    };
                    return ret;
                }
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
                    case "loadproductlist":
                        return LoadProductList();
                    default:
                        throw new Exception("Method not found");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadDetail()
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ProductsRecommendedDB(Session))
                {
                    var data = db.QueryTable<vProductsRecommended_Detail>("(SELECT * FROM vProductsRecommended_Detail WHERE ID_ProductsRecommended = {0})a", to, Parameter["ID"].ToInt32());
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadProductsRecommendedDetail", Helpers.CurrentUser(Session), "BC.ProductsRecommended");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tProductsRecommended GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tProductsRecommended>("SELECT * FROM tProductsRecommended WHERE ID = {0}", id).SingleOrDefault();
        }
    }
    public class ProductsRecommendedDB : InSysContext
    {
        public ProductsRecommendedDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tProductsRecommended> tProductsRecommended { get; set; }
        public DbSet<tProductsRecommended_Detail> tProductsRecommended_Detail { get; set; }
        public DbQuery<vProductsRecommended_Detail> vProductsRecommended_Detail { get; set; }
        public DbQuery<vProducts> vProducts { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
