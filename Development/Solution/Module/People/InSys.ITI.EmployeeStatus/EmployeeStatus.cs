using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.EmployeeStatus
{
    public class EmployeeStatus : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new EmployeeStatus(_Session, _Parameter);
        }

        public EmployeeStatus(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public EmployeeStatus() { }

        public override ReturnSet LoadList()
        {
            string message = "";
            var r = new ReturnSet();
            int EmployeeStatusId = Parameter["Data"].ToObject<tEmployeeStatus>().ID;

            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();

                using (var db = new EmployeeStatusDb(Session))
                {
                    var data = db.QueryTable<tEmployeeStatus>("(select * from dbo.tEmployeeStatus)a", to);
                    r = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadList", Helpers.CurrentUser(Session), "InSys.ITI.EmployeeStatus");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }

        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new EmployeeStatusDb(Session))
                {
                    var data = db.Single<tEmployeeStatus>("select * from dbo.tEmployeeStatus where ID = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tEmployeeStatus()),
                            Schema = Helpers.GetSchema("tEmployeeStatus")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadForm", Helpers.CurrentUser(Session), "InSys.ITI.EmployeeStatus");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }

        public override ReturnSet Save()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new EmployeeStatusDb(Session))
                {
                    tEmployeeStatus tfm = Parameter["Data"].ToObject<tEmployeeStatus>();
                    if (db.Any("(select * from dbo.tEmployeeStatus where ID = {0})a", tfm.ID)) db.Update(tfm);
                    else db.Add(tfm);
                    db.SaveChanges(true);

                    return new ReturnSet() { Data = tfm.ID, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "SaveForm", Helpers.CurrentUser(Session), "InSys.ITI.EmployeeStatus");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }

        public override ReturnSet DeleteRecord()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new EmployeeStatusDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tEmployeeStatus>("select * from dbo.tEmployeeStatus where ID = {0}", id);
                        if (record != null) db.Remove(record);
                    }
                    db.SaveChanges();

                    return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteRecord", Helpers.CurrentUser(Session), "InSys.ITI.EmployeeStatus");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
    }
}
