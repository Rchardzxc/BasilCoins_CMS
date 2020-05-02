using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Overtime
{
    public class Overtime : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Overtime(_Session,_Parameter);
        }
        public Overtime()
        {

        }
        public Overtime(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new OvertimeDB(Session))
                {
                    if (Session.ID_Roles == 1) throw new Exception("System Account is not entitled to access Overtime module. Please sign in using Employee account.");
                    var options = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<tOvertime>("(SELECT o.*,fs.Name FilingStatus FROM tOvertime o left join tFilingStatus fs on fs.id = o.id_FilingStatus WHERE o.ID_Employee = {0})a", options,Session.ID_Employee);
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

            using (var db = new OvertimeDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {
                    var overtime = Parameter["Data"].ToObject<tOvertime>();
                    var post = Parameter["Post"].IsNull(false).ToBool();
                    var refDate = DateTime.Parse(overtime.WorkDate.ToString("MM/dd/yyyy"));

                    new Validate(overtime, Session);
                    
                    if (overtime.EndTime < overtime.StartTime)
                    {
                        overtime.EndTime = overtime.EndTime.AddDays(1);
                    }
                    overtime.StartMinute = overtime.StartTime.ToDatetimeMinute(refDate);
                    overtime.EndMinute = overtime.EndTime.ToDatetimeMinute(refDate);
                    overtime.ComputedHours = overtime.EndTime.Subtract(overtime.StartTime).TotalHours.ToInt32();

                    if (post)
                        overtime.IsPosted = true;

                    if (overtime.ID == 0)
                    {
                        overtime.ReferenceNo = Helpers.getReferenceNumber("tOvertime", Session.ID_Company.ToInt32());
                        db.Add(overtime);
                        Helpers.updateReferenceNum("tOvertime", Session.ID_Company.ToInt32(), Session);
                    }
                    else
                    {
                        if (db.Overtimes.Any(x => x.ID == overtime.ID))
                            db.Update(overtime);
                        else
                        {
                            overtime.ReferenceNo = Helpers.getReferenceNumber("tOvertime", Session.ID_Company.ToInt32());
                            db.Add(overtime);
                            Helpers.updateReferenceNum("tOvertime", Session.ID_Company.ToInt32(), Session);
                        }    

                    }
                    db.SaveChanges();
                    db.Database.CommitTransaction();

                    //return new ReturnSet { Data = GetOne(db,overtime.ID), Type = ReturnType.Result };
                    return new ReturnSet() { Message = "Successfully saved.", Data = GetOne(db,overtime.ID), Type = ReturnType.Result };
                }
                catch (Exception ex)
                {
                    if (IsValidateError)
                    {
                        db.Database.RollbackTransaction();
                        return new ReturnSet() { Message = ex.Message, Type = ReturnType.ValidationError };
                    }
                    else
                    {
                        db.Database.RollbackTransaction();
                        return ExceptionLogger(ex, Session);
                    }
                }
            }
            
        }
        public override ReturnSet LoadForm()
        {
            try
            {
                using (var db = new OvertimeDB(Session))
                {
                    var id = Parameter["ID"].IsNull(0).ToInt32();
                    var rec = GetOne(db, id);

                    //Load Work Credit
                    var workCredits = base.LoadLookup<tWorkCredit>(db, "SELECT * FROM tWorkCredit WHERE IsActive = 1");

                    return new ReturnSet 
                    {
                        Data = new 
                        { 
                            Form = rec.IsNull(new vOvertime() 
                            {
                                ReferenceNo = "OT-XXXXXX",
                                ID_Employee = Session.ID_Employee.ToInt32(),
                                FilingDate = DateTime.Now,
                                ID_FilingStatus = 1,
                                ID_WorkCredit = 1,
                                WorkDate = DateTime.Now,
                                CurrentApproverLevel = 1
                            }),
                            WorkCredits = workCredits,
                            Schema = Helpers.GetSchema("tOvertime") 
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
            using (var db = new OvertimeDB(Session))
            {
                db.Database.BeginTransaction();
                try
                {
                    var ids = Parameter["Data"].ToObject<List<int>>();
                    var rejRec = new List<string>();
                    var deletedCnt = 0;
                    if (ids.Count > 0)
                    {
                        foreach (var id in ids)
                        {
                            var rec = db.Overtimes.SingleOrDefault(x => x.ID == id);
                            if (rec != null)
                            {
                                if (rec.ID_FilingStatus > 1)
                                {
                                    rejRec.Add(rec.ReferenceNo);
                                }
                                else 
                                {
                                    db.Remove(rec);
                                    deletedCnt++;
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    db.Database.CommitTransaction();
                    var AddMsg = rejRec.Count > 0 ? $"Cannot delete records with Reference Number(s) {String.Join(",", rejRec)}. Only the filed applications can be deleted." : "";
                    return new ReturnSet { Message = $"{deletedCnt} records(s) deleted. {AddMsg}",Type = ReturnType.Result };
                }
                catch (Exception ex)
                {
                    db.Database.RollbackTransaction();
                    return ExceptionLogger(ex, Session);
                }
            }
        }
        public override ReturnSet ExecuteAction()
        {
            try
            {
                var action = Parameter["Action"].ToObject<ActionModel>();//use parameter inside the case block

                switch (action.Name) 
                {
                    case "UnpostOvertime":
                        var id = action.Parameter["ID"].IsNull(0).ToInt32();
                        return UnpostOvertime(id);
                    case "PostOvertime":
                        var id2 = action.Parameter["ID"].IsNull(0).ToInt32();
                        return PostOvertime(id2);
                    default:
                        var msg = $"Couldn't find the {action.Name} action from the action list. Check your ExecuteAction method.";
                        Logger.LogError(ref msg,"ExecuteAction -> LoadLookup",Session.Name,"InSys.ITI.Overtime");
                        throw new Exception("An error occured. Please contact your Administrator.");
                };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex,Session);
            }
        }
        private vOvertime GetOne(DbContext db, int id)
        {
            return db.ExecQuery<vOvertime>("SELECT o.*,fs.Name FilingStatus FROM tOvertime o left join tFilingStatus fs on fs.id = o.id_FilingStatus WHERE o.ID = {0}", id).SingleOrDefault();
        }
        private ReturnSet UnpostOvertime(int id)
        {
            using (var db = new OvertimeDB(Session))
            {
                var rec = db.Overtimes.SingleOrDefault(x => x.ID == id);
                if (rec != null)
                {
                    rec.IsPosted = false;
                    rec.CurrentApproverLevel = 1;
                }

                db.SaveChanges();

                return new ReturnSet { Data = GetOne(db,rec.ID), Type = ReturnType.Result};
            }
        }
        private ReturnSet PostOvertime(int id)
        {
            using (var db = new OvertimeDB(Session))
            {
                var rec = db.Overtimes.SingleOrDefault(x => x.ID == id);
                if (rec != null)
                {
                    rec.IsPosted = true;
                }

                db.SaveChanges();

                return new ReturnSet { Data = GetOne(db, rec.ID), Type = ReturnType.Result };
            }
        }
    }



    public class Validate : BaseModule
    {
        public tOvertime Overtime { get; set; }

        public Validate(tOvertime _overtime, BrowserSession _Session)
        {
            this.Session = _Session;

            Overtime = _overtime;

            General();
        }


        public void General()
        {
            try
            {






            }
            catch (Exception ex)
            {

                throw ex;
            }

        }




        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            throw new NotImplementedException();
        }
    }

}
