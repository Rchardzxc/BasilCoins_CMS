using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Religion
{
    public class Religion: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Religion(_Session, _Parameter);
        }

        public Religion(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public Religion() { }

        public override ReturnSet LoadList()
        {
            string message = "";
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ReligionDb(Session))
                {
                    var data = db.QueryTable<tReligion>("(select * from dbo.tReligion)a", to);
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
                using (var db = new ReligionDb(Session))
                {
                    var data = db.Single<tReligion>("select * from dbo.tReligion where ID = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tReligion()),
                            Schema = Helpers.GetSchema("tReligion")
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
                using (var db = new ReligionDb(Session))
                {
                    tReligion tfm = Parameter["Data"].ToObject<tReligion>();
                    if (db.Any("(select * from dbo.tReligion where ID = {0})a", tfm.ID)) db.Update(tfm);
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
                using (var db = new ReligionDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tReligion>("select * from dbo.tReligion where ID = {0}", id);
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
