using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using z.Data;

namespace InSys.ITI.MRF
{
    public class MRF : BaseModule
    {
        private int? _ID_Company;
        private int? ID_Company;
        private int? ID_User;
        private int? ID_Employee;
        private string tblName;
        //This will be used for conditioning all company access
        public int? System_ID_Company;

        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new MRF(_Session, _Parameter);
        }

        public MRF(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
            ID_User = Session.ID_User;
            ID_Employee = Session.ID_Employee;
            ID_Company = Session.ID_Company;
            tblName = "tMRF";
            System_ID_Company = ID_Company; // (ID_Company == 0 ? OrganizationDb.CompanyRights(Session.ID_Roles) : ID_Company;
        }

        public MRF() {
           
        }
        public override ReturnSet LoadList()
        {
            try
            {
                //APPLY BASIC FILTERING
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new MRFDb(Session))
                {
                    var fs = to.Filters.Where(x => x.Name == "ID_Company").FirstOrDefault();
                    if (fs == null)
                        to.Filters.Add(new FilterSchema()
                        {
                            Name = "ID_Company",
                            Type = FilterType.IN,
                            Value = System_ID_Company
                        });
                    else if (fs.Value == null)
                    {
                        fs.Type = FilterType.IN;
                        fs.Value = System_ID_Company;
                    }
                    to.SortDirection = "DESC";
                    to.SortName = "PostingDate";
                    var data = db.QueryTable<vMRF>("(SELECT mrf.ID, mrf.MRFNumber, osPos.Name AS Position," +
                        " mrf.RequestHeadcount, tp.Name AS RequestedBy, tp2.Name AS ApprovedBy, " +
                        " rs.Name AS RecruitmentStatus, mrf.ID_Company, mrf.PostingDate, mrf.ApprovedDate" +
                        " FROM tMrf mrf" +
                        " LEFT OUTER JOIN tDesignation osPos ON osPos.ID = mrf.ID_Designation" +
                        " LEFT OUTER JOIN dbo.tEmployee te ON mrf.ID_RequestedBy = te.ID" +
                        " LEFT OUTER JOIN dbo.tPersona tp ON te.ID_Persona = tp.ID" +
                        " LEFT OUTER JOIN dbo.tEmployee te2 ON mrf.ID_ApprovedBy = te2.ID" +
                        " LEFT OUTER JOIN dbo.tPersona tp2 ON te2.ID_Persona = tp2.ID" +
                        " LEFT OUTER JOIN tRecruitmentStatus rs ON rs.Id = mrf.ID_RecruitmentStatus)a", to);
                    var ForApproval = db.ExecScalarInt($"select count(1) as Value from {Helpers.GetSqlQuery("vApprovalEmployeeMRF")}", Session.ID_Employee);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList(), ForApproval = ForApproval }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }


        public override ReturnSet Save()
        {
            tMRF MrfData = Parameter["data"].ToObject<tMRF>();
            tKioskMRF KioskmrfData = Parameter["data"].ToObject<tKioskMRF>();

            int? currentUser = ID_User.ToInt32();
            DateTime? _dateToday = DateTime.Now;
            int? MrfDataId = MrfData.ID;
            MrfDataId = MrfDataId == null ? 0 : MrfDataId;
            int RecordId = 0;

            try
            {
                using (var db = new MRFDb(Session))
                {
                    ValidateMRF(MrfData, db);
                    //New
                    if (MrfDataId == 0)
                    {
                        MrfData.MRFNumber = Helpers.getReferenceNumber(tblName, MrfData.ID_Company);
                        MrfData.DateTimeCreated = DateTime.Now;
                        MrfData.ID_CreatedBy = currentUser;

                        //if Approved
                        MrfData.ID_ApprovedBy = (MrfData.ID_RecruitmentStatus == 7) ? currentUser : null;
                        MrfData.ApprovedDate = (MrfData.ID_RecruitmentStatus == 7) ? _dateToday : null;
                        MrfData.Posted = (MrfData.ID_RecruitmentStatus == 11) ? false : MrfData.Posted; // if cancel make sure to unpost MRF.

                        MrfData.PostingDate = MrfData.PostingDate.Date;
                        db.Add(MrfData);
                        db.SaveChanges(true);
                        Helpers.updateReferenceNum(tblName, MrfData.ID_Company, Session);

                        //Post MRF on Kiosk
                        KioskmrfData.ID = MrfData.ID;
                        KioskmrfData.MRFNumber = MrfData.MRFNumber;
                        KioskmrfData.DateTimeCreated = MrfData.DateTimeCreated;
                        KioskmrfData.ID_CreatedBy = MrfData.ID_CreatedBy;
                        KioskmrfData.ID_ApprovedBy = MrfData.ID_ApprovedBy;
                        KioskmrfData.ApprovedDate = MrfData.ApprovedDate;
                        KioskmrfData.PostingDate = MrfData.PostingDate.Date;
                        KioskmrfData.ApproverLevel = MrfData.ApproverLevel;

                        PostKioskdMrf(KioskmrfData);
                        RecordId = MrfData.ID; ;
                    }
                    //Edit
                    else if (MrfDataId > 0)
                    {
                        var MrfToUpdate = db.Single<tMRF>("SELECT * FROM tMRF WHERE ID = {0}", MrfDataId);

                        if (MrfToUpdate != null)
                        {
                            MrfData.DateTimeModified = DateTime.Now;
                            MrfData.PostingDate = MrfData.PostingDate.Date;
                            MrfData.ID_ModifiedBy = currentUser;

                            MrfData.ID_ApprovedBy = (MrfData.ID_RecruitmentStatus == 7) ? currentUser : null;
                            MrfData.ApprovedDate = (MrfData.ID_RecruitmentStatus == 7) ? _dateToday : null;
                            MrfData.Posted = (MrfData.ID_RecruitmentStatus == 11) ? false : MrfData.Posted; // if cancel make sure to unpost MRF.

                            db.Update(MrfData);
                            db.SaveChanges(true);

                            //Post MRF on Kiosk
                            KioskmrfData.MRFNumber = MrfData.MRFNumber;
                            KioskmrfData.DateTimeCreated = MrfData.DateTimeCreated;
                            KioskmrfData.ID_CreatedBy = MrfData.ID_CreatedBy;
                            KioskmrfData.ID_ApprovedBy = MrfData.ID_ApprovedBy;
                            KioskmrfData.ApprovedDate = MrfData.ApprovedDate;
                            KioskmrfData.PostingDate = MrfData.PostingDate.Date;
                            KioskmrfData.ApproverLevel = MrfData.ApproverLevel;
                            PostKioskdMrf(KioskmrfData);

                            RecordId = MrfData.ID;
                        }
                        else
                        {
                            throw new Exception("Data does not exists.");
                        }
                    }
                    return new ReturnSet() { Data = RecordId, Type = ReturnType.Result };
                }

            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual void ValidateMRF(tMRF mrfData, MRFDb db)
        {

            //Check Date
            if (mrfData.PostingDate > mrfData.EndDate)
                throw new Exception("Posting Date is greater than End Date");

            //Check conflict MRF
            db.Database.ExecuteSqlCommand("pValidateMRF @PostingDate, @ID_Designation, @ID"
                            , new SqlParameter("@PostingDate", mrfData.PostingDate)
                            , new SqlParameter("@ID_Designation", mrfData.ID_Designation)
                            , new SqlParameter("@ID", mrfData.ID));

            //Do not allow to delete MRF if it is already used.
        }

        public override ReturnSet DeleteRecord()
        {
            var res = new ReturnSet();
            string message = "";
            try
            {
                using (var db = new MRFDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tMRF>("SELECT * FROM tMRF WHERE ID = {0}", id);
                        //tMRF.Where(x => x.ID == id).FirstOrDefault();
                        if (record != null) db.Remove(record);
                    }
                    DeletePostedMrf(ids);
                    db.SaveChanges();
                }
                return new ReturnSet() { Message = "Record Deleted", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new MRFDb(Session))
                {
                    var data = db.Single<vMRF2>($"SELECT mrf.*, tjc.Name AS JobClass, td.Name AS Designation"
                                                + $" FROM dbo.tMrf mrf"
                                                + $" LEFT OUTER JOIN tJobClass tjc ON tjc.ID = mrf.ID_JobClass"
                                                + $" LEFT OUTER JOIN tDesignation td ON td.ID = mrf.ID_Designation"
                                                + $" WHERE mrf.ID = {{0}} AND mrf.ID_Company IN({System_ID_Company})", Parameter["ID"].IsNull(0).ToInt32());
                    if (data == null && Parameter["ID"].IsNull(0).ToInt32() > 0)
                    {
                        return new ReturnSet()
                        {
                            Message = "Page not found.",
                            Type = ReturnType.PageNotFound
                        };
                    }
                    var EmployeeName = new List<MRFEmployeeName>();
                    string PersonToBeReplaced = "";
                    string RequestedBy = "";
                    string ApprovedBy = "";

                    if (data != null)
                    {
                        EmployeeName = db.ExecQuery<MRFEmployeeName>("SELECT e.ID AS ID, p.Name AS Name FROM tEmployee e"
                                            + "    INNER JOIN tPersona p ON p.ID = e.ID_Persona"
                                            + " WHERE e.ID IN ({0}, {1}, {2})", data.ID_PersonToBeReplaced.IsNull(0), data.ID_RequestedBy.IsNull(0), data.ID_ApprovedBy.IsNull(0)).ToList();
                        if (EmployeeName != null)
                        {
                            var _PersonToBeReplaced = EmployeeName.Where(x => x.ID == data.ID_PersonToBeReplaced).FirstOrDefault();
                            var _RequestedBy = EmployeeName.Where(x => x.ID == data.ID_RequestedBy).FirstOrDefault();
                            var _ApprovedBy = EmployeeName.Where(x => x.ID == data.ID_ApprovedBy).FirstOrDefault();

                            PersonToBeReplaced = _PersonToBeReplaced == null ? "" : _PersonToBeReplaced.Name;
                            RequestedBy = _RequestedBy == null ? "" : _RequestedBy.Name;
                            ApprovedBy = _ApprovedBy == null ? "" : _ApprovedBy.Name;
                        }
                    }
                    else
                    {
                        EmployeeName = db.ExecQuery<MRFEmployeeName>("SELECT e.ID AS ID, p.Name AS Name FROM tEmployee e"
                                            + "    INNER JOIN tPersona p ON p.ID = e.ID_Persona"
                                            + " WHERE (e.ID = {0})", ID_Employee.ToInt32()).ToList();
                        if (EmployeeName.Count != 0)
                            RequestedBy = EmployeeName[0].Name;
                    }


                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tMRF()
                            {
                                ID_Company = ID_Company.ToInt32(),
                                MRFNumber = Helpers.getReferenceNumber(tblName, ID_Company.ToInt32()),
                                PostingDate = DateTime.Now,
                                RequestedDate = DateTime.Now,
                                ID_RecruitmentStatus = 6,
                                ID_EmployeeStatus = 1, //Regular
                                IsBudgeted = false,
                                ID_RequestedBy = ID_Employee.ToInt32(),
                                MaleCount = 0,
                                FemaleCount = 0
                            }),
                            EmployeeNames = new
                            {
                                PersonToBeReplaced = PersonToBeReplaced,
                                RequestedBy = RequestedBy,
                                ApprovedBy = ApprovedBy
                            },
                            //PersonToBeReplaced = (PersonToBeReplacedVal != null) ? PersonToBeReplacedVal.PersonToBeReplaced.ToString() : "",
                            Schema = Helpers.GetSchema("tMRF")
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

        public virtual void PostKioskdMrf(tKioskMRF KioskmrfData)
        {
            //try
            //{
            //    using (var db = new KioskContext())
            //    {
            //        var mrfKiosk = db.Single<tKioskMRF>("SELECT * FROM tKioskMrf WHERE ID = {0}", KioskmrfData.ID);

            //        //new
            //        if (mrfKiosk == null)
            //        {
            //            db.Add(KioskmrfData);
            //            db.SaveChanges(true);
            //        }
            //        else if (mrfKiosk != null)
            //        {
            //            db.Update(KioskmrfData);
            //            db.SaveChanges();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string message = (ex.InnerException ?? ex).Message;
            //    Logger.LogError(ref message, "PostMRF", "Internal", "InSys.ITI.Recruitment");
            //    throw new Exception(message);
            //}
        }


        public virtual void DeletePostedMrf(List<int> Ids)
        {
            //To be reviewed
            //try
            //{
            //    using (var db = new KioskContext())
            //    {
            //        foreach (var id in Ids)
            //        {
            //            var data = db.Single<tKioskMRF>("SELECT * FROM tKioskMrf WHERE ID = {0}", id);
            //            if (data != null) db.Remove(data);
            //        }
            //        db.SaveChanges(true);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    string message = (ex.InnerException ?? ex).Message;
            //    Logger.LogError(ref message, "DeletePostedMrf", "Internal", "InSys.ITI.Recruitment", new KioskContext());
            //    throw new Exception(message);
            //}
        }

        public override ReturnSet Post()
        {
            int mrfID = Parameter["ID"].ToObject<int>();
            try
            {
                using (var db = new MRFDb(Session))
                {
                    var record = db.Single<tMRF>("select * from dbo.tMRF where ID = {0}", mrfID);
                    if(record != null)
                    {
                        record.Posted = true;
                        record.ID_RecruitmentStatus = Helpers.IIf(record.ID_RecruitmentStatus == 11, 6, record.ID_RecruitmentStatus).ToInt32();
                        db.Update(record);
                        db.SaveChanges();
                    }
                    else
                    {
                        return new ReturnSet() { Message = "Record not found.", Type = ReturnType.Error };
                    }
                    //db.ExecNonQuery("UPDATE tMRF SET Posted = CAST(1 AS BIT), ID_RecruitmentStatus = (CASE WHEN ID_RecruitmentStatus = 11 THEN 6 ELSE ID_RecruitmentStatus END) WHERE ID = {0}", mrfID);
                    //To be reviewed
                    //using (var db2 = new KioskContext())
                    //{
                    //    db2.ExecNonQuery("UPDATE tKioskMRF SET Posted = CAST(1 AS BIT), ID_RecruitmentStatus = (CASE WHEN ID_RecruitmentStatus = 11 THEN 6 ELSE ID_RecruitmentStatus END) WHERE ID = {0}", mrfID);
                    //    db2.SaveChanges();
                    //}
                    
                }
                return new ReturnSet() { Data = mrfID, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet Cancel()
        {
            int mrfID = Parameter["ID"].ToObject<int>();
            try
            {
                using (var db = new MRFDb(Session))
                {
                    var record = db.Single<tMRF>("select * from dbo.tMRF where ID = {0}", mrfID);
                    if (record != null)
                    {
                        record.Posted = false;
                        record.ID_RecruitmentStatus = 11;
                        db.Update(record);
                        db.SaveChanges();
                    }
                    else
                    {
                        return new ReturnSet() { Message = "Record not found.", Type = ReturnType.Error };
                    }
                        //db.ExecNonQuery("UPDATE tMRF SET Posted = CAST(0 AS BIT), ID_RecruitmentStatus = 11 WHERE ID = {0}", mrfID);
                    //using (var db2 = new KioskContext())
                    //{
                    //    db2.ExecNonQuery("UPDATE tKioskMRF SET Posted = CAST(0 AS BIT), ID_RecruitmentStatus = 11 WHERE ID = {0}", mrfID);
                    //    db2.SaveChanges();
                    //}
                }
                return new ReturnSet() { Data = mrfID, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet LoadApproval()
        {
            var res = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new MRFDb(Session))
                {
                    var fs = to.Filters.Where(x => x.Name == "ID_Company").FirstOrDefault();
                    if (fs == null)
                        to.Filters.Add(new FilterSchema()
                        {
                            Name = "ID_Company",
                            Type = FilterType.IN,
                            Value = System_ID_Company //IIf(Ctx.Session.Get<int>("ID_Company") == 0, OrganizationDb.CompanyRights(Ctx.Session.Get<int>("ID_Roles")), Ctx.Session.Get<int>("ID_Company"))
                        });
                    else if (fs.Value == null)
                    {
                        fs.Type = FilterType.IN;
                        fs.Value = System_ID_Company; //IIf(Ctx.Session.Get<int>("ID_Company") == 0, OrganizationDb.CompanyRights(Ctx.Session.Get<int>("ID_Roles")), Ctx.Session.Get<int>("ID_Company"));
                    }

                    List<vMRFApproval> data = db.QueryTable<vMRFApproval>(Helpers.GetSqlQuery("vApprovalEmployeeMRF"), to, ID_Employee).Data.ToList();
                    res = new ReturnSet() { Data = new { Total = data.Count, Rows = data }, Type = ReturnType.Result };
                }
                return res;
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        //Reports
        public override ReturnSet LoadPDF()
        {
            //return null;
            try
            {
                using (var report = new InSysReport(Helpers.GetSqlQuery("vzMRFReport").BuildParameter(null, Parameter["ID"].ToString()), "MRF", DbConnection.HRMS))
                {
                    return report.CreateReport(Session);
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet LoadExcel()
        {
            try
            {
                using (var report = new InSysReport(Helpers.GetSqlQuery("vzMRFReport").BuildParameter(null, Parameter["ID"].ToString()), "MRF", DbConnection.HRMS, ReportType.XLS))
                {
                    return report.CreateReport(Session);
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }

        }
        public override ReturnSet SetStatus()
        {

            try
            {
                int CurrentUser = ID_User.ToInt32();
                tMRF data = Parameter["data"].ToObject<tMRF>();
                int RecruitmentStatusType = Parameter["RecruitmentStatusType"].ToObject<int>();
                var ApproverComment = Parameter["ApproverComment"].ToObject<string>();
                tWebNotification SendStatusToEmployee = Parameter["SendStatusToEmployee"].ToObject<tWebNotification>();
                tWebNotification SendNotificationToNextApprover = Parameter["ForApprovalNotifMsg"].ToObject<tWebNotification>();

                using (var db = new MRFDb(Session))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            int ID_MRFMenu = db.ExecScalarInt("SELECT TOP 1 ID AS Value FROM tMenus WHERE Code = 'RecruitmentManpowerRequisitionForm'");
                            string Approver = db.Single<string>(@"SELECT TOP 1 EmployeeName AS Value FROM dbo.vUserEmployee WHERE ID = {0}", CurrentUser);

                            int RecruitmentStatus_Filed = db.ExecScalarInt(@"SELECT ID AS Value FROM tRecruitmentSTatus WHERE [Name] = 'Filed'");
                            int RecruitmentStatus_OnHold = db.ExecScalarInt(@"SELECT ID AS Value FROM tRecruitmentStatus WHERE [Name] = 'On-hold'");
                            int RecruitmentStatus_Approved = db.ExecScalarInt(@"SELECT ID AS Value FROM tRecruitmentStatus WHERE [Name] = 'Approved'");
                            int RecruitmentStatus_DisApproved = db.ExecScalarInt(@"SELECT ID AS Value FROM tRecruitmentStatus WHERE [Name] = 'Disapproved'");


                            string RecruitmentStatusTypeValue = "";

                            switch (RecruitmentStatusType)
                            {
                                case (1):
                                    RecruitmentStatusTypeValue = "Approved";
                                    break;
                                case (2):
                                    RecruitmentStatusTypeValue = "Disapproved";
                                    break;
                                case (3):
                                    RecruitmentStatusTypeValue = "On hold";
                                    break;
                                default:
                                    RecruitmentStatusTypeValue = "";
                                    break;
                            }

                            //Send Status to Requestor
                            tApprovalHistory ApproverHistory = new tApprovalHistory()
                            {
                                ApprovalAction = RecruitmentStatusTypeValue,
                                ApprovalLevel = data.ApproverLevel.IsNull(1).ToInt32(),
                                ApproverComment = ApproverComment,
                                DateTimeCreated = DateTime.Now,
                                ID_Employee = ID_Employee.ToInt32(),
                                ID_Menus = ID_MRFMenu,
                                ID_Reference = data.ID,
                                ID_TargetEmployee = data.ID_RequestedBy.ToInt32()
                            };
                            db.Add(ApproverHistory);
                            int MaxApprover = db.ExecScalarInt("SELECT dbo.fgetMaxApproverLevel({0}, {1}) as Value", data.ID_RequestedBy, ID_MRFMenu);

                            var record = db.Single<tMRF>("select * from dbo.tMRF where ID = {0} and ID_RequestedBy = {1}", data.ID, data.ID_RequestedBy);
                            if (record != null)
                            {
                                if (RecruitmentStatusType == 1)
                                {
                                    record.ID_RecruitmentStatus = Helpers.IIf((data.ApproverLevel + 1) > MaxApprover, RecruitmentStatus_Approved, RecruitmentStatus_Filed).ToInt32();
                                    record.ApproverLevel += 1;
                                }
                                else if (RecruitmentStatusType == 2) record.ID_RecruitmentStatus = RecruitmentStatus_DisApproved;
                                else if (RecruitmentStatusType == 3) record.ID_RecruitmentStatus = RecruitmentStatus_OnHold;
                                db.Update(record);
                                db.SaveChanges();
                            }
                            else
                            {
                                throw new Exception("Record not found.");
                            }
                        
                            //    if (RecruitmentStatusType == 1) // Approve
                            //{
                            //    db.ExecNonQuery("UPDATE dbo.tMRF SET ID_RecruitmentStatus = {0}, ApproverLevel = ApproverLevel + 1 WHERE ID = {1} AND ID_RequestedBy = {2}"
                            //                , Helpers.IIf((data.ApproverLevel + 1) > MaxApprover, RecruitmentStatus_Approved, RecruitmentStatus_Filed), data.ID, data.ID_RequestedBy);
                            //}

                            //else if (RecruitmentStatusType == 2) // Disapprove
                            //{
                            //    db.ExecNonQuery("UPDATE dbo.tMRF SET ID_RecruitmentStatus = {0} WHERE ID = {1} AND ID_RequestedBy = {2}"
                            //                                               , RecruitmentStatus_DisApproved, data.ID, data.ID_RequestedBy);
                            //}

                            //else if (RecruitmentStatusType == 3) // On Hold
                            //    db.ExecNonQuery("UPDATE dbo.tMRF SET ID_RecruitmentStatus = {0} WHERE ID = {1} AND ID_RequestedBy = {2}"
                            //                                               , RecruitmentStatus_OnHold, data.ID, data.ID_RequestedBy);

                            //db.SaveChanges();

                            //to be reviewed
                            //using (var db2 = new KioskContext())
                            //{
                            //    //Set Status on Kiosk Context Database
                            //    if (RecruitmentStatusType == 1) // Approve
                            //    {
                            //        db2.ExecNonQuery("UPDATE dbo.tKioskMRF SET ID_RecruitmentStatus = {0}, ApproverLevel = ApproverLevel + 1 WHERE ID = {1} AND ID_RequestedBy = {2}"
                            //                    , Helpers.IIf((data.ApproverLevel + 1) > MaxApprover, RecruitmentStatus_Approved, RecruitmentStatus_Filed), data.ID, data.ID_RequestedBy);
                            //    }

                            //    else if (RecruitmentStatusType == 2) // Disapprove
                            //    {
                            //        db2.ExecNonQuery("UPDATE dbo.tKioskMRF SET ID_RecruitmentStatus = {0} WHERE ID = {1} AND ID_RequestedBy = {2}"
                            //                                                   , RecruitmentStatus_DisApproved, data.ID, data.ID_RequestedBy);
                            //    }

                            //    else if (RecruitmentStatusType == 3) // On Hold
                            //        db2.ExecNonQuery("UPDATE dbo.tKioskMRF SET ID_RecruitmentStatus = {0} WHERE ID = {1} AND ID_RequestedBy = {2}"
                            //            , RecruitmentStatus_OnHold, data.ID, data.ID_RequestedBy);
                            //    db2.SaveChanges();
                            //}




                            ////Send Notification To Next Approver 
                            if ((data.ApproverLevel + 1) <= MaxApprover)
                            {
                                SendNotification(SendNotificationToNextApprover, db, Session, data.ID_RequestedBy.ToInt32(), (data.ApproverLevel + 1).ToInt32());
                            }

                            ////Send Updates To Employee
                            SendStatusToEmployee.Content += Approver;
                            Helpers.SendNotification(SendStatusToEmployee, Session);
                            SendStatusToEmployee.IsSent = true;
                            tran.Commit();
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            return ExceptionLogger(ex, Session);
                        }
                    }
                    return new ReturnSet() { Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet LoadHistory()
        {
            try
            {
                string menuCode = "RecruitmentManpowerRequisitionForm";
                using (var db = new MRFDb(Session))
                {
                    int MRFId = db.ExecScalarInt("SELECT TOP 1 ID AS Value FROM tMenus WHERE Code = {0}", menuCode);
                    var data = db.ExecQuery<vApprovalHistory>($"SELECT * FROM {Helpers.GetSqlQuery("vApprovalHistory")} WHERE ID_Menus = {{0}} AND ID_Reference = {{1}} AND ID_TargetEmployee = {{2}}", MRFId, Parameter["ID"].ToInt32(), Parameter["Employee"].ToInt32()).ToList();
                    return new ReturnSet() { Data = data.ToList(), Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }


        public virtual void SendNotification(tWebNotification data, MRFDb db, BrowserSession Session, int ID_Employee = 0, int ID_Level = 0)
        {
            int ID_Menu = 0;
            int? ID_WebNotificationTypes = 0;
            List<tApproverMatrixPerLevel> ApproverMatrixPerLevel;
            string MenuCode = "RecruitmentManpowerRequisitionForm";

            try
            {
                ID_Menu = db.ExecScalarInt("SELECT ID AS Value FROM tMenus WHERE Code = {0}", MenuCode);
                ID_WebNotificationTypes = db.ExecScalarInt("SELECT ID AS Value FROM tWebNotificationTypes WHERE Name = {0}", MenuCode);

                //Get Receivers
                ApproverMatrixPerLevel = db.ExecQuery<tApproverMatrixPerLevel>("SELECT * FROM dbo.fApproverMatrixPerLevel({0}, {1}, {2})", ID_Menu, ID_Employee, ID_Level).ToList();
                data.ID_WebNotificationTypes = ID_WebNotificationTypes;
                data.IsSent = true;
                foreach (tApproverMatrixPerLevel ApproverData in ApproverMatrixPerLevel)
                {

                    if (ApproverData.ID_Approver1 > 0)
                    {
                        data.ID_Receiver = ApproverData.ID_Approver1;
                        Helpers.SendNotification(data, Session);
                    }

                    if (ApproverData.ID_Approver2 > 0)
                    {
                        data.ID_Receiver = ApproverData.ID_Approver2;
                        Helpers.SendNotification(data, Session);
                    }

                    if (ApproverData.ID_Approver3 > 0)
                    {
                        data.ID_Receiver = ApproverData.ID_Approver3;
                        Helpers.SendNotification(data, Session);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionLogger(ex, Session).Message);
            }
        }

        public override ReturnSet LoadLookup()
        {
            string ParamName = Parameter["Name"].ToString();
            var ParamData = new ReturnSet();
            try {
                using (var db = new MRFDb(Session)) {
                    switch (ParamName) {
                        case "RecruitmentStatus":
                            ParamData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tRecruitmentStatus WHERE Type = 2");
                            break;
                        case "Designation":
                            ParamData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tOrgMasterList WHERE ID_OrgType = 7");
                            break;
                        case "EmployeeStatus":
                            ParamData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tEmployeeStatus");
                            break;
                        case "NatureOfRequests2":
                            ParamData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tNatureOfRequests");
                            break;
                        case "Company":
                            ParamData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tCompany");
                            break;
                        case "ReplacementReasons":
                            ParamData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, CAST(1 AS BIT) AS IsActive FROM dbo.fReplacementReasons()");
                            break;
                        case "Employee":
                            ParamData = base.LoadLookup<tLookUpData>(db, "SELECT e.ID, p.Name, e.IsActive, e.ID_Company FROM tEmployee e" +
                                " INNER JOIN tPersona p ON p.ID = e.ID_Persona");
                            break;
                        case "JobClass":
                            ParamData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive, ID_Company FROM tJobClass");
                            break;
                        case "MRFDesignation":
                            ParamData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive, 1 AS ID_Company FROM tOrgMasterList WHERE ID_OrgType = 7");
                            break;
                    }
                    return new ReturnSet() { Data = ParamData.Data, Type = ReturnType.Result };
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }      
        }

        public override ReturnSet ExecuteAction()
        {
            try
            {
                string MethodName = Parameter["MethodName"]?.ToString();
                switch (MethodName)
                {
                    case ("JobPosting"):
                        return JobPosting();
                    case ("GetManPowerPlanDetail"):
                        return GetManPowerPlanDetail();
                    default: throw new Exception("Method not found");
                }
            }
            catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual ReturnSet JobPosting()
        {
            int mrfID = Parameter["ID"].ToObject<int>();
            int JobPostingStatus = Parameter["Status"].ToObject<int>();
            try
            {
                using (var db = new MRFDb(Session))
                {
                    var record = db.Single<tMRF>("select * from dbo.tMRF where ID = {0} and Posted = 1", mrfID);
                    if (record != null)
                    {
                        record.PostForJobPosting = JobPostingStatus.ToBool();
                        db.Update(record);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Record not found.");
                    }
                }
                return new ReturnSet() { Data = mrfID, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "JobPosting", "Internal", "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet GetManPowerPlanDetail()
        {
            var _param = Parameter["Data"].ToObject<dynamic>();
            var data = new vManPowerPlanDetailsIntoMRF();
            int _ID_Designation = _param.ID_Designation;
            DateTime _PostingDate = _param.PostingDate;
            try
            {
                using (var db = new MRFDb(Session))
                {
                    data = db.Single<vManPowerPlanDetailsIntoMRF>("SELECT mppd.ID, mppd.ID_ManPowerPlans, mppd.ID_Designation, d.Name AS Position,"
                                + " mppd.Plantilla, mppd.HeadCount, ISNULL(mppd.Plantilla - mppd.HeadCount, 0) AS Vacancy"
                                + " FROM tManPowerPlans mpp"
                                + " INNER JOIN tManPowerPlanDetails mppd ON mppd.ID_ManPowerPlans = mpp.ID"
                                + " INNER JOIN tDesignation d ON d.ID = mppd.ID_Designation"
                                + " WHERE mppd.ID_Designation = {0}"
                                + "     AND {1} BETWEEN mpp.FromDate AND mpp.ToDate"
                                , _ID_Designation, _PostingDate);
                }
                return new ReturnSet() { Data = data, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "GetManPowerPlanDetail", "Internal", "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }




    }
}
