using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.ShiftSchedule
{
    public class ShiftSchedule : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new ShiftSchedule(_Session, _Parameter);
        }
        public ShiftSchedule(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public ShiftSchedule()
        {

        }
        public override ReturnSet LoadList()
        {
            try
            {
                var options = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ShiftScheduleDB(Session))
                {
                    var data = db.QueryTable<tShiftSchedule>("(SELECT * FROM tShiftSchedule WHERE ID_Company = {0})a",options,Session.ID_Company);
                    return new ReturnSet 
                    {
                        Data = new { Total = data.Count, Rows = data.Data.ToList() },
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
            using (var db = new ShiftScheduleDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {
                    var ss = Parameter["Data"].ToObject<tShiftSchedule>();
                    var details = Parameter["Details"].ToObject<List<tShiftSchedule_Detail>>();
                    var dateRef = Convert.ToDateTime(DateTime.Now.ToString("MM/dd/yyyy 00:00"));
                    ss.StartMinute = ss.TimeIn.Subtract(dateRef).Hours * 60;
                    var timeOut = ss.TimeOut;
                    var timeIn = ss.TimeIn;
                    if (timeOut < timeIn)
                    {
                        timeOut = timeOut.AddDays(1);
                        ss.TimeOut = timeOut;
                        ss.EndMinute = timeOut.Subtract(dateRef).TotalHours.ToInt32() * 60;
                    } 
                    else    
                        ss.EndMinute = timeOut.Subtract(dateRef).Hours * 60;

                    if (ss.FirstHalfTimeOut.HasValue)
                    {
                        var newFirstHalfTimeout = DateTime.Parse(ss.FirstHalfTimeOut.ToString());
                        if (ss.FirstHalfTimeOut < timeIn)
                            newFirstHalfTimeout = DateTime.Parse(ss.FirstHalfTimeOut.ToString()).AddDays(1);

                        ss.FirstHalfTimeOut = newFirstHalfTimeout;
                        ss.FirstHalfMinuteOut = newFirstHalfTimeout.Subtract(dateRef).TotalHours.ToInt32() * 60;
                        ss.FirstHalfWorkingHours = Convert.ToDecimal(newFirstHalfTimeout.Subtract(timeIn).TotalHours);
                        ss.FirstHalfWorkingMinutes = (ss.FirstHalfWorkingHours * Convert.ToDecimal(60)).ToInt32();
                    }

                    if (ss.SecondHalfTimeIn.HasValue)
                    {
                        var newSecondHalfTimeIn = DateTime.Parse(ss.SecondHalfTimeIn.ToString());
                        if(ss.SecondHalfTimeIn < timeIn)
                            newSecondHalfTimeIn = DateTime.Parse(ss.SecondHalfTimeIn.ToString()).AddDays(1);

                        ss.SecondHalfTimeIn = newSecondHalfTimeIn;
                        ss.SecondHalfMinuteIn = newSecondHalfTimeIn.Subtract(dateRef).TotalHours.ToInt32() * 60;
                        ss.SecondHalfWorkingHours = Convert.ToDecimal(timeOut.Subtract(newSecondHalfTimeIn).TotalHours);
                        ss.SecondHalfWorkingMinutes = (ss.SecondHalfWorkingHours * Convert.ToDecimal(60)).ToInt32();
                    }
                    if (ss.ID == 0)
                    {
                        db.Add(ss);
                    }
                    else 
                    {
                        if (db.tShiftSchedules.Any(x => x.ID == ss.ID))
                            db.Update(ss);
                        else
                            db.Add(ss);
                    }
                    db.SaveChanges(true);

                    if (details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.StartMinute = DateTime.Parse(detail.StartTime.ToString()).Subtract(dateRef).TotalHours.ToInt32() * 60;
                            detail.FlexibleMinutes = (detail.FlexibleHours * Convert.ToDecimal(60)).ToInt32();
                            if (detail.EndTime < detail.StartTime)
                            {
                                detail.EndTime = DateTime.Parse(detail.EndTime.ToString()).AddDays(1);
                            }
                            detail.EndMinute = DateTime.Parse(detail.EndTime.ToString()).Subtract(dateRef).TotalHours.ToInt32() * 60;
                            if (detail.ID == 0)
                            {
                                db.Add(detail);
                            }
                            else
                            {
                                if (db.tShiftSchedule_Details.Any(x => x.ID == detail.ID))
                                {
                                    db.Update(detail);
                                }
                                else 
                                {
                                    db.Add(detail);
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    db.Database.CommitTransaction();
                    return new ReturnSet { Data = ss.ID,Type = ReturnType.Result };
                }
                catch (Exception ex)
                {
                    db.Database.RollbackTransaction();
                    throw ex;
                }
            }   
        }
        public override ReturnSet LoadForm()
        {
            try
            {
                var id = Parameter["ID"].IsNull(0).ToInt32();
                using (var db = new ShiftScheduleDB(Session))
                {
                    var record = db.Single<vShiftSchedule>(@"SELECT ss.*,c.Name Company FROM dbo.tShiftSchedule ss
                                                             LEFT JOIN dbo.tCompany c ON c.ID = ss.ID_Company
                                                             WHERE ss.ID = {0}",id);
                    return new ReturnSet
                    {
                        Data = new
                        {
                            Form = record.IsNull(new vShiftSchedule() {TimeIn = new DateTime(),TimeOut = new DateTime(), ID_Company = Session.ID_Company.ToInt32(),Company= Session.Company}),
                            TableSchema = Helpers.GetSchema("tShiftSchedule")
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
        public override ReturnSet DeleteRecord()
        {
            using (var db = new ShiftScheduleDB(Session))
            {
                try
                {
                    db.Database.BeginTransaction();
                    var ids = Parameter["data"].ToObject<List<int>>();
                    var deletedIds = 0;
                    if (ids.Count > 0)
                    {
                        foreach (var id in ids)
                        {
                            var rec = db.tShiftSchedule_Details.SingleOrDefault(x => x.ID == id);
                            if (rec != null) db.Remove(rec);
                            deletedIds++;
                        }
                    }
                    db.SaveChanges();
                    db.Database.CommitTransaction();
                    return new ReturnSet
                    {
                        Message = $"{deletedIds} record(s) has been deleted.",
                        Type = ReturnType.Result
                    };
                }
                catch (Exception ex)
                {
                    return ExceptionLogger(ex,Session);
                }
            }
        }
        public override ReturnSet LoadLookup()
        {
            using (var db = new ShiftScheduleDB(Session))
            {
                var name = Parameter["Name"].ToString();
                var rawRights = Helpers.CompanyRights(Session.ID_Roles);
                var companyRights = rawRights.Split(",");
                switch (name.Trim().ToLower())
                {
                    case "company":
                        return base.LoadLookup<vCompany>(db,$"SELECT * FROM tCompany WHERE ID IN ({String.Join(",",companyRights)}) AND IsActive = 1");
                    case "hourtype":
                        return base.LoadLookup<vHourType>(db,"SELECT * FROM tHourType WHERE IsActive = 1");
                    default:
                        var msg = $"{name} is not available on your lookup option check your 'Switch Case' statement.";
                        Logger.LogError(ref msg,"LoadLookup",Session.Name,"InSys.ITI.ShiftSchedule");
                        throw new Exception("System Error! Please contact your System Administrator");
                }
            }
        }
        public override ReturnSet ExecuteAction()
        {
            try
            {
                var actionName = Parameter["Action"].ToString();
                switch (actionName)
                {
                    case "LoadShiftScheduleDetails":
                        return LoadShiftScheduleDetails();
                    default:
                        var msg = $"{actionName} is not available on your request parameter check your 'Switch Case' statement.";
                        Logger.LogError(ref msg, "ExecuteAction(LoadShiftScheduleDetails)", Session.Name, "InSys.ITI.ShiftSchedule");
                        throw new Exception("System Error! Please contact your System Administrator");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public ReturnSet LoadShiftScheduleDetails()
        {
            using (var db = new ShiftScheduleDB(Session))
            {
                var options = Parameter["data"].ToObject<TableOptions>();
                var shiftScheduleId = Parameter["ID_ShiftSchedule"].IsNull(0).ToInt32();

                var data = db.QueryTable<tShiftSchedule_Detail>("(SELECT * FROM tShiftSchedule_Detail WHERE ID_ShiftSchedule = {0})a",options,shiftScheduleId);
                return new ReturnSet
                {
                    Data = new
                    {
                        Total = data.Count,
                        Rows = data.Data.ToList()
                    },
                    Type = ReturnType.Result
                };
            }
        }
    }
}
