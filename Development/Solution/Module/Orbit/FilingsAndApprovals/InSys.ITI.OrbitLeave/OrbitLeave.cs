using InSys.Helper;
using InSys.ITI.Common;
using InSys.ITI.Controller;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;
using z.Data.JsonClient;

namespace InSys.ITI.OrbitLeave
{
    public class OrbitLeave : BaseModule
    {

        private string _MenuCode = "OrbitFilings&ApprovalsLeave";
        private int ID_Company;
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new OrbitLeave(_Session, _Parameter);
        }
        public OrbitLeave(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
            this.ID_Company = Session.ID_Company.ToInt32();
        }
        public OrbitLeave() { }

        public override ReturnSet LoadList()
        {
            try
            {
                var options = Parameter["data"].ToObject<TableOptions>();
                using (var db = new OrbitLeaveDb(Session))
                {
                    var data = db.QueryTable<vOrbitLeaveList>(@"(SELECT * FROM dbo.fOrbitLeaveList({0}, 0))a", options, ID_Company);
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
                using (var db = new OrbitLeaveDb(Session))
                {

                    id = id == 0 ? -1 : id;
                    var record = db.Single<vOrbitLeaveList>(@"SELECT * FROM dbo.fOrbitLeaveList({0}, {1})", Session.ID_Company, id);

                    return new ReturnSet
                    {
                        Data = new
                        {
                            Form = record.IsNull(new tLeave
                            {
                                RefNum = "LV-XXXXXX", //Helpers.getReferenceNumber("tLeave", Session.ID_Company.ToInt32())
                                ID_Company = ID_Company,
                                ID_CreatedBy = Session.ID_User,
                                CreatedAt = DateTime.Now,
                                ID_FilingStatus = 1, // Filed status
                                ID_Employee = Session.ID_Employee.IsNull(0).ToInt32(),
                                StartDate = DateTime.Now,
                                EndDate = DateTime.Now,
                                FiledDate = DateTime.Now
                            }),
                            Schema = Helpers.GetSchema("tLeave")
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
            var LeaveData = Parameter["Data"].ToObject<tLeave>();
            string ErrorMessage = "";
            //Validate duplicate record
            //Validate if O.T. is conflict with other O.T.
            try
            {
                using (var db = new OrbitLeaveDb(Session))
                {
                    //Validate
                    ErrorMessage = ValidateBeforeSave(LeaveData, db);

                    if (ErrorMessage != "") throw new Exception(ErrorMessage);


                    if (db.Any("(SELECT Id FROM tLeave WHERE ID = {0})a", LeaveData.ID))
                    {
                        LeaveData.ID_ModifiedBy = Session.ID_User;
                        LeaveData.ModifiedAt = DateTime.Now;
                        db.Update(LeaveData);
                        db.SaveChanges();
                    }
                    else
                    {

                        ErrorMessage = CheckPosted(LeaveData, db); //Do not allow posted or approved to be modified
                        if (ErrorMessage != "") throw new Exception(ErrorMessage);

                        LeaveData.ID_CreatedBy = Session.ID_User;
                        LeaveData.CreatedAt = DateTime.Now;
                        LeaveData.ID_Company = Session.ID_Company.ToInt32();
                        LeaveData.RefNum = Helpers.getReferenceNumber("tLeave", Session.ID_Company.ToInt32());
                        db.Add(LeaveData);
                        db.SaveChanges();
                        Helpers.updateReferenceNum("tLeave", Session.ID_Company.ToInt32(), Session);
                    }

                    return new ReturnSet() { Data = LeaveData.ID, Type = ReturnType.Result };
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

                //Validate Dependency
                //Do not delete posted
                //Do not delete approved
                //Do not delete if timesheet is processed.
                var msg = "";
                var ids = Parameter["data"].ToObject<List<int>>();
                using (var db = new OrbitLeaveDb(Session))
                {
                    if (ids.Count > 0)
                    {
                        foreach (var id in ids)
                        {
                            var _msg = db.ExecScalarString("SELECT RefNum AS Value FROM tLeave WHERE Id = {0} AND IsPosted = 1", id);
                            var rec = db.tLeave.SingleOrDefault(x => x.ID == id);
                            if (rec != null) db.Remove(rec);

                            if (_msg != null)
                            {
                                msg += _msg + ", ";
                            }
                        }

                        if (msg != "")
                        {
                            msg = msg.Substring(0, msg.Length - 2);
                            throw new Exception("The following posted record(s) are not allowed to delete. [" + msg + "]");
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

        public string ValidateBeforeSave(tLeave data, OrbitLeaveDb db)
        {
            string msg = "";
            try
            {
                //Check duplicate O.T. filing
                var DuplicatedReferenceNumber = db.ExecScalarString(@"SELECT RefNum AS Value
                                                                    FROM tLeave o
                                                                        INNER JOIN vEmployees e ON e.ID_Employee = o.ID_Employee
                                                                    WHERE o.ID <> {0}
	                                                                    AND o.ID_Employee = {1}
	                                                                    AND ({2} BETWEEN o.StartDate AND o.EndDate
		                                                                    OR {3} BETWEEN o.StartDate AND o.EndDate
		                                                                    OR o.StartDate BETWEEN {2} AND {3}
		                                                                    OR o.EndDate BETWEEN {2} AND {3})", data.ID, data.ID_Employee, data.StartDate, data.EndDate);

                if (DuplicatedReferenceNumber != null)
                    msg += "Your filing's Start Date/End Date is conflict with record [" + DuplicatedReferenceNumber.ToString() + "].";

                //Check Start Time and End Time Conflict

                if (data.StartDate > data.EndDate)
                    msg += "Your Start Date is greater than End Date";


            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return msg;
        }

        public string CheckPosted(tLeave data, OrbitLeaveDb db)
        {
            string msg = "";
            try
            {
                if (data.IsPosted != true && data.ID_FilingStatus != 2)
                {
                    var EmployeeReference = db.ExecScalarString(@"SELECT RefNum AS Value
                                                            FROM tLeave
                                                            WHERE Id = {0} AND (
	                                                            IsPosted = 1
	                                                            OR ID_FilingStatus = 2
                                                            )", data.ID);

                    if (EmployeeReference != null)
                        msg = "Posted/Approved filing  [" + EmployeeReference.ToString() + "] is not allowed to update.";
                }
                else msg = "";

            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return msg;
        }

        public override ReturnSet LoadLookup()
        {
            try
            {
                string lookupName = Parameter["LookupName"].ToString().ToLower();
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                return CommonHelper.GetLookUp(lookupName, to, Session);
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet Post()
        {
            try
            {
                Save();
                var Id = Parameter["ID"].ToInt32();
                using (var db = new OrbitLeaveDb(Session))
                {
                    db.ExecNonQuery("Update tLeave SET IsPosted = 1 WHERE (Id = {0} AND IsPosted <> 1)", Id);
                    return new ReturnSet { Data = Id, Message = "Successfully Posted.", Type = ReturnType.Result };
                }
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
                var Id = Parameter["ID"].ToInt32();
                using (var db = new OrbitLeaveDb(Session))
                {
                    db.ExecNonQuery("Update tLeave SET IsPosted = 0, ID_FilingStatus = 1 WHERE (Id = {0} AND IsPosted <> 0)", Id);
                    return new ReturnSet { Data = Id, Message = "Successfully Unposted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet Approve()
        {
            try
            {
                var Data = Parameter["Data"].ToObject<tLeave>();
                var ApprovalHistory = new tApprovalHistory();
                string Comment = Parameter["ApproverComment"].ToString();
                string Msg = "";
                using (var db = new OrbitLeaveDb(Session))
                {
                    //Update Filing Status
                    if (Data.ID_FilingStatus != 2)
                    {
                        db.ExecNonQuery(@"UPDATE tLeave SET ID_FilingStatus = 2, ID_ApprovedBy = {0} WHERE (Id = {1} AND ID_FilingStatus <> 2)", Session.ID_User, Data.ID);
                        ApprovalHistory = new tApprovalHistory()
                        {
                            ID_Reference = Data.ID,
                            MenuCode = _MenuCode,
                            ID_Employee = 0, //Since this is Orbit only the user id will be saved.
                            ID_CreatedBy = Session.ID_User, //Since this is Orbit only the user id will be saved.
                            ID_TargetEmployee = Data.ID_Employee,
                            ApprovalAction = "Approved",
                            ApprovalLevel = -1,
                            ApproverComment = Comment,
                            DateTimeCreated = DateTime.Now
                        };
                        CommonHelper.PostApprovalHistory(ApprovalHistory, Session);
                    }
                    return new ReturnSet { Data = Data.ID, Message = Msg, Type = ReturnType.Result };
                }

            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet Disapprove()
        {
            try
            {
                var Data = Parameter["Data"].ToObject<tLeave>();
                var ApprovalHistory = new tApprovalHistory();
                string Comment = Parameter["ApproverComment"].ToString();
                string Msg = "";
                using (var db = new OrbitLeaveDb(Session))
                {
                    if (Data.ID_FilingStatus != 3)
                    {
                        db.ExecNonQuery(@"UPDATE tLeave SET ID_FilingStatus = 3, ID_ApprovedBy = NULL WHERE (Id = {0} AND ID_FilingStatus <> 3)", Data.ID);
                        ApprovalHistory = new tApprovalHistory()
                        {
                            ID_Reference = Data.ID,
                            MenuCode = _MenuCode,
                            ID_Employee = 0, //Since this is Orbit only the user id will be saved.
                            ID_CreatedBy = Session.ID_User, //Since this is Orbit only the user id will be saved.
                            ID_TargetEmployee = Data.ID_Employee,
                            ApprovalAction = "Disapproved",
                            ApprovalLevel = -1,
                            ApproverComment = Comment,
                            DateTimeCreated = DateTime.Now
                        };
                        CommonHelper.PostApprovalHistory(ApprovalHistory, Session);
                    }
                    return new ReturnSet { Data = Data.ID, Message = Msg, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadApprovalHistory()
        {
            int ID_Reference = Parameter["ID"].ToInt32();
            return CommonHelper.GetApprovalHistory(_MenuCode, ID_Reference, Session);
        }
    }
}
