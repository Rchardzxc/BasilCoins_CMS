using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using z.Data;

namespace BC.Products
{
    public class Products : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Products(_Session, _Parameter);
        }

        public Products()
        {

        }

        public Products(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new ProductsDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vProducts>("(SELECT * FROM vProducts)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListProducts", Helpers.CurrentUser(Session), "BC.Products");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new ProductsDB(Session))
                {
                    var data = db.Single<vProducts>("SELECT * FROM vProducts WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tProducts()),
                            Schema = Helpers.GetSchema("tProducts")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormProducts", Helpers.CurrentUser(Session), "BC.Products");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new ProductsDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {

                    var data = Parameter["Data"].ToObject<tProducts>();
                    var detail = Parameter["Detail"].ToObject<List<tProducts_Detail>>();
                    var _detail = db.tProducts_Detail.Where(x => x.ID_Products == data.ID).AsNoTracking().ToList();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tProducts.Any(x => x.ID == data.ID))
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
                        d.ID_Products = data.ID;

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
                    Logger.LogError(ref message, "SaveProducts", Helpers.CurrentUser(Session), "BC.Products");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new ProductsDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tProducts>("SELECT * FROM dbo.tProducts where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordProducts", Helpers.CurrentUser(Session), "BC.Products");
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
                case "productbrand":
                    return LoadProductBrand();
                case "color":
                    return LoadColor();
                default:
                    throw new Exception("Method not found.");
            }
        }

        public virtual ReturnSet LoadCategory()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new ProductsDB(Session))
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
                Logger.LogError(ref message, "LoadCategory", Helpers.CurrentUser(Session), "BC.Products");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadProductBrand()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new ProductsDB(Session))
                {
                    var data = db.QueryTable<tProductBrand>("(SELECT * FROM dbo.tProductBrand WHERE IsActive = 1)a", to);
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
                Logger.LogError(ref message, "LoadProductBrand", Helpers.CurrentUser(Session), "BC.Products");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadColor()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new ProductsDB(Session))
                {
                    var data = db.QueryTable<tColor>("(SELECT * FROM dbo.tColor WHERE IsActive = 1)a", to);
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
                Logger.LogError(ref message, "LoadColor", Helpers.CurrentUser(Session), "BC.Products");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadDetail()
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ProductsDB(Session))
                {
                    var data = db.QueryTable<vProducts_Detail>("(SELECT * FROM vProducts_Detail WHERE ID_Products = {0})a", to, Parameter["ID"].ToInt32());
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadProductBrandDetail", Helpers.CurrentUser(Session), "BC.Products");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tProducts GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tProducts>("SELECT * FROM tProducts WHERE ID = {0}", id).SingleOrDefault();
        }

    }

    public class ProductsDB : InSysContext
    {
        public ProductsDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tProducts> tProducts { get; set; }
        public DbQuery<vProducts> vProducts { get; set; }
        public DbSet<tProducts_Detail> tProducts_Detail { get; set; }
        public DbQuery<vProducts_Detail> vProducts_Detail { get; set; }
        public DbSet<tCategory> tCategory { get; set; }
        public DbSet<tProductBrand> tProductBrand { get; set; }
        public DbSet<tColor> tColor { get; set; }
        public DbQuery<CountData> CountData { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
    }
}
