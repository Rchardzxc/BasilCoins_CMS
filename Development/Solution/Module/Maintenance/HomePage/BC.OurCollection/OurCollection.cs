using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace BC.OurCollection
{
    public class OurCollection : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new OurCollection(_Session, _Parameter);
        }

        public OurCollection()
        {

        }

        public OurCollection(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new OurCollectionDB(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<tOurCollections>("(SELECT * FROM tOurCollections)a", to);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadListOurCollection", Helpers.CurrentUser(Session), "BC.OurCollection");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new OurCollectionDB(Session))
                {
                    var data = db.Single<tOurCollections>("SELECT * FROM tOurCollections WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tOurCollections()),
                            Schema = Helpers.GetSchema("tOurCollections")
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadFormOurCollection", Helpers.CurrentUser(Session), "BC.OurCollection");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            using (var db = new OurCollectionDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {
                    var data = Parameter["Data"].ToObject<tOurCollections>();
                    var detail = Parameter["Detail"].ToObject<List<tOurCollections_Detail>>();
                    var _detail = db.tOurCollections_Detail.Where(x => x.ID_OurCollections == data.ID).AsNoTracking().ToList();

                    if (data.ID == 0)
                    {
                        db.Add(data);
                    }
                    else
                    {
                        if (db.tOurCollections.Any(x => x.ID == data.ID))
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
                        d.ID_OurCollections = data.ID;

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
                    Logger.LogError(ref message, "SaveOurCollection", Helpers.CurrentUser(Session), "BC.OurCollection");
                    return new ReturnSet() { Message = message, Type = ReturnType.Error };
                }
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new OurCollectionDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tOurCollections>("SELECT * FROM dbo.tOurCollections where ID = {0}", id);
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
                Logger.LogError(ref message, "DeleteRecordOurCollection", Helpers.CurrentUser(Session), "BC.OurCollection");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadLookup()
        {
            string lookupName = Parameter["LookupName"].ToString().ToLower();
            switch (lookupName)
            {
                case "product":
                    return LoadProduct();
                default:
                    throw new Exception("Method not found.");
            }
        }

        public virtual ReturnSet LoadProduct()
        {
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new OurCollectionDB(Session))
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
                Logger.LogError(ref message, "LoadFilingStatus", Helpers.CurrentUser(Session), "BC.OurCollection");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadDetail()
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new OurCollectionDB(Session))
                {
                    var data = db.QueryTable<vOurCollections_Detail>("(SELECT * FROM vOurCollections_Detail WHERE ID_OurCollections = {0})a", to, Parameter["ID"].ToInt32());
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadOurCollectionDetail", Helpers.CurrentUser(Session), "BC.OurCollection");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        private tOurCollections GetOne(DbContext db, int id)
        {
            return db.ExecQuery<tOurCollections>("SELECT * FROM tOurCollections WHERE ID = {0}", id).SingleOrDefault();
        }
    }

    public class OurCollectionDB : InSysContext
    {
        public OurCollectionDB(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbSet<tOurCollections> tOurCollections { get; set; }
        public DbSet<tOurCollections_Detail> tOurCollections_Detail { get; set; }
        public DbQuery<vOurCollections_Detail> vOurCollections_Detail { get; set; }
        public DbSet<tCategory> tCategory { get; set; }
        public DbQuery<CountData> CountData { get; set; }
    }
}
