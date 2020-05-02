using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.MissedLog
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
                                    @"(SELECT * FROM vMissedLog where ID_Employee = {0})a", to, Session.ID_Employee);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() } , Type = ReturnType.Result };
                }            
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadForm() {
            try {
                var paramId = Parameter["Id"].ToInt32();
                using (var db = new MissedLogDb(Session)) {
                    var data = db.Single<vMissedLog>("SELECT * FROM vMissedLog WHERE ID = {0}", paramId);

                    var LogType = base.LoadLookup<tLogType>(db, $"SELECT * FROM dbo.tLogType");


                    return new ReturnSet()
                    {
                        Data = new {
                            Form = data.IsNull(new vMissedLog()
                            {
                                RefNum = "ML-XXXXXX",
                                ID_Employee = Session.ID_Employee.ToInt32(),
                                FiledDate = DateTime.Now,
                                ID_CreatedBy = Session.ID_Employee.ToInt32(),
                                ID_ModifiedBy = Session.ID_Employee.ToInt32(),
                                ModifiedAt = DateTime.Now,
                                ID_FilingStatus = 1,
                                ID_Company = Session.ID_Company.ToInt32(),
                                WorkDate = DateTime.Now,
                                CreateAt = DateTime.Now,
                                IsPosted = false

                            }),
                            Schema = Helpers.GetSchema("tMissedLog"),
                            SchemaDetail = Helpers.GetSchema("tMissedLog_Detail"),
                            LogType
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
                using (var db = new MissedLogDb(Session))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {

                        try
                        {
                            var MissedLogData = Parameter["data"].ToObject<tMissedLog>();
                            var MissedLogData_Detail = Parameter["detail"].ToObject<List<tMissedLog_Detail>>();

                            //Validation
                            new Validate(MissedLogData_Detail, MissedLogData, Session);
                            
                            if (db.Any("(select * from dbo.tMissedLog where ID = {0})a", MissedLogData.ID))
                            {
                                db.Update(MissedLogData);
                            }
                            else
                            {
                                MissedLogData.RefNum = Helpers.getReferenceNumber("tMissedLog", Session.ID_Company.ToInt32());
                                db.Add(MissedLogData);
                                Helpers.updateReferenceNum("tMissedLog", Session.ID_Company.ToInt32(), Session);
                            }


                            db.ExecNonQuery("Delete From tMissedLog_Detail Where ID_MissedLog = {0}", MissedLogData.ID);

                            db.SaveChanges(true);

                            foreach (var d in MissedLogData_Detail)
                            {
                                d.Id = 0;
                                d.ID_MissedLog = MissedLogData.ID;
                                db.Add(d);

                            }

                            db.SaveChanges(true);
                            tran.Commit();
                            return new ReturnSet() { Message = MissedLogData.IsPosted ? "Successfully Posted" : "Successfully Saved.", Data = new { ID = MissedLogData.ID, RefNo = MissedLogData.RefNum }, Type = ReturnType.Result };

                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw ex;
                        }
                    }

                }



            } catch (Exception ex) {

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
                var RecordId = Parameter["ID_MissedLog"].ToInt32();
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

            try
            {
                switch (Parameter["detailname"].ToString().ToLower())
                {
                    case "missedlogdetail":
                        return LoadMissedLogDetail();
                    default:
                        throw new Exception("Detail not found.");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual ReturnSet LoadMissedLogDetail()
        {
            using (var db = new MissedLogDb(Session))
            {
                var data = db.ExecQuery<tMissedLog_Detail>("SELECT * FROM tMissedLog_Detail WHERE ID_MissedLog = {0}", Parameter["ID_MissedLog"].ToInt32()).ToList();
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data }, Type = ReturnType.Result };
            }
        }


        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new MissedLogDb(Session)) {

                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var ids = Parameter["Data"].ToObject<List<int>>();

                            //db.RemoveRange(db.tLeave.Where(x => ids.Contains(x.Id)).ToList()); //use this for multiple deleting without iteration

                            foreach (var id in ids)
                            {
                                var DeleteRecord = db.tMissedLog.Where(x => x.ID == id).FirstOrDefault();
                                if (DeleteRecord != null)
                                {
                                    db.Remove(DeleteRecord);
                                    db.RemoveRange(db.tMissedLogDetail.Where(x => x.Id == DeleteRecord.ID).ToList());
                                }
                            }

                            db.SaveChanges();
                            tran.Commit();
                            return new ReturnSet() { Message = "Successfully deleted.", Type = ReturnType.Result };
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }


    }


    public class Validate : BaseModule
    {

        public tMissedLog missLog { get; set; }
        public List<tMissedLog_Detail> missedLogDetails { get; set; }
      

        public Validate(List<tMissedLog_Detail> _missedLogDetails, tMissedLog _missedLog, BrowserSession _Session)
        {
            this.Session = _Session;

            missLog = _missedLog;
            missedLogDetails = _missedLogDetails;

            General();
            
        }


        public void General()
        {
            try
            {
                //Check valid Work Date
                if (missLog.WorkDate >= missLog.FiledDate)
                {
                    IsValidateError = true;
                    throw new Exception("Missed Log should not be filed in advance.");
                }




                //Check for Duplicate filing
                using (var db = new MissedLogDb(Session))
                {
                    
                    if (db.tMissedLog.Where(x => x.WorkDate == missLog.WorkDate && x.ID_Employee == Session.ID_Employee && x.ID != missLog.ID).Any())
                    {
                        IsValidateError = true;
                        throw new Exception("An existing record overlapped with the current filing.");
                    }

                }


              



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
