using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Province
{
    public class Province: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Province(_Session, _Parameter);
        }

        public Province(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public Province() { }

        public override ReturnSet LoadList()
        {
            string message = "";
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ProvinceDb(Session))
                {
                    var data = db.QueryTable<tProvince_old>("(select * from dbo.tProvince)a", to);
                    r = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadList", Helpers.CurrentUser(Session), "InSys.ITI.SupplementarySettings");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }

        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new ProvinceDb(Session))
                {
                    var data = db.Single<tProvince_old>("select * from dbo.tProvince where ID = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tProvince_old()),
                            Schema = Helpers.GetSchema("tProvince")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadForm", Helpers.CurrentUser(Session), "InSys.ITI.SupplementarySettings");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }

        public override ReturnSet Save()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new ProvinceDb(Session))
                {
                    tProvince_old tfm = Parameter["Data"].ToObject<tProvince_old>();
                    List<tCity> d_city = Parameter["City"].ToObject<List<tCity>>();
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            if (db.Any("(select * from dbo.tProvince where ID = {0})a", tfm.ID)) db.Update(tfm);
                            else db.Add(tfm);
                            db.SaveChanges(true);

                            foreach (var city in d_city)
                            {
                                city.ID_Province = tfm.ID;
                                if (db.Any("(select * from dbo.tCity where ID = {0})a", city.ID)) db.Update(city);
                                else db.Add(city);
                            }
                            if (d_city.Count > 0)
                                db.SaveChanges();
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            string message = (ex.InnerException ?? ex).Message;
                            Logger.LogError(ref message, "SaveForm", Helpers.CurrentUser((Session)), "InSys.ITI.SupplementarySettings");
                            r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                            return r;
                        }
                    }
                    return new ReturnSet() { Data = tfm.ID, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "SaveForm", Helpers.CurrentUser(Session), "InSys.ITI.SupplementarySettings");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public override ReturnSet DeleteRecord()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new ProvinceDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tProvince_old>("select * from dbo.tProvince where ID = {0} and ID > 1", id);
                        if (record != null) db.Remove(record);
                    }
                    db.SaveChanges();
                    return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteRecord", Helpers.CurrentUser(Session), "InSys.ITI.SupplementarySettings");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
    }
}
