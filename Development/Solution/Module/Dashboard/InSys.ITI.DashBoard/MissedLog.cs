using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.DashBoard
{
    public class MissedLog: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new MissedLog(_Session, _Parameter);
        }
        public MissedLog() { }
        public MissedLog(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }

        public override ReturnSet LoadList() {
            try {
                using (var db = new MissedLogDb(Session)) {
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vMissedLog>(
                                    @"(SELECT m.ID, m.RefNum, e.EmployeeName, m.FiledDate, m.StartDate, m.EndDate, m.Reasons, m.ID_Company,
	                                    fs.Name AS Status, e2.EmployeeName AS LastModifiedBy, m.ModifiedAt
                                    FROM tMissedLog m
	                                    INNER JOIN tFilingStatus fs ON fs.ID = m.ID_FilingStatus
                                        LEFT OUTER JOIN vEmployees e ON e.ID_Employee = m.ID_Employee
	                                    LEFT OUTER JOIN vEmployees e2 ON e2.ID_Employee = m.ID_ModifiedBy
                                    ORDER BY m.FiledDate)a", to);
                    return new ReturnSet() { Data = data.Data.ToList(), Type = ReturnType.Result };
                }            
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadForm() {
            try {
                var paramId = Parameter["Id"].ToInt32();
                using (var db = new MissedLogDb(Session)) {
                    var data = db.Single<tMissedLog>("SELECT * FROM tMissedLog WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new {
                            Form = data.IsNull(new tMissedLog()),
                            Schema = Helpers.GetSchema("tMissedLog")
                        }
                    };
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        
        }

        public override ReturnSet Save()
        {
            try {
                var MissedLogData = Parameter["data"].ToObject<tMissedLog>();
                var MissedLogDetailData = Parameter["detailRecord"].ToObject<List<tMissedLog_Detail>>();
                var DeletedIds = Parameter["DeletedIds"].ToObject<List<int>>();
                using (var db = new MissedLogDb(Session)) {

                    if (MissedLogData.ID == 0)
                    {
                        db.Add(MissedLogData);
                        db.SaveChanges(true);
                        PostMissedLogDetail(MissedLogData.ID, db, MissedLogDetailData);
                    }
                    else {
                        db.Update(MissedLogData);
                        db.SaveChanges(true);
                        PostMissedLogDetail(MissedLogData.ID, db, MissedLogDetailData);
                        RemoveMissedLogDetails(db, DeletedIds);
                    }

                    return new ReturnSet() { Data = MissedLogData.ID, Type = ReturnType.Result };
                
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }

        }

        public virtual void PostMissedLogDetail(int RecordId, MissedLogDb db, List<tMissedLog_Detail> MissedLogDetails)
        {
            try {
                foreach (var detail in MissedLogDetails) {
                    detail.ID_MissedLog = RecordId;
                    if (db.Any("SELECT ID FROM tMissedLog_Detail WHERE ID = {0}", detail.Id))
                        db.Add(detail);
                    else db.Update(detail);
                }
                db.SaveChanges();
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public virtual void RemoveMissedLogDetails(MissedLogDb db, List<int> DeleteRecords)
        {
            try
            {
                foreach (var id in DeleteRecords) {
                    var deletedData = db.Single<tMissedLog_Detail>("SELECT * FROM tMissedLog_Detail WHERE ID = {0}", id);
                    if (deletedData != null) db.Remove(deletedData);
                }
                db.SaveChanges();
            }
            catch (Exception ex) { 
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

        public override ReturnSet Post()
        {
            try {
                var RecordId = Parameter["ID"].ToInt32();
                using (var db = new MissedLogDb(Session)) {
                    db.ExecNonQuery("UPDATE tMissedLog SET IsPosted = 1 WHERE ID = {0}", RecordId);
                }
                return new ReturnSet() { Data = RecordId, Message="All approved will be applied on Timekeeping", Type = ReturnType.Result };
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet UnPost()
        {
            try
            {
                var RecordId = Parameter["ID"].ToInt32();
                using (var db = new MissedLogDb(Session))
                {
                    db.ExecNonQuery("UPDATE tMissedLog SET IsPosted = 0 WHERE ID = {0}", RecordId);
                }
                return new ReturnSet() { Data = RecordId, Message = "All approved will not be applied on Timekeeping", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadDetail() {
            try {
                var paramId = Parameter["Id"].ToInt32();
                using (var db = new MissedLogDb(Session)) {
                    var data = db.ExecQuery<tMissedLog_Detail>("SELECT * FROM tMissedLog_Detail WHERE ID_MissedLog = {0}", paramId);
                    return new ReturnSet() { Data = data, Type = ReturnType.Result };
                
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                var MissedLogIds = Parameter["Ids"].ToObject<List<int>>();
                using (var db = new MissedLogDb(Session)) {
                    db.ExecNonQuery("DELETE FROM tMissedLog WHERE ID IN ({0})", String.Join(",", MissedLogIds));
                    db.ExecNonQuery("DELETE FROM tMissedLog_Detail WHERE ID_MissedLog IN ({0})", String.Join(",", MissedLogIds));
                    //foreach (var ids in MissedLogIds) {
                    //    var DeleteRecord = db.Single<tMissedLog>("SELECT * FROM tMissedLog WHERE Id = {0}", ids);
                    //    var DeleteDetailRecord = db.ExecQuery<tMissedLogDetail>("SELECT * FROM tMissedLog_Detail WHERE ID_MissedLog = {0}", DeleteRecord.ID);
                    //    if (DeleteRecord != null) {
                    //        db.Remove(DeleteRecord);
                    //        db.Remove(DeleteDetailRecord);
                    //    }
                    //}
                    //db.SaveChanges();

                    return new ReturnSet() { Message = "Successfully deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }


    }
}
