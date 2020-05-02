using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.DashBoard
{
    public class Leave: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Leave(_Session, _Parameter);
        }
        
        public Leave(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }
        public Leave() { }

        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new LeaveDb(Session))
                {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vLeave>(
                                    @"(SELECT l.Id, l.RefNum, e.EmployeeName, StartDate, EndDate, l.ID_Company, FiledDate,
	                                    fs.Name AS Status, l.IsPosted, l.Reasons
                                    FROM tLeave l
	                                    INNER JOIN tFilingStatus fs ON fs.ID = l.ID_FilingStatus
	                                    INNER JOIN vEmployees e ON e.ID_Employee = l.ID_Employee)a", to);
                    return new ReturnSet() { Data = new { Rows = data.Data.ToList() }, Type = ReturnType.Result };
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
                var paramId = Parameter["ParamId"].ToInt32();
                using (var db = new LeaveDb(Session))
                {
                    var data = db.Single<tLeave>("SELECT * FROM tLeave WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tLeave()),
                            Schema = Helpers.GetSchema("tLeave")
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
            try
            {
                var LeaveData = Parameter["data"].ToObject<tLeave>();
                var LeaveDetailData = Parameter["detailRecord"].ToObject<List<tLeave_Detail>>();
                var DeletedIds = Parameter["DeletedIds"].ToObject<List<int>>();
                using (var db = new LeaveDb(Session))
                {

                    if (LeaveData.ID == 0)
                    {
                        db.Add(LeaveData);
                        db.SaveChanges(true);
                        PostLeaveDetail(LeaveData.ID, db, LeaveDetailData);
                    }
                    else
                    {
                        db.Update(LeaveData);
                        db.SaveChanges(true);
                        PostLeaveDetail(LeaveData.ID, db, LeaveDetailData);
                        RemoveLeaveDetail(db, DeletedIds);
                    }

                    return new ReturnSet() { Data = LeaveData.ID, Type = ReturnType.Result };

                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }

        }

        public virtual void PostLeaveDetail(int RecordId, LeaveDb db, List<tLeave_Detail> tLeaveDetails)
        {
            try
            {
                foreach (var detail in tLeaveDetails)
                {
                    detail.ID_Leave = RecordId;
                    if (db.Any("SELECT ID FROM tLeave_Detail WHERE ID = {0}", detail.Id))
                        db.Add(detail);
                    else db.Update(detail);
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual void RemoveLeaveDetail(LeaveDb db, List<int> DeleteRecords)
        {
            try
            {
                foreach (var id in DeleteRecords)
                {
                    var deletedData = db.Single<tLeave_Detail>("SELECT * FROM tLeave_Detail WHERE ID = {0}", id);
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
            //Check if employee has more leave.
        }

        public override ReturnSet Disapprove()
        {
            return base.Disapprove();
        }

        public override ReturnSet Post()
        {
            try
            {
                var RecordId = Parameter["ID"].ToInt32();
                using (var db = new LeaveDb(Session))
                {
                    db.ExecNonQuery("UPDATE tLeave SET IsPosted = 1 WHERE ID = {0}", RecordId);
                }
                return new ReturnSet() { Data = RecordId, Message = "All approved will be applied on Timekeeping", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet UnPost()
        {
            try
            {
                var RecordId = Parameter["ID"].ToInt32();
                using (var db = new LeaveDb(Session))
                {
                    db.ExecNonQuery("UPDATE tLeave SET IsPosted = 0 WHERE ID = {0}", RecordId);
                }
                return new ReturnSet() { Data = RecordId, Message = "All approved will not be applied on Timekeeping", Type = ReturnType.Result };
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
                var paramId = Parameter["Id"].ToInt32();
                using (var db = new LeaveDb(Session))
                {
                    var data = db.ExecQuery<tLeave_Detail>("SELECT * FROM tLeave_Detail WHERE ID_Leave = {0}", paramId);
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
                var LeaveIds = Parameter["Ids"].ToObject<List<int>>();
                using (var db = new LeaveDb(Session))
                {
                    db.ExecNonQuery("DELETE FROM tLeave WHERE ID IN ({0})", String.Join(",", LeaveIds));
                    db.ExecNonQuery("DELETE FROM tLeave_Detail WHERE ID_Leave IN ({0})", String.Join(",", LeaveIds));
                    //foreach (var ids in MissedLogIds) {
                    //    var DeleteRecord = db.Single<tMissedLog>("SELECT * FROM tMissedLog WHERE Id = {0}", ids);
                    //    var DeleteDetailRecord = db.ExecQuery<tMissedLogDetail>("SELECT * FROM tMissedLogDetail WHERE ID_MissedLog = {0}", DeleteRecord.ID);
                    //    if (DeleteRecord != null) {
                    //        db.Remove(DeleteRecord);
                    //        db.Remove(DeleteDetailRecord);
                    //    }
                    //}
                    //db.SaveChanges();

                    return new ReturnSet() { Message = "Successfully deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadApproval()
        {
            TableOptions to = Parameter["data"].ToObject<TableOptions>();
            int? ID_Employee = Session.ID_Employee;
            try {
                using (var db = new LeaveDb(Session)) {
                    var fs = to.Filters.Where(x => x.Name == "ID_Company").FirstOrDefault();
                    //if (fs == null)
                    //    to.Filters.Add(new FilterSchema()
                    //    {
                    //        Name = "ID_Company",
                    //        Type = FilterType.IN,
                    //        Value = IIf(Session.ID_Company == 0, OrganizationDb.CompanyRights(Ctx.Session.Get<int>("ID_Roles")), Ctx.Session.Get<int>("ID_Company"))
                    //    });
                    //else if (fs.Value == null)
                    //{
                    //    fs.Type = FilterType.IN;
                    //    fs.Value = IIf(Ctx.Session.Get<int>("ID_Company") == 0, OrganizationDb.CompanyRights(Ctx.Session.Get<int>("ID_Roles")), Ctx.Session.Get<int>("ID_Company"));
                    //}

                    var data = db.QueryTable<vApprovalLeave>(Helpers.GetSqlQuery("vApprovalLeave"), to, ID_Employee).Data.ToList();
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data }, Type = ReturnType.Result };
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);  
            }
        }
    }
}
