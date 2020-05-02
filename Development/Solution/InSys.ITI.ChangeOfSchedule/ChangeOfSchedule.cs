using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.ChangeOfSchedule
{
    public class ChangeOfSchedule : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new ChangeOfSchedule(_Session, _Parameter);
        }
        public ChangeOfSchedule() { }
        public ChangeOfSchedule(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new ChangeOfScheduleDb(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vChangeOfSchedule>("(select * from vChangeOfSchedule) a", to);
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

        public override ReturnSet LoadForm()
        {
            try
            {
                using (var db = new ChangeOfScheduleDb(Session))
                {
                    var paramId = Parameter["ID"].IsNull(0).ToInt32();
                    var data = db.Single<vChangeOfSchedule>(@"select * from vChangeOfSchedule where ID = {0}", paramId);

                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var empSched = db.QueryTable<vGetWeeklySchedule>($"(select * from dbo.vGetWeeklySchedule where ID = { Session.ID_Employee.IsNull(0).ToInt32() }) a", to);
                    //var empSched = db.ExecQuery<vGetWeeklySchedule>("select * from dbo.vGetWeeklySchedule where ID = {0}", Session.ID_Employee.IsNull(0).ToInt32());

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new vChangeOfSchedule()
                            {
                                ReferenceNo = "CS-XXXXXX",
                                ID_Employee = Session.ID_Employee.ToInt32(),
                                ID_Company = Session.ID_Company.ToInt32(),
                                ID_FilingStatus = 1,
                                FiledDate = DateTime.Now,
                                CreatedAt = DateTime.Now,
                                StartDate = DateTime.Now,
                                EndDate = DateTime.Now,
                                ID_CreatedBy = Session.ID_Employee.ToInt32(),
                                ID_ModifiedBy = Session.ID_Employee.ToInt32(),
                                ModifiedAt = DateTime.Now,
                                ApproverLevel = 1
                            }),
                            EmpWeeklySchedule = empSched.Data.ToList(),
                            Schema = Helpers.GetSchema("tChangeOfSchedule"),
                            SchemaDetail = Helpers.GetSchema("tChangeOfSchedule_Detail")
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
                using (var db = new ChangeOfScheduleDb(Session))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {

                        try
                        {
                            var COSData = Parameter["data"].ToObject<tChangeOfSchedule>();
                            var post = Parameter["Post"].IsNull(false).ToBool();
                            var COSData_Detail = Parameter["detail"].ToObject<List<tChangeOfSchedule_Detail>>();

                            if (post)
                                COSData.IsPosted = true;

                            if (db.Any("(select * from dbo.tChangeOfSchedule where ID = {0})a", COSData.ID))
                            {
                                db.Update(COSData);
                            }
                            else
                            {
                                COSData.ReferenceNo = Helpers.getReferenceNumber("tChangeofSchedule", Session.ID_Company.ToInt32());
                                db.Add(COSData);
                                Helpers.updateReferenceNum("tChangeofSchedule", Session.ID_Company.ToInt32(), Session);
                            }

                            db.ExecNonQuery("Delete From dbo.tChangeOfSchedule_Detail Where ID_ChangeOfSchedule = {0}", COSData.ID);
                            db.SaveChanges(true);

                            foreach (var d in COSData_Detail)
                            {
                                d.ID = 0;
                                d.ID_ChangeOfSchedule = COSData.ID;
                                db.Add(d);
                            }

                            db.SaveChanges(true);
                            tran.Commit();
                            return new ReturnSet() { Data = GetOne(db, COSData.ID), Type = ReturnType.Result };
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

        public override ReturnSet ExecuteAction()
        {
            try
            {
                var action = Parameter["Action"].ToObject<ActionModel>();//use parameter inside the case block

                switch (action.Name)
                {
                    case "Unpost":
                        var idunpost = action.Parameter["ID"].IsNull(0).ToInt32();
                        return Unpost(idunpost);
                    case "Post":
                        var idpost = action.Parameter["ID"].IsNull(0).ToInt32();
                        return Post(idpost);
                    default:
                        var msg = $"Couldn't find the {action.Name} action from the action list. Check your ExecuteAction method.";
                        Logger.LogError(ref msg, "ExecuteAction -> LoadLookup", Session.Name, "InSys.ITI.ChangeOfSChedule");
                        throw new Exception("An error occured. Please contact your Administrator.");
                };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        private ReturnSet Unpost(int id)
        {
            using (var db = new ChangeOfScheduleDb(Session))
            {
                var rec = db.tChangeOfSchedule.SingleOrDefault(x => x.ID == id);
                if (rec != null)
                {
                    rec.IsPosted = false;
                    rec.ApproverLevel = 1;
                }

                db.SaveChanges();

                return new ReturnSet { Data = GetOne(db, rec.ID), Type = ReturnType.Result };
            }
        }

        private ReturnSet Post(int id)
        {
            using (var db = new ChangeOfScheduleDb(Session))
            {
                var rec = db.tChangeOfSchedule.SingleOrDefault(x => x.ID == id);
                if (rec != null)
                {
                    rec.IsPosted = true;
                }

                db.SaveChanges();

                return new ReturnSet { Data = GetOne(db, rec.ID), Type = ReturnType.Result };
            }
        }

        private vChangeOfSchedule GetOne(DbContext db, int id)
        {
            return db.ExecQuery<vChangeOfSchedule>("SELECT * FROM vChangeOfSchedule WHERE ID = {0}", id).SingleOrDefault();
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new ChangeOfScheduleDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tChangeOfSchedule>("select * from dbo.tChangeOfSchedule where ID = {0}", id);
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
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadDetail()
        {
            try
            {
                switch (Parameter["DetailName"].ToString().ToLower())
                {
                    case "cosdetail":
                        return LoadCOSDetail();
                    case "checkapprovedcos":
                        return CheckApprovedCOS();
                    default:
                        throw new Exception("Detail not found.");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual ReturnSet LoadCOSDetail()
        {
            TableOptions to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new ChangeOfScheduleDb(Session))
            {
                var data = db.QueryTable<vChangeOfSchedule_Detail>("(select * from dbo.vChangeOfSchedule_Detail where ID_ChangeOfSchedule = {0})a", to, Parameter["ID_ChangeOfSchedule"].ToInt32());
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }

        public virtual ReturnSet CheckApprovedCOS()
        {
            TableOptions to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new ChangeOfScheduleDb(Session))
            {
                var data = db.QueryTable<vChangeOfSchedule_Detail>("(select * from dbo.vChangeOfSchedule_Detail where ID_Employee = {0} AND CAST(WorkDate AS DATE) = CAST({1} AS DATE))a", to, Session.ID_Employee.IsNull(0).ToInt32(), Parameter["WorkDate"].ToString());
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }

        public override ReturnSet LoadLookup()
        {
            try
            {
                string lookupName = Parameter["LookupName"].ToString().ToLower();
                switch (lookupName)
                {
                    case "shiftschedule":
                        return LoadShiftSchedule();
                    default:
                        throw new Exception("System Error! Please contact your System Administrator");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual ReturnSet LoadShiftSchedule()
        {
            var to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new ChangeOfScheduleDb(Session))
            {
                var data = db.QueryTable<tLookUpData>("(select ID, Name, IsActive from dbo.tShiftSchedule where IsActive = 1)a", to);
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }

    }

    //public class ValidateCOS : BaseModule
    //{
    //    public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    public ValidateCOS(tOB OBData, BrowserSession session)
    //    {
    //        this.Session = session;
    //        CheckDuplicateFiling(OBData.WorkDate, OBData.StartTime, OBData.EndTime, OBData.ID);
    //        CheckFilingCutOff(OBData.WorkDate);
    //    }

    //    public void CheckDuplicateFiling(DateTime WorkDate, DateTime StartTime, DateTime EndTime, int OBID)
    //    {
    //        try
    //        {
    //            using (var db = new ChangeOfScheduleDb(Session))
    //            {
    //                if (db.Any("(SELECT ID FROM tOB WHERE ID_Employee = {3} AND ID NOT IN (ISNULL({4}, 0)) AND (CONCAT(CAST({0} AS DATE), ' ', FORMAT({2}, 'HH:mm:ss')) >= CONCAT(CAST(WorkDate AS DATE), ' ', FORMAT(StartTime, 'HH:mm:ss'))) AND (CONCAT(CAST({0} AS DATE), ' ', FORMAT({1}, 'HH:mm:ss')) <= CONCAT(CAST(WorkDate AS DATE), ' ', FORMAT(EndTime, 'HH:mm:ss')))) a", WorkDate, StartTime, EndTime, Session.ID_Employee, OBID))
    //                {
    //                    IsValidationError = true;
    //                    throw new Exception("There's already filed OB with same date/the OB filing is overlapped!");
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            throw ex;
    //        }
    //    }

    //    public void CheckFilingCutOff(DateTime WorkDate)
    //    {
    //        try
    //        {
    //            var with2CutOff = true;

    //            if (with2CutOff)
    //            {
    //                var cutoffS = getCutOffDate(1);
    //                var cutoffE = getCutOffDate(15);
    //                var cutoff2S = getCutOffDate(16);
    //                var cutoff2E = getCutOffDate(100);

    //                if (DateTime.Now > cutoffS && DateTime.Now < cutoffE)
    //                {
    //                    if (WorkDate < cutoffS || WorkDate > cutoffE)
    //                    {
    //                        IsValidationError = true;
    //                        throw new Exception("Can't file outside cutoff!");
    //                    }
    //                }

    //                if (DateTime.Now > cutoff2S && DateTime.Now < cutoff2E)
    //                {
    //                    if (WorkDate < cutoff2S || WorkDate > cutoff2E)
    //                    {
    //                        IsValidationError = true;
    //                        throw new Exception("Can't file outside cutoff!");
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            throw ex;
    //        }
    //    }

    //    static public DateTime getCutOffDate(int cutOff)
    //    {
    //        var result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, cutOff == 100 ? DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) : cutOff);
    //        return result;
    //    }

    //}
}
