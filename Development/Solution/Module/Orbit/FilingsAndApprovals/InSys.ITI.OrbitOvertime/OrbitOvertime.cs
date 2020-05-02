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

namespace InSys.ITI.OrbitOvertime
{
    public class OrbitOvertime : BaseModule
    {

        private string _MenuCode = "OrbitFilings&ApprovalsOvertime";
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new OrbitOvertime(_Session, _Parameter);
        }
        public OrbitOvertime(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public OrbitOvertime() { }

        public override ReturnSet LoadList()
        {
            try
            {
                var options = Parameter["data"].ToObject<TableOptions>();
                using (var db = new OrbitOvertimeDb(Session))
                {
                    var data = db.QueryTable<vOrbitOvertimeList>(@"(SELECT * FROM dbo.fOrbitOvertimeList({0}, 0))a", options, Session.ID_Company);
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


        public override ReturnSet LoadForm() {
            try {
                var id = Parameter["ID"].IsNull(0).ToInt32();
                using (var db = new OrbitOvertimeDb(Session)) {

                    id = id == 0 ? -1 : id;
                    var record = db.Single<vOrbitOvertimeList>(@"SELECT * FROM dbo.fOrbitOvertimeList({0}, {1})", Session.ID_Company, id);

                    return new ReturnSet
                    {
                        Data = new
                        {
                            Form = record.IsNull(new tOvertime
                            {
                                ReferenceNo = "OT-XXXXXX", //Helpers.getReferenceNumber("tOvertime", Session.ID_Company.ToInt32())
                                ID_Company = Session.ID_Company,
                                ID_CreatedBy = Session.ID_User,
                                CreatedAt = DateTime.Now,
                                ID_FilingStatus = 1, // Filed status
                                ID_WorkCredit = 1,
                                WorkDate = DateTime.Now,
                                ID_Employee = Session.ID_Employee.IsNull(0).ToInt32(),
                                StartTime = DateTime.Now,
                                EndTime = DateTime.Now,
                                FilingDate = DateTime.Now
                            }),
                            Schema = Helpers.GetSchema("tOvertime")
                        },
                        Type = ReturnType.Result
                    };

                }

            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet Save()
        {
            var OvertimeData = Parameter["Data"].ToObject<tOvertime>();
            string ErrorMessage = "";
            //Validate duplicate record
            //Validate if O.T. is conflict with other O.T.
            try
            {
                using (var db = new OrbitOvertimeDb(Session))
                {
                    //Concat StartDate and StartTime && EndDate and EndTime

                    OvertimeData.StartTime = new DateTime(OvertimeData.StartDate.Year,
                                                    OvertimeData.StartDate.Month,
                                                    OvertimeData.StartDate.Day,
                                                    OvertimeData.StartTime.Hour,
                                                    OvertimeData.StartTime.Minute,
                                                    OvertimeData.StartTime.Second);

                    OvertimeData.EndTime = new DateTime(OvertimeData.EndDate.Year,
                                                    OvertimeData.EndDate.Month,
                                                    OvertimeData.EndDate.Day,
                                                    OvertimeData.EndTime.Hour,
                                                    OvertimeData.EndTime.Minute,
                                                    OvertimeData.EndTime.Second);

                    //if StartTime is greter that endtime then add 1 day
                    if (OvertimeData.StartTime > OvertimeData.EndTime) OvertimeData.EndTime.AddDays(1);

                    //Plot Startminute and Endminute
                    OvertimeData.StartMinute = ConvertDateTimeToMinute(OvertimeData.StartTime);
                    OvertimeData.EndMinute = ConvertDateTimeToMinute(OvertimeData.EndTime);

                    //Validate
                    ErrorMessage = ValidateBeforeSave(OvertimeData, db);

                    if (ErrorMessage != "") throw new Exception(ErrorMessage);


                    if (db.Any("(SELECT Id FROM tOvertime WHERE ID = {0})a", OvertimeData.ID))
                    {
                        OvertimeData.ID_ModifiedBy = Session.ID_User;
                        OvertimeData.ModifiedAt = DateTime.Now;
                        db.Update(OvertimeData);
                        db.SaveChanges();
                    }
                    else
                    {

                        ErrorMessage = CheckPosted(OvertimeData, db); //Do not allow posted or approved to be modified
                        if (ErrorMessage != "") throw new Exception(ErrorMessage);

                        OvertimeData.ID_CreatedBy = Session.ID_User;
                        OvertimeData.CreatedAt = DateTime.Now;
                        OvertimeData.ID_Company = Session.ID_Company;
                        OvertimeData.ReferenceNo = Helpers.getReferenceNumber("tOvertime", Session.ID_Company.ToInt32());
                        db.Add(OvertimeData);
                        db.SaveChanges();
                        Helpers.updateReferenceNum("tOvertime", Session.ID_Company.ToInt32(), Session);
                    }

                    return new ReturnSet() { Data = OvertimeData.ID, Type = ReturnType.Result };
                }
            }
            catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }

        }

        public override ReturnSet DeleteRecord() {
            try
            {

                //Validate Dependency
                //Do not delete posted
                //Do not delete approved
                //Do not delete if timesheet is processed.
                var msg = "";
                var ids = Parameter["data"].ToObject<List<int>>();
                using (var db = new OrbitOvertimeDb(Session))
                {
                    if (ids.Count > 0)
                    {
                        foreach (var id in ids)
                        {
                            var _msg = db.ExecScalarString("SELECT ReferenceNo AS Value FROM tOvertime WHERE Id = {0} AND IsPosted = 1", id);
                            var rec = db.tOvertime.SingleOrDefault(x => x.ID == id);
                            if (rec != null) db.Remove(rec);

                            if (_msg != null) {
                                msg += _msg + ", ";
                            }
                        }

                        if (msg != "") {
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

        public string ValidateBeforeSave(tOvertime data, OrbitOvertimeDb db) {
            string msg = "";
            try
            {
                //Check duplicate O.T. filing
                var DuplicatedReferenceNumber = db.ExecScalarString(@"SELECT ReferenceNo AS Value
                                        FROM tOvertime o
                                            INNER JOIN vEmployees e ON e.ID_Employee = o.ID_Employee
                                        WHERE o.ID <> {0}
                                            AND o.ID_Employee = {1}
                                            AND o.[WorkDate] = {2}
                                            AND ({3} BETWEEN o.StartTime AND o.EndTime
                                                OR {4} BETWEEN o.StartTime AND o.EndTime
                                                OR o.StartTime BETWEEN {3} AND {4}
                                                OR o.EndTime BETWEEN {3} AND {4})", data.ID, data.ID_Employee, data.WorkDate, data.StartTime, data.EndTime);

                if (DuplicatedReferenceNumber != null)
                    msg += "Your filing's Start Time/End Time is conflict with record [" + DuplicatedReferenceNumber.ToString() +"].";

                //Check Start Time and End Time Conflict

                if (data.StartTime > data.EndTime)
                    msg += "Your Start Time is greater than End Time";
                

            }
            catch (Exception ex) {
                msg = ex.Message;
            }
            return msg;
        }

        public string CheckPosted(tOvertime data, OrbitOvertimeDb db) {
            string msg = "";
            try
            {
                if (data.IsPosted != true && data.ID_FilingStatus != 2)
                {
                    var EmployeeReference = db.ExecScalarString(@"SELECT ReferenceNo AS Value
                                                            FROM tOvertime
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
                using (var db = new OrbitOvertimeDb(Session))
                {
                    db.ExecNonQuery("Update tOvertime SET IsPosted = 1 WHERE (Id = {0} AND IsPosted <> 1)", Id);
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
                using (var db = new OrbitOvertimeDb(Session))
                {
                    db.ExecNonQuery("Update tOvertime SET IsPosted = 0, ID_FilingStatus = 1 WHERE (Id = {0} AND IsPosted <> 0)", Id);
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
                var Data = Parameter["Data"].ToObject<tOvertime>();
                var ApprovalHistory = new tApprovalHistory();
                string Comment = Parameter["ApproverComment"].ToString();
                string Msg = "";
                using (var db = new OrbitOvertimeDb(Session))
                {
                    //Update Filing Status
                    if (Data.ID_FilingStatus != 2) {
                        db.ExecNonQuery(@"UPDATE tOvertime SET ID_FilingStatus = 2, ID_ApprovedBy = {0} WHERE (Id = {1} AND ID_FilingStatus <> 2)", Session.ID_User, Data.ID);
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
                var Data = Parameter["Data"].ToObject<tOvertime>();
                var ApprovalHistory = new tApprovalHistory();
                string Comment = Parameter["ApproverComment"].ToString();
                string Msg = "";
                using (var db = new OrbitOvertimeDb(Session))
                {
                    if (Data.ID_FilingStatus != 3) {
                        db.ExecNonQuery(@"UPDATE tOvertime SET ID_FilingStatus = 3, ID_ApprovedBy = NULL WHERE (Id = {0} AND ID_FilingStatus <> 3)", Data.ID);
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

        public int ConvertDateTimeToMinute(DateTime dt) {
            return DateTimeHelper.ConvertDateTimeToMinutes(dt);
        }

    }
}
