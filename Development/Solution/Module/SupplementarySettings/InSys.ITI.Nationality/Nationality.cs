using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Nationality
{
    public class Nationality: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Nationality(_Session, _Parameter);
        }
        public Nationality(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }
        public Nationality() { }

        public override ReturnSet LoadList()
        {
            string message = "";
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db  = new  NationalityDb(Session))
                {
                    var data = db.QueryTable<tNationality>("(select * from dbo.tNationality)a", to);
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
                using (var db  = new  NationalityDb(Session))
                {
                    var data = db.Single<tNationality>("select * from dbo.tNationality where ID = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tNationality()),
                            Schema = Helpers.GetSchema("tNationality")
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
                using (var db  = new  NationalityDb(Session))
                {
                    tNationality tfm = Parameter["Data"].ToObject<tNationality>();
                    if (db.Any("(select * from dbo.tNationality where ID = {0})a", tfm.ID)) db.Update(tfm);
                    else db.Add(tfm);
                    db.SaveChanges(true);

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
                using (var db  = new  NationalityDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tNationality>("select * from dbo.tNationality where ID = {0}", id);
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
