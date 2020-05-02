using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.Orders
{
    public class Orders : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Orders(_Session, _Parameter);
        }

        public Orders()
        {

        }

        public Orders(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new CartDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vCart>("(SELECT * FROM vCart)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListOrders", Helpers.CurrentUser(Session), "BC.Orders");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new CartDB(Session))
                {
                    var data = db.Single<vCart>("SELECT * FROM vCart WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tCart()),
                            Schema = Helpers.GetSchema("tCart")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormOrders", Helpers.CurrentUser(Session), "BC.Orders");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new CartDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {
                    var data = Parameter["Data"].ToObject<tCart>();
                    var detail = Parameter["Detail"].ToObject<List<tCart_Detail>>();
                    var _detail = db.tCart_Detail.Where(x => x.ID_Cart == data.ID).AsNoTracking().ToList();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tCart.Any(x => x.ID == data.ID))
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
                        d.ID_Cart = data.ID;

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
                    Logger.LogError(ref message, "SaveOrders", Helpers.CurrentUser(Session), "BC.Orders");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new CartDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tCart>("SELECT * FROM dbo.tCart where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordOrders", Helpers.CurrentUser(Session), "BC.Orders");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadLookup()
        {
            string lookupName = Parameter["LookupName"].ToString().ToLower();
            switch (lookupName)
            {
                case "customer":
                    return LoadCustomer();
                case "paymentmethod":
                    return LoadPaymentMethod();
                case "filingstatus":
                    return LoadFilingStatus();
                case "product":
                    return LoadProduct();
                default:
                    throw new Exception("Method not found.");
            }
        }

        public virtual ReturnSet LoadCustomer()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new CartDB(Session))
                {
                    var data = db.QueryTable<tCustomer>("(SELECT * FROM tCustomer WHERE IsActive = 1)a", to);
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
                Logger.LogError(ref message, "LoadCustomer", Helpers.CurrentUser(Session), "BC.Orders");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadPaymentMethod()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new CartDB(Session))
                {
                    var data = db.QueryTable<tPaymentMethod>("(SELECT * FROM dbo.tPaymentMethod WHERE IsActive = 1)a", to);
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
                Logger.LogError(ref message, "LoadPaymentMethod", Helpers.CurrentUser(Session), "BC.Orders");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadFilingStatus()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new CartDB(Session))
                {
                    var data = db.QueryTable<tFilingStatus>("(SELECT * FROM dbo.tFilingStatus WHERE IsActive = 1)a", to);
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
                Logger.LogError(ref message, "LoadFilingStatus", Helpers.CurrentUser(Session), "BC.Orders");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadProduct()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new CartDB(Session))
                {
                    var data = db.QueryTable<tProducts>("(SELECT * FROM dbo.tProducts WHERE IsActive = 1)a", to);
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
                Logger.LogError(ref message, "LoadFilingStatus", Helpers.CurrentUser(Session), "BC.Orders");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadDetail()
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new CartDB(Session))
                {
                    var data = db.QueryTable<vCart_Detail>("(SELECT * FROM vCart_Detail WHERE ID_Cart = {0})a", to, Parameter["ID"].ToInt32());
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadOrdersDetail", Helpers.CurrentUser(Session), "BC.Orders");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tCart GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tCart>("SELECT * FROM tCart WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class CartDB : InSysContext
    {
        public CartDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tCart> tCart { get; set; }
        public DbQuery<vCart> vCart { get; set; }
        public DbSet<tCart_Detail> tCart_Detail { get; set; }
        public DbQuery<vCart_Detail> vCart_Detail { get; set; }
        public DbSet<tCustomer> tCustomer { get; set; }
        public DbSet<tPaymentMethod> tPaymentMethod { get; set; }
        public DbSet<tFilingStatus> tFilingStatus { get; set; }
        public DbSet<tProducts> tProducts { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
