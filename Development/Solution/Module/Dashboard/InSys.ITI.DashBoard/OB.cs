using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.DashBoard
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
                    var data = db.QueryTable<vOB>(
                                    @"(SELECT o.ID, o.RefNum, e.EmployeeName, o.FiledDate, o.StartDate, o.EndDate, o.Reasons, o.ID_Company,
	                                    fs.Name AS Status, e2.EmployeeName AS LastModifiedBy, o.ModifiedAt
                                    FROM tOB o
	                                    INNER JOIN tFilingStatus fs ON fs.ID = o.ID_FilingStatus
	                                    LEFT OUTER JOIN vEmployees e ON e.ID_Employee = o.ID_Employee
	                                    LEFT OUTER JOIN vEmployees e2 ON e2.ID_Employee = o.ID_ModifiedBy)a", to);

                    return new ReturnSet() { Data = data.Data.ToList(), Type = ReturnType.Result };
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
                var paramId = Parameter["Id"].ToInt32();
                using (var db = new OBDb(Session))
                {
                    var data = db.Single<tOB>("SELECT * FROM tOB WHERE ID = {0}", paramId);

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tOB()),
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
            try
            {
                var OBData = Parameter["data"].ToObject<tOB>();
                var OBDetailData = Parameter["detailRecord"].ToObject<List<tOB_Detail>>();
                var DeletedIds = Parameter["DeletedIds"].ToObject<List<int>>();
                using (var db = new OBDb(Session))
                {

                    if (OBData.ID == 0)
                    {
                        db.Add(OBData);
                        db.SaveChanges(true);
                        PostOBDetail(OBData.ID, db, OBDetailData);
                    }
                    else
                    {
                        db.Update(OBData);
                        db.SaveChanges(true);
                        PostOBDetail(OBData.ID, db, OBDetailData);
                        RemoveOBDetails(db, DeletedIds);
                    }

                    return new ReturnSet() { Data = OBData.ID, Type = ReturnType.Result };

                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }

        }

        public virtual void PostOBDetail(int RecordId, OBDb db, List<tOB_Detail> OBDetails)
        {
            try
            {
                foreach (var detail in OBDetails)
                {
                    detail.ID_OB = RecordId;
                    if (db.Any("SELECT ID FROM tOB_Detail WHERE ID = {0}", detail.Id))
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

        public override ReturnSet Post()
        {
            try
            {
                var RecordId = Parameter["ID"].ToInt32();
                using (var db = new OBDb(Session))
                {
                    db.ExecNonQuery("UPDATE tOB SET IsPosted = 1 WHERE ID = {0}", RecordId);
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
                using (var db = new OBDb(Session))
                {
                    db.ExecNonQuery("UPDATE tOB SET IsPosted = 0 WHERE ID = {0}", RecordId);
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
                var OBIds = Parameter["Ids"].ToObject<List<int>>();
                using (var db = new OBDb(Session))
                {
                    db.ExecNonQuery("DELETE FROM tOB WHERE ID IN ({0})", String.Join(",", OBIds));
                    db.ExecNonQuery("DELETE FROM tOB_Detail WHERE ID_OB IN ({0})", String.Join(",", OBIds));
                    //foreach (var ids in MissedLogIds) {
                    //    var DeleteRecord = db.Single<tOB>("SELECT * FROM tOB WHERE Id = {0}", ids);
                    //    var DeleteDetailRecord = db.ExecQuery<tMissedLogDetail>("SELECT * FROM tOB_Detail WHERE ID_OB = {0}", DeleteRecord.ID);
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



    }
}
