using InSys.Helper;
using InSys.ITI.Common;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.LeaveCredit
{
    public class LeaveCredit : BaseModule
    {
        private string _TableName = "tLeaveCredit";
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new LeaveCredit(_Session, _Parameter);
        }
        public LeaveCredit(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public LeaveCredit() { }


        public override ReturnSet LoadList()
        {
            try
            {
                var options = Parameter["data"].ToObject<TableOptions>();
                using (var db = new LeaveCreditDb(Session))
                {
                    var data = db.QueryTable<vLeaveCreditList>(@"(SELECT la.ID, la.RefNum, la.Name, la.[Description], la.ID_CreatedBy, vue.EmployeeName AS CreatedBy, CreatedAt,
	                                                                vue2.EmployeeName AS ModifiedBy, la.ModifiedAt,	la.IsPosted, la.IsLocked, la.ID_Company
                                                                FROM tLeaveCredit la
	                                                                LEFT OUTER JOIN vUserEmployee vue ON vue.ID = la.ID_CreatedBy
	                                                                LEFT OUTER JOIN vUserEmployee vue2 ON vue2.ID = la.ID_ModifiedBy
                                                                WHERE la.ID_Company = {0})a", options, Session.ID_Company);
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
                var id = Parameter["ID"].IsNull(0).ToInt32();
                using (var db = new LeaveCreditDb(Session))
                {
                    var record = db.Single<tLeaveCredit>(@"SELECT * FROM tLeaveCredit WHERE ID = {0}", id);
                    return new ReturnSet
                    {
                        Data = new
                        {
                            Form = record.IsNull(new tLeaveCredit()
                            {
                                RefNum = Helpers.getReferenceNumber(_TableName, Session.ID_Company.ToInt32()),
                                ID_Company = Session.ID_Company.ToInt32(),
                                ID_CreatedBy = Session.ID_User,
                                CreatedAt = DateTime.Now,
                                ID_ModifiedBy = Session.ID_User,
                                ReferenceDate = DateTime.Now
                            }),
                            Schema = Helpers.GetSchema(_TableName),
                            DetailSchema = Helpers.GetSchema("tLeaveCredit_Detail")
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
            var LeaveCreditData = Parameter["Data"].ToObject<tLeaveCredit>();
            var LeaveCreditDetailData = Parameter["Details"].ToObject<List<tLeaveCredit_Detail>>();
            var LeaveCreditDeletedData = Parameter["DetailsToDelete"].ToObject<List<int>>();
            int LeaveCreditId = LeaveCreditData.ID;
            int ReturnId = 0;
            try
            {
                using (var db = new LeaveCreditDb(Session))
                {
                    try {
                        db.Database.BeginTransaction();

                        LeaveCreditData.ID_CreatedBy = Session.ID_User;
                        LeaveCreditData.CreatedAt = DateTime.Now;
                        LeaveCreditData.ID_ModifiedBy = Session.ID_User;
                        LeaveCreditData.ModifiedAt = DateTime.Now;
                        LeaveCreditData.ID_Company = Session.ID_Company.ToInt32();

                        LeaveCreditData.RefNum = Helpers.getReferenceNumber(_TableName, Session.ID_Company.ToInt32());

                        if (LeaveCreditId == 0)
                        {
                            db.Add(LeaveCreditData);
                            db.SaveChanges(true);
                            ReturnId = LeaveCreditData.ID;
                            PostLeaveCreditDetails(LeaveCreditDetailData, ReturnId, db);
                        }
                        //Edit
                        else if (LeaveCreditId > 0)
                        {
                            var LeaveCreditToUpdate = db.Single<tLeaveCredit>("select * from dbo.tLeaveCredit where ID = {0}", LeaveCreditId);

                            if (LeaveCreditToUpdate != null)
                            {
                                db.Update(LeaveCreditData);
                                db.SaveChanges(true);
                                ReturnId = LeaveCreditData.ID;
                                RemoveLeaveAjustmentDetails(LeaveCreditDeletedData, db);
                                PostLeaveCreditDetails(LeaveCreditDetailData, ReturnId, db);
                            }
                            else { throw new Exception("Data does not exists"); }
                        }

                        db.Database.CommitTransaction();
                    }
                    catch (Exception ex) {
                        db.Database.RollbackTransaction();
                        throw new Exception(ex.Message);
                    }
                }

                return new ReturnSet() { Data = ReturnId, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                
                return ExceptionLogger(ex, Session);
            }
        }

        public void PostLeaveCreditDetails(List<tLeaveCredit_Detail> DetailData, int ID_LeaveCredit, LeaveCreditDb db) {
            try {
                foreach (var detail in DetailData) {
                    detail.ID_LeaveCredit = ID_LeaveCredit;
                    if (db.Any("(SELECT Id FROM tLeaveCredit_Detail WHERE ID = {0})a", detail.Id))
                        db.Update(detail);
                    else db.Add(detail);
                }
                db.SaveChanges();
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        
        }

        public void RemoveLeaveAjustmentDetails(List<int> Ids, LeaveCreditDb db) {
            try
            {
                foreach (var id in Ids) {
                    var detailToDelete = db.Single<tLeaveCredit_Detail>("SELECT * FROM tLeaveCredit_Detail WHERE Id = {0}", id);
                    if (detailToDelete != null){
                        db.Remove(detailToDelete);
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                var ids = Parameter["data"].ToObject<List<int>>();
                using (var db = new LeaveCreditDb(Session))
                {
                    if (ids.Count > 0)
                    {
                        foreach (var id in ids)
                        {
                            var rec = db.tLeaveCredit.SingleOrDefault(x => x.ID == id);
                            if (rec != null) db.Remove(rec);
                        }
                    }
                    db.SaveChanges();
                    return new ReturnSet() { Message = "Successfully Deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }


        public override ReturnSet LoadLookup()
        {
            try {
                string lookupName = Parameter["Name"].ToString().ToLower();
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                return CommonHelper.GetLookUp(lookupName, to, Session);
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        

    }
}
