using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.Customer
{
    public class Customer : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Customer(_Session, _Parameter);
        }

        public Customer()
        {

        }

        public Customer(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        //Load Product List
        public virtual ReturnSet ChangePassword()
        {
            var ret = new ReturnSet();
            try
            {
                var Action = Parameter["Action"].ToObject<ActionModel>();//use parameter inside the case block
                var customerData = Action.Parameter["Data"].ToObject<CustomerAccountModel>();

                using (var db = new CustomerDB(Session))
                {
                    var customer = db.Single<tCustomer>("select * from dbo.tCustomer where ID = {0}", customerData.ID_Customer);
                    string newPassword = customerData.NewPassword;
                    customer.Password = newPassword;
                    db.Update(customer);
                    db.SaveChanges(true);
                    return new ReturnSet() { Type = ReturnType.Result };
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
                var MethodName = Parameter["Action"].ToObject<ActionModel>();//use parameter inside the case block
                switch (MethodName.Name.ToLower())
                {
                    case "savepasswordchange":
                        return ChangePassword();
                    default:
                        throw new Exception("Method not found");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new CustomerDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vCustomer>("(SELECT * FROM vCustomer)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListCustomer", Helpers.CurrentUser(Session), "BC.Customer");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new CustomerDB(Session))
                {
                    var data = db.Single<vCustomer>("SELECT * FROM vCustomer WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tCustomer()),
                            Schema = Helpers.GetSchema("tCustomer")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormCustomer", Helpers.CurrentUser(Session), "BC.Customer");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new CustomerDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {

                    var data = Parameter["Data"].ToObject<tCustomer>();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tCustomer.Any(x => x.ID == data.ID))
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
                    Logger.LogError(ref message, "SaveCustomer", Helpers.CurrentUser(Session), "BC.Customer");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new CustomerDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tCustomer>("SELECT * FROM dbo.tCustomer where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordCustomer", Helpers.CurrentUser(Session), "BC.Customer");
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
                case "province":
                    return LoadProvince();
                default:
                    throw new Exception("Method not found.");
            }
        }

        public virtual ReturnSet LoadCountry()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new CustomerDB(Session))
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
                Logger.LogError(ref message, "LoadCountry", Helpers.CurrentUser(Session), "BC.Customer");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadProvince()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new CustomerDB(Session))
                {
                    var data = db.QueryTable<tProvince>("(SELECT * FROM tProvince WHERE IsActive = 1)a", to);
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
                Logger.LogError(ref message, "LoadProvince", Helpers.CurrentUser(Session), "BC.Customer");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tCustomer GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tCustomer>("SELECT * FROM tCustomer WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class CustomerDB : InSysContext
    {
        public CustomerDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tCustomer> tCustomer { get; set; }
        public DbQuery<vCustomer> vCustomer { get; set; }
        public DbSet<tCountry> tCountry { get; set; }
        public DbSet<tProvince> tProvince { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
