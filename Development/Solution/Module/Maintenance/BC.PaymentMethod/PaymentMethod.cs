using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.PaymentMethod
{
    public class PaymentMethod : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new PaymentMethod(_Session, _Parameter);
        }

        public PaymentMethod()
        {

        }

        public PaymentMethod(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new PaymentMethodDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<tPaymentMethod>("(SELECT * FROM tPaymentMethod)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListPaymentMethod", Helpers.CurrentUser(Session), "BC.PaymentMethod");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new PaymentMethodDB(Session))
                {
                    var data = db.Single<tPaymentMethod>("SELECT * FROM tPaymentMethod WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tPaymentMethod()),
                            Schema = Helpers.GetSchema("tPaymentMethod")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormPaymentMethod", Helpers.CurrentUser(Session), "BC.PaymentMethod");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new PaymentMethodDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {

                    var data = Parameter["Data"].ToObject<tPaymentMethod>();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tPaymentMethod.Any(x => x.ID == data.ID))
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
                    Logger.LogError(ref message, "SavePaymentMethod", Helpers.CurrentUser(Session), "BC.PaymentMethod");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new PaymentMethodDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tPaymentMethod>("SELECT * FROM dbo.tPaymentMethod where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordPaymentMethod", Helpers.CurrentUser(Session), "BC.PaymentMethod");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tPaymentMethod GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tPaymentMethod>("SELECT * FROM tPaymentMethod WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class PaymentMethodDB : InSysContext
    {
        public PaymentMethodDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tPaymentMethod> tPaymentMethod { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
