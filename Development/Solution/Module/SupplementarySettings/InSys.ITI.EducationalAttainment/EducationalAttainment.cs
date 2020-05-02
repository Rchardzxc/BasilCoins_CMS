using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.EducationalAttainment
{
    public class EducationalAttainment: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new EducationalAttainment(_Session, _Parameter);
        }

        public EducationalAttainment(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public EducationalAttainment() { }

        public override ReturnSet LoadList()
        {
            string message = "";
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new EducationalAttainmentDb(Session))
                {
                    var data = db.QueryTable<tEducationalAttainment>("(select * from dbo.tEducationalAttainment)a", to);
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
                using (var db = new EducationalAttainmentDb(Session))
                {
                    var data = db.Single<tEducationalAttainment>("select * from dbo.tEducationalAttainment where ID = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tEducationalAttainment()
                            {
                                DateTimeCreated = DateTime.Now,
                                ID_CreatedBy = (Session.ID_User)
                            }),
                            Schema = Helpers.GetSchema("tEducationalAttainment")
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
                using (var db = new EducationalAttainmentDb(Session))
                {
                    tEducationalAttainment tfm = Parameter["Data"].ToObject<tEducationalAttainment>();
                    if (db.Any("(select * from dbo.tEducationalAttainment where ID = {0})a", tfm.ID)) db.Update(tfm);
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
                using (var db = new EducationalAttainmentDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tEducationalAttainment>("select * from dbo.tEducationalAttainment where ID = {0}", id);
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
