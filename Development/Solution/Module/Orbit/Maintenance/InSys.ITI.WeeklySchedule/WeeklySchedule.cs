using System;
using System.Collections.Generic;
using System.Linq;
using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using z.Data;

namespace InSys.ITI.WeeklySchedule
{
    public class WeeklySchedule : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new WeeklySchedule(_Session, _Parameter);
        }
        public WeeklySchedule()
        {
        }
        public WeeklySchedule(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public override ReturnSet LoadList()
        {
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new WeeklyScheduleDb(Session))
                {
                    var data = db.QueryTable<vWeeklySchedule>($"({Helpers.GetSqlQuery("vWeeklySchedule")})a",to);
                    r = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet LoadForm()
        {
            try 
            {
                var dataID = Parameter["ID"].IsNull(0).ToInt32();
                using (var db = new WeeklyScheduleDb(Session))
                {
                    var WeeklySchedule = db.Single<vWeeklySchedule>($"{Helpers.GetSqlQuery("vWeeklySchedule")} where week.ID = {{0}}", dataID);
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = WeeklySchedule.IsNull(new vWeeklySchedule { }),
                            Schema = Helpers.GetSchema("tWeeklySchedule")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet Save()
        {
            try 
            {
                using (var db = new WeeklyScheduleDb(Session))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            tWeeklySchedule tws = Parameter["data"].ToObject<tWeeklySchedule>();
                            tws.ID_Company = Session.ID_Company;
                            int ID = tws.ID; //check if new
                            if (db.Any("(select * from dbo.tWeeklySchedule where ID = {0})a", tws.ID)) db.Update(tws);
                            else db.Add(tws);
                            db.SaveChanges(true);

                            tran.Commit();

                            return new ReturnSet() { Data = tws.ID, Type = ReturnType.Result };
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new WeeklyScheduleDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tWeeklySchedule>("select * from dbo.tWeeklySchedule where id = {0}", id);
                        if (record != null) db.Remove(record);
                    }
                    db.SaveChanges();

                    return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        } 
        public override ReturnSet LoadLookup()
        {
            try
            {
                string lookupName = Parameter["lookupName"].ToString().ToLower();
                switch (lookupName)
                {
                    case "dailyschedule":
                        return LoadDailySchedule();
                    default:
                        throw new Exception("Lookup Not found.");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadDailySchedule()
        {
            var to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new WeeklyScheduleDb(Session)) 
            {
                var data = db.QueryTable<tLookUpData>("(select ID, Name, isActive from dbo.tShiftSchedule where isActive = 1)a", to);
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }
    }
}
