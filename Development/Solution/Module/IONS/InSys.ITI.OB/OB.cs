using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.OB
{
    public class OB: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new OB(_Session, _Parameter);
        }

        public OB() { }

        public OB(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new OBDb(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vOB>(@"(select * from vOB) a", to);
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
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new OBDb(Session))
                {
                    var data = db.Single<vOB>("SELECT * FROM vOB WHERE ID = {0}", paramId);
                    
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tOB()
                            {
                                ReferenceNo = "OB-XXXXXX",
                                ID_Employee = Session.ID_Employee.ToInt32(),
                                ID_Company = Session.ID_Company.ToInt32(),
                                ID_FilingStatus = 1,
                                FiledDate = DateTime.Now,
                                CreatedAt = DateTime.Now,
                                WorkDate = DateTime.Now,
                                ID_CreatedBy = Session.ID_Employee.ToInt32(),
                                ID_ModifiedBy = Session.ID_Employee.ToInt32(),
                                ModifiedAt = DateTime.Now,
                                ApproverLevel = 1
                            }),
                            Schema = Helpers.GetSchema("tOB")
                        }
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
            using (var db = new OBDb(Session))
            {
                db.Database.BeginTransaction();
                try
                {
                   
                    var OBData = Parameter["Data"].ToObject<tOB>();
                    var post = Parameter["Post"].IsNull(false).ToBool();
                    //var refDate = DateTime.Parse(OBData.WorkDate.ToString("MM/dd/yyyy"));
                    
                    new ValidateOB(OBData, Session);

                    if (OBData.EndTime < OBData.StartTime)
                    {
                        OBData.EndTime = OBData.EndTime.AddDays(1);
                    }
                    //OBData.StartMinute = OBData.StartTime.ToDatetimeMinute(refDate);
                    //OBData.EndMinute = OBData.EndTime.ToDatetimeMinute(refDate);
                    //OBData.ComputedHours = OBData.EndTime.Subtract(OBData.StartTime).TotalHours.ToInt32();

                    if (post)
                        OBData.IsPosted = true;

                    if (OBData.ID == 0)
                    {
                        OBData.ReferenceNo = Helpers.getReferenceNumber("tOB", Session.ID_Company.ToInt32());
                        db.Add(OBData);
                        Helpers.updateReferenceNum("tOB", Session.ID_Company.ToInt32(), Session);
                    }
                    else
                    {
                        if (db.tOBs.Any(x => x.ID == OBData.ID))
                            db.Update(OBData);
                        else
                        {
                            OBData.ReferenceNo = Helpers.getReferenceNumber("tOB", Session.ID_Company.ToInt32());
                            db.Add(OBData);
                            Helpers.updateReferenceNum("tOB", Session.ID_Company.ToInt32(), Session);
                        }

                    }
                    db.SaveChanges();
                    db.Database.CommitTransaction();

                    return new ReturnSet() { Data = GetOne(db, OBData.ID), Type = ReturnType.Result };
                }
                catch (Exception ex)
                {
                    db.Database.RollbackTransaction();

                    if (IsValidateError)
                    {
                        return new ReturnSet() { Message = ex.Message, Type = ReturnType.ValidationError };
                    }
                    else
                    {
                        return ExceptionLogger(ex, Session);
                    }
                    
                }
            }

        }

        //public virtual void PostOBDetail(int RecordId, OBDb db, List<tOB_Detail> OBDetails)
        //{
        //    try
        //    {
        //        foreach (var detail in OBDetails)
        //        {
        //            detail.ID_OB = RecordId;
        //            if (db.Any("SELECT ID FROM tOB_Detail WHERE ID = {0}", detail.Id))
        //                db.Add(detail);
        //            else db.Update(detail);
        //        }
        //        db.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

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
                        Logger.LogError(ref msg, "ExecuteAction -> LoadLookup", Session.Name, "InSys.ITI.OB");
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
            using (var db = new OBDb(Session))
            {
                var rec = db.tOBs.SingleOrDefault(x => x.ID == id);
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
            using (var db = new OBDb(Session))
            {
                var rec = db.tOBs.SingleOrDefault(x => x.ID == id);
                if (rec != null)
                {
                    rec.IsPosted = true;
                }

                db.SaveChanges();

                return new ReturnSet { Data = GetOne(db, rec.ID), Type = ReturnType.Result };
            }
        }

        private vOB GetOne(DbContext db, int id)
        {
            return db.ExecQuery<vOB>("SELECT * FROM vOB WHERE ID = {0}", id).SingleOrDefault();
        }

        public virtual void RemoveOBDetails(OBDb db, List<int> DeleteRecords)
        {
            try
            {
                foreach (var id in DeleteRecords)
                {
                    var deletedData = db.Single<tOB_Detail>("SELECT * FROM tOB_Detail WHERE ID = {0}", id);
                    if (deletedData != null) db.Remove(deletedData);
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public override ReturnSet Approve()
        {
            return base.Approve();
        }

        public override ReturnSet Disapprove()
        {
            return base.Disapprove();
        }
        
        public override ReturnSet LoadDetail()
        {
            try
            {
                var paramId = Parameter["ID"].ToInt32();
                using (var db = new OBDb(Session))
                {
                    var data = db.ExecQuery<tOB_Detail>("SELECT * FROM tOB_Detail WHERE ID_OB = {0}", paramId);
                    return new ReturnSet() { Data = data, Type = ReturnType.Result };

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
                using (var db = new OBDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tOB>("select * from dbo.tOB where ID = {0}", id);
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

        public override ReturnSet LoadLookup()
        {
            try
            {
                string lookupName = Parameter["LookupName"].ToString().ToLower();
                switch (lookupName)
                {
                    case "client":
                        return LoadClient();
                    case "project":
                        return LoadProject();
                    default:
                        throw new Exception("Lookup not found.");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual ReturnSet LoadClient()
        {
            var to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new OBDb(Session))
            {
                var data = db.QueryTable<tLookUpData>("(select ID, Name, IsActive from dbo.tClient where IsActive = 1)a", to);
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }

        public virtual ReturnSet LoadProject()
        {
            var to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new OBDb(Session))
            {
                var data = db.QueryTable<tLookUpData>("(select ID, Name, IsActive from dbo.tProject where IsActive = 1)a", to);
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }
    }

    public class ValidateOB : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            throw new NotImplementedException();
        }
        public ValidateOB(tOB OBData, BrowserSession session)
        {
            this.Session = session;
            CheckDuplicateFiling(OBData.WorkDate, OBData.StartTime, OBData.EndTime, OBData.ID);
            CheckFilingCutOff(OBData.WorkDate);
        }

        public void CheckDuplicateFiling(DateTime WorkDate, DateTime StartTime, DateTime EndTime, int OBID)
        {
            try
            {
                using (var db = new OBDb(Session))
                {
                    if (db.Any("(SELECT ID FROM tOB WHERE ID_Employee = {3} AND ID NOT IN (ISNULL({4}, 0)) AND (CONCAT(CAST({0} AS DATE), ' ', FORMAT({2}, 'HH:mm:ss')) >= CONCAT(CAST(WorkDate AS DATE), ' ', FORMAT(StartTime, 'HH:mm:ss'))) AND (CONCAT(CAST({0} AS DATE), ' ', FORMAT({1}, 'HH:mm:ss')) <= CONCAT(CAST(WorkDate AS DATE), ' ', FORMAT(EndTime, 'HH:mm:ss')))) a", WorkDate, StartTime, EndTime, Session.ID_Employee, OBID))
                    {
                        IsValidateError = true;
                        throw new Exception("There's already filed OB with same date/the OB filing is overlapped!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CheckFilingCutOff(DateTime WorkDate)
        {
            try
            {
                var with2CutOff = true;

                if (with2CutOff)
                {
                    var cutoffS = getCutOffDate(1);
                    var cutoffE = getCutOffDate(15);
                    var cutoff2S = getCutOffDate(16);
                    var cutoff2E = getCutOffDate(100);

                    if (DateTime.Now > cutoffS && DateTime.Now < cutoffE)
                    {
                        if (WorkDate < cutoffS || WorkDate > cutoffE)
                        {
                            IsValidateError = true;
                            throw new Exception("Can't file outside cutoff!");
                        }
                    }

                    if (DateTime.Now > cutoff2S && DateTime.Now < cutoff2E)
                    {
                        if (WorkDate < cutoff2S || WorkDate > cutoff2E)
                        {
                            IsValidateError = true;
                            throw new Exception("Can't file outside cutoff!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static public DateTime getCutOffDate(int cutOff)
        {
            var result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, cutOff == 100 ? DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) : cutOff);
            return result;
        }

    }
}
