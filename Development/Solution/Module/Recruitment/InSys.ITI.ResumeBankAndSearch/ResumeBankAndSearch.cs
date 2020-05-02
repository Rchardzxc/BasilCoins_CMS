using System;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System.Linq;
using z.Data;
using System.Collections.Generic;
using System.Data.Common;
using DbConnection = InSys.Helper.DbConnection;

namespace InSys.ITI.ResumeBankAndSearch
{
    public class ResumeBankAndSearch: BaseModule
    {
        private int? ID_Employee { get; set; }
        private int? ID_Roles { get; set; }
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new ResumeBankAndSearch(_Session, _Parameter);
        }

        public ResumeBankAndSearch(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
            this.ID_Employee = _Session.ID_Employee;
            this.ID_Roles = _Session.ID_Roles;
        }

        public ResumeBankAndSearch() { }

        public override ReturnSet LoadList()
        {
            var r = new ReturnSet();
            var db2 = new ResumeBankAndSearchDB(Session);
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                int id_recruitmentstatus = Parameter["ID_RecruitmentStatus"].ToInt32();
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    var data = new Helper.TableData<vApplicationForm>();
                    var count = new List<RecruitmentStatusCount>();
                    if (ID_Roles != 7)
                    {
                        var forExam = db2.ExecQuery<tApplicationFormExamination>("SELECT * from dbo.tApplicationFormExamination where ID_AssignedEmployee = {0}", ID_Employee);
                        var forInterview = db2.ExecQuery<tApplicationFormInterviews>("SELECT * from dbo.tApplicationFormInterviews where ID_AssignedEmployee = {0}", ID_Employee);

                        count = db.ExecQuery<RecruitmentStatusCount>($"SELECT COUNT(af.ID) Cnt, 17 as ID FROM dbo.tApplicationForm af where af.ID_RecruitmentStatus = 17 and ID IN ({(forExam.ToList().Count > 0 ? string.Join(",", forExam.Select(x => x.ID_ApplicationForm).ToList()) : "NULL")})" +
                            $" UNION ALL SELECT COUNT(af.ID) Cnt, 18 as ID FROM dbo.tApplicationForm af where af.ID_RecruitmentStatus = 18 and ID IN ({(forInterview.ToList().Count > 0 ? string.Join(",", forInterview.Select(x => x.ID_ApplicationForm).ToList()) : "NULL")})").ToList();
                        if (id_recruitmentstatus == 17)
                        {

                            var ids = string.Join(",", forExam.Select(x => x.ID_ApplicationForm).ToList());
                            data = db.QueryTable<vApplicationForm>("(SELECT af.ID, af.TransDate, af.FirstName, af.LastName, SUBSTRING(af.MiddleName, 1, 1) MiddleInitial, af.ID_Designation"
                                            + " , af.MiddleName, af.EmailAddress, af.PhoneNo, mrf.MRFNumber, rs.Name AS Status, af.RefNum, af.ID_RecruitmentStatus, des.Name Designation"
                                            + " FROM tApplicationForm af"
                                            + " LEFT OUTER JOIN tMrf mrf ON mrf.ID = af.ID_Mrf"
                                            + " LEFT OUTER JOIN tRecruitmentStatus rs ON rs.ID = af.ID_RecruitmentStatus"
                                            + " LEFT OUTER JOIN tDesignation des on af.ID_Designation = des.ID"
                                            + $" Where af.ID_RecruitmentStatus = 17 and af.ID IN ({(forExam.ToList().Count > 0 ? ids : "NULL")})"
                                            + ")a", to);
                        }
                        else if (id_recruitmentstatus == 18)
                        {

                            var ids = string.Join(",", forInterview.Select(x => x.ID_ApplicationForm).ToList());
                            data = db.QueryTable<vApplicationForm>("(SELECT af.ID, af.TransDate, af.FirstName, af.LastName, SUBSTRING(af.MiddleName, 1, 1) MiddleInitial, af.ID_Designation"
                                                + " , af.MiddleName, af.EmailAddress, af.PhoneNo, mrf.MRFNumber, rs.Name AS Status, af.RefNum, af.ID_RecruitmentStatus, des.Name Designation"
                                                + " FROM tApplicationForm af"
                                                + " LEFT OUTER JOIN tMrf mrf ON mrf.ID = af.ID_Mrf"
                                                + " LEFT OUTER JOIN tRecruitmentStatus rs ON rs.ID = af.ID_RecruitmentStatus"
                                                + " LEFT OUTER JOIN tDesignation des on af.ID_Designation = des.ID"
                                                + $" Where af.ID_RecruitmentStatus = 18 and af.ID IN ({(forInterview.ToList().Count > 0 ? ids : "NULL")})"
                                                + ")a", to);
                        }
                    }
                    else
                    {
                        data = db.QueryTable<vApplicationForm>("(SELECT af.ID, af.TransDate, af.FirstName, af.LastName, SUBSTRING(af.MiddleName, 1, 1) MiddleInitial, af.ID_Designation"
                                            + " , af.MiddleName, af.EmailAddress, af.PhoneNo, mrf.MRFNumber, rs.Name AS Status, af.RefNum, af.ID_RecruitmentStatus, des.Name Designation"
                                            + " FROM tApplicationForm af"
                                            + " LEFT OUTER JOIN tMrf mrf ON mrf.ID = af.ID_Mrf"
                                            + " LEFT OUTER JOIN tRecruitmentStatus rs ON rs.ID = af.ID_RecruitmentStatus"
                                            + " LEFT OUTER JOIN tDesignation des on af.ID_Designation = des.ID"
                                            + " Where af.ID_RecruitmentStatus = {0}"
                                            + ")a", to, id_recruitmentstatus);
                        count = db.ExecQuery<RecruitmentStatusCount>("SELECT COUNT(af.ID) Cnt, tkrs.ID FROM dbo.tApplicationForm af" +
                                " RIGHT OUTER JOIN dbo.tRecruitmentStatus tkrs ON af.ID_RecruitmentStatus = tkrs.ID" +
                                " WHERE tkrs.Type = 1" +
                                " GROUP BY af.ID_RecruitmentStatus, tkrs.ID").ToList();
                    }

                    r = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList(), RecruitmentCount = count }, Type = ReturnType.Result };
                    db2.Dispose();
                    return r;
                }
            }
            catch (Exception ex)
            {
                db2.Dispose();
                ExceptionLogger(ex, Session);
                return r;
            }
        }

        public virtual List<vApplicationForm_Address> GetAppFormAddress(int ID, ResumeBankAndSearchDB db)
        {
            var afaddress = db.ExecQuery<vApplicationForm_Address>(Helpers.GetSqlQuery("vApplicationForm_Address"), ID).ToList();
            return afaddress;
        }

        public virtual List<vApplicationForm_CharacterReference> GetAppFormCharRef(int ID, ResumeBankAndSearchDB db)
        {
                var afacharRef = db.ExecQuery<vApplicationForm_CharacterReference>("SELECT ID, ID_ApplicationForm, Name, Position, Company"
                                            + " , ContactNo, IsActive, DatetimeCreated, ID_UserCreatedBy, IsEmploymentReference"
                                            + " FROM dbo.tApplicationForm_CharacterReference"
                                            + " WHERE ID_ApplicationForm = {0}", ID).ToList();
                return afacharRef;
        }

        public virtual List<vApplicationForm_Educational> GetAppFormEducational(int ID, ResumeBankAndSearchDB db)
        {
                var afaEducational = db.ExecQuery<vApplicationForm_Educational>("SELECT ID, ID_ApplicationForm, ID_EducationAttainmentStatus, SchoolAttended"
                                            + " , CourseDegree, IsActive, DatetimeCreated, ID_UserCreatedBy, [From], [To]"
                                            + " FROM tApplicationForm_Educational"
                                            + " WHERE ID_ApplicationForm = {0}", ID).ToList();

                return afaEducational;
        }

        public virtual List<vApplicationForm_Employment> GetAppFormEmployment(int ID, ResumeBankAndSearchDB db)
        {
                var afaEmployment = db.ExecQuery<vApplicationForm_Employment>("SELECT ID, ID_ApplicationForm, Company"
                                            + " , Position, Address, [From], [To], Salary, ReasonForLeaving, ID_UserCreatedBy, IsActive"
                                            + " , DatetimeCreated"
                                            + " FROM tApplicationForm_Employment"
                                            + " WHERE ID_ApplicationForm = {0}", ID).ToList();
                return afaEmployment;
        }
        public virtual List<vApplicationForm_LicensesAndCertificates> GetAppFormLicenses(int ID, ResumeBankAndSearchDB db)
        {
                var afaLicenses = db.ExecQuery<vApplicationForm_LicensesAndCertificates>("SELECT ID, ID_ApplicationForm, [Name]"
                                            + " , LicenseNo, [Description], ValidityDate, DateTimeCreated, ID_UserCreatedBy, IsActive"
                                            + " FROM tApplicationForm_LicensesAndCertificates"
                                            + " WHERE ID_ApplicationForm = {0}", ID).ToList();

                return afaLicenses;
        }

        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    var ParamId = Parameter["ID"].IsNull(0).ToInt32();
                    var data = db.Single<vApplicationForm2>(Helpers.GetSqlQuery("vApplicationForm"), ParamId);

                    var dAddress = GetAppFormAddress(ParamId, db);
                    var dCharRef = GetAppFormCharRef(ParamId, db);
                    var dEducational = GetAppFormEducational(ParamId, db);
                    var dEmployment = GetAppFormEmployment(ParamId, db);
                    var dLicenses = GetAppFormLicenses(ParamId, db);
                    int InterviewRecordCount = db.ExecScalarInt("SELECT COUNT(Id) as Value FROM tApplicationFormInterviews WHERE ID_ApplicationForm = {0}", ParamId).IsNull(0).ToInt32();
                    int ExaminationRecordCount = db.ExecScalarInt("SELECT COUNT(Id) as Value FROM tApplicationFormExamination WHERE ID_ApplicationForm = {0}", ParamId).IsNull(0).ToInt32();

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tApplicationForm()
                            {
                                ID_RecruitmentStatus = 1,
                                IsActive = true,
                                TransDate = DateTime.Now,
                                AvailableStartDate = DateTime.Now,
                                BirthDate = DateTime.Now,
                                DatetimeCreated = DateTime.Now,
                            }),
                            Schema = Helpers.GetSchema("tApplicationForm", db),
                            SchemaAddress = dAddress,
                            SchemaCharRef = dCharRef,
                            SchemaEducational = dEducational,
                            SchemaEmployment = dEmployment,
                            SchemaLicenses = dLicenses,
                            InterviewRecordCount = InterviewRecordCount,
                            ExaminationRecordCount = ExaminationRecordCount
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
            string postInterviewDataMessage = "";
            string postExaminationDataMessage = "";
            string postEmployeeJobOfferMessage = "";
            try
            {
                tApplicationForm tfm = Parameter["Data"].ToObject<tApplicationForm>();
                if (tfm.ID_RecruitmentStatus == 5)
                {
                    tfm.IsHired = true;
                }
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    using (var kioskTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            List<tApplicationFormExamination> ExaminationData = Parameter["ExaminationData"].ToObject<List<tApplicationFormExamination>>();
                            List<int> DeletedExaminationData = Parameter["DeletedExaminationData"].ToObject<List<int>>();

                            List<tApplicationFormInterviews> InterviewData = Parameter["InterviewData"].ToObject<List<tApplicationFormInterviews>>();
                            List<int> DeleteInterviewData = Parameter["DeleteInterviewData"].ToObject<List<int>>();

                            tEmployee_JobOffer EmployeeJobOffer = Parameter["JobOffer"] != null ? Parameter["JobOffer"].ToObject<tEmployee_JobOffer>() : null;

                            if (db.Single<tApplicationForm>("SELECT * FROM dbo.tApplicationForm where ID = {0}", tfm.ID) != null)
                                db.Update(tfm);
                            else db.Add(tfm);


                            postInterviewDataMessage = PostInterviewData(InterviewData, DeleteInterviewData, tfm, db);
                            if (postInterviewDataMessage != "") throw new Exception(postInterviewDataMessage);

                            postExaminationDataMessage = PostExaminationData(ExaminationData, DeletedExaminationData, tfm, db);
                            if (postExaminationDataMessage != "") throw new Exception(postExaminationDataMessage);

                            if (EmployeeJobOffer != null)
                            {
                                postEmployeeJobOfferMessage = postEmployeeJobOffer(EmployeeJobOffer, tfm.ID, db);
                                if (postEmployeeJobOfferMessage != "")
                                    throw new Exception(postEmployeeJobOfferMessage);
                            }

                            if (tfm.ID_RecruitmentStatus == 5)
                            {
                                SaveEmploymentRecord(db);
                            }


                            kioskTran.Commit();
                           
                        }
                        catch (Exception ex)
                        {
                            kioskTran.Rollback();
                            return ExceptionLogger(ex, Session);
                        }
                    }
                    db.SaveChanges(true);
                }
                return new ReturnSet() { Data = tfm.ID, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public void SaveEmploymentRecord(ResumeBankAndSearchDB db)
        {
            tEmployee tEmployee = null;
            List<ApproverEmployeeModuleData> ApproverEmployeeModuleData = null;
            tPersona tPersona = Parameter["Data"].ToObject<tPersona>();
            //tPersonaAddress personaAddress = null;

            tPersona.ID_ApplicationForm = tPersona.ID;
            tPersona.ID = 0;

            if (Parameter["Employee"] != null)
            {
                tEmployee = Parameter["Employee"].ToObject<tEmployee>();
                tPersona.ID_Company = tEmployee.ID_Company;
                db.Add(tPersona);
                db.SaveChanges(true);

                //personaAddress = db2.Single<tPersonaAddress>($"SELECT *,0 ID_Persona FROM tApplicationForm_Address WHERE ID_ApplicationForm = {tPersona.ID_ApplicationForm.ToInt32()}");
                //personaAddress.ID = 0;
                //personaAddress.ID_Persona = tPersona.ID;
                //db.Add(personaAddress);
                //db.SaveChanges(true);

                tEmployee.ID_Persona = tPersona.ID;
                db.Add(tEmployee);
                db.SaveChanges(true);

                if (Parameter["Approver"] != null)
                {
                    ApproverEmployeeModuleData = Parameter["Approver"].ToObject<List<ApproverEmployeeModuleData>>();
                    //save modules
                    foreach (ApproverEmployeeModuleData amd in ApproverEmployeeModuleData)
                    {
                        var tam = new tApprover_Employee_Module() { ID = amd.ID, ID_Employee = tEmployee.ID, ID_FilingModules = amd.ID_FilingModules, IsActive = true };
                        if (db.Any("(select * from dbo.tApprover_Employee_Module where ID = {0})a", tam.ID)) db.Update(tam);
                        else db.Add(tam);
                        db.SaveChanges(true);

                        if (amd.Default != null)
                        {
                            //save default Approver
                            foreach (var tad in amd.Default)
                            {
                                tad.ID_Approver_Employee_Module = tam.ID;
                                if (db.Any("(select * from dbo.tApprover_Employee_Module_Approvers where ID = {0})a", tad.ID)) db.Update(tad);
                                else db.Add(tad);
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
            else
            {
                throw new Exception("Employee data is required.");
            }
        }

        public override ReturnSet DeleteRecord()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tApplicationForm>("SELECT * from dbo.tApplicationForm where ID = {0}", id);
                        if (record != null) db.Remove(record);
                    }
                    db.SaveChanges();

                    return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        //THIS IS A REQUEST
        

        public virtual string PostInterviewData(List<tApplicationFormInterviews> InterviewData, List<int> DeleteInterviewData, tApplicationForm ApplicationForm, ResumeBankAndSearchDB db)
        {
            string message = "";
            int ID_ApplicationForm = ApplicationForm.ID;
            try
            {

                //delete Interviews
                foreach (int id in DeleteInterviewData)
                {
                    var InterviewToDelete = db.Single<tApplicationFormInterviews>("SELECT * FROM tApplicationFormInterviews WHERE ID = {0}", id);
                    if (InterviewToDelete != null)
                    {
                        db.Remove(InterviewToDelete);
                    }
                }
                db.SaveChanges();

                //Add and Update REcord
                foreach (var data in InterviewData)
                {
                    var InterviewToUpdate = db.Single<tApplicationFormInterviews>("SELECT * FROM tApplicationFormInterviews WHERE ID = {0}", data.ID);
                    data.ID_ApplicationForm = ID_ApplicationForm;
                    if (InterviewToUpdate != null) db.Update(data);
                    else db.Add(data);
                    db.SaveChanges();
                }

                if (ApplicationForm.ID_RecruitmentStatus == 5 || ApplicationForm.ID_RecruitmentStatus == 18)
                {
                    var checkData = db.Single<tApplicationFormInterviews>("SELECT TOP (1) * FROM tApplicationFormInterviews WHERE ID_ApplicationForm = {0}", ID_ApplicationForm);
                    if (checkData == null)
                    {
                        throw new Exception("\"Interview\" records are required, please fill-in with correct info.");
                    }
                }

                return message;
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session).Message;
            }
        }

        public virtual string PostExaminationData(List<tApplicationFormExamination> ExaminationData, List<int> DeletedExaminationData, tApplicationForm ApplicationForm, ResumeBankAndSearchDB db)
        {
            string message = "";
            int ID_ApplicationForm = ApplicationForm.ID;
            try
            {
                //delete Examinations
                foreach (int id in DeletedExaminationData)
                {
                    var ExaminationToDelete = db.Single<tApplicationFormExamination>("SELECT * FROM tApplicationFormExamination WHERE ID = {0}", id);
                    if (ExaminationToDelete != null)
                    {
                        db.Remove(ExaminationToDelete);
                    }
                }
                db.SaveChanges(true);

                //Add and Update REcord
                foreach (var data in ExaminationData)
                {
                    var ExaminationToUpdate = db.Single<tApplicationFormExamination>("SELECT * FROM tApplicationFormExamination WHERE ID = {0}", data.ID);
                    data.ID_ApplicationForm = ID_ApplicationForm;
                    if (ExaminationToUpdate != null) db.Update(data);
                    else db.Add(data);
                    db.SaveChanges();
                }

                if (ApplicationForm.ID_RecruitmentStatus == 17)
                {
                    var checkData = db.Single<tApplicationFormExamination>("SELECT TOP (1) * FROM tApplicationFormExamination WHERE ID_ApplicationForm = {0}", ID_ApplicationForm);
                    if (checkData == null) throw new Exception("\"Examination\" records are required, please fill-in with correct info.");
                }

                return message;
            }
            catch (Exception ex)
            {
                
                throw new Exception(ExceptionLogger(ex, Session).Message);
            }
        }

        //public override ReturnSet LoadOrganization()
        //{
        //    var res = new ReturnSet();
        //    try
        //    {
        //        TableOptions to = Parameter["data"].ToObject<TableOptions>();
        //        using (var db = new ResumeBankAndSearchDB(Session))
        //        {
        //            var data = db.Single<tMRF>("SELECT * FROM tMRF WHERE ID = {0}", Parameter["ID"].ToInt32());
        //            var dt = null;
        //            //var dt = db.Single<vDesignation>("SELECT des.*"
        //            //            + "     , cod.CompanyName AS Company"
        //            //            + "     , cod.BranchName AS Branch"
        //            //            + "     , cod.DivisionName AS Division"
        //            //            + "     , cod.DepartmentName AS Department"
        //            //            + "     , cod.SectionName AS Section"
        //            //            + "     , cod.JobClassGroupName AS JobClassGroup"
        //            //            + "     , cod.JobClassName AS JobClass"
        //            //            + "     , tea.Name AS EducationalAttainment"
        //            //            + " FROM tDesignation des"
        //            //            + "     LEFT OUTER JOIN vCompanyOrgDetails cod ON cod.ID_Designation = des.ID"
        //            //            + "     LEFT JOIN dbo.tEducationalAttainment tea ON des.ID_EducationalAttainment = tea.ID"
        //            //            + " WHERE des.ID = {0}", data.ID_Designation);
        //            res = new ReturnSet() { Data = dt, Type = ReturnType.Result };
        //        }
        //        return res;
        //    }
        //    catch (Exception ex)
        //    {
        //        return ExceptionLogger(ex, Session);
        //    }
        //}

        //public virtual ReturnSet LoadPDF()
        //{
        //    try
        //    {
        //        string ParentID = Parameter["ID"].ToString();
        //        if (Config.UseReportServer)
        //        {

        //            using (var report = new InSysReport(Helpers.GetSqlQuery("vzApplicationForm").BuildParameter(null, ParentID), "ApplicationForm", DbConnection.KIOSK))
        //            {
        //                return report
        //                    .AddSubReport("Address", Helpers.GetSqlQuery("vzApplicationForm_Address").BuildParameter(null, ParentID), DbConnection.KIOSK)
        //                    ////.AddReportParameter("ParentID", ParentID)
        //                    .AddSubReport("Education", Helpers.GetSqlQuery("vzApplicationForm_Education").BuildParameter(null, ParentID), DbConnection.KIOSK)
        //                    ////.AddReportParameter("ParentID", ParentID)
        //                    .AddSubReport("Licenses_And_Certificates", Helpers.GetSqlQuery("vzApplicationForm_License").BuildParameter(null, ParentID), DbConnection.KIOSK)
        //                    ////////.AddReportParameter("ParentID", ParentID)
        //                    .AddSubReport("Employment", Helpers.GetSqlQuery("vzApplicationForm_Employment").BuildParameter(null, ParentID), DbConnection.KIOSK)
        //                    //////AddReportParameter("ParentID", ParentID)
        //                    .AddSubReport("Family_Background", Helpers.GetSqlQuery("vzFamilyBackground").BuildParameter(null, ParentID), DbConnection.KIOSK)
        //                    ////.AddReportParameter("ParentID", ParentID)                    
        //                    .AddSubReport("Character", Helpers.GetSqlQuery("vzApplicationForm_Character").BuildParameter(null, ParentID), DbConnection.KIOSK)
        //                    ////.AddReportParameter("ParentID", ParentID)
        //                    .AddSubReport("Character_PreviousEmployment", Helpers.GetSqlQuery("vzCharacter_PreviousEmployment").BuildParameter(null, ParentID), DbConnection.KIOSK)
        //                    //.AddReportParameter("ParentID", ParentID)
        //                    .AddSubReport("SourcingPartner", Helpers.GetSqlQuery("vzApplicationForm_SourcingPartner"), DbConnection.KIOSK)
        //                    .CreateSubReport()
        //                    .CreateReport(Session);
        //            }
        //        }
        //        else
        //        {
        //            return new ReturnSet() { Message = "No JSReport found.", Type = ReturnType.Error };
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        return ExceptionLogger(ex, Session);
        //    }
        //}

        public virtual string postEmployeeJobOffer(tEmployee_JobOffer data, int id, ResumeBankAndSearchDB db)
        {
            try
            {
                var applicationInfo = db.Single<tApplicationForm>("SELECT * FROM dbo.tApplicationForm where ID = {0}", id);
                data.FirstName = applicationInfo.FirstName;
                data.LastName = applicationInfo.LastName;
                data.MiddleName = applicationInfo.MiddleName;
                data.ID_ApplicationForm = id;

                var EmployeeJobOfferData = db.ExecQuery<tEmployee_JobOffer>("SELECT * FROM tEmployee_JobOffer WHERE ID = {0}", data.ID).ToList();

                if (EmployeeJobOfferData.Count != 0)
                {
                    db.Update(data);
                }
                else db.Add(data);
                db.SaveChanges(true);
                return "";
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session).Message;
            }
        }

        public override ReturnSet LoadLookup()
        {
            string ParamName = Parameter["Name"].ToString();
            ReturnSet ResultData = new ReturnSet();
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session)) {
                    switch (ParamName) {
                        case "ApplicationStatus":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tRecruitmentStatus WHERE Type = 1");
                            break;
                        case "EducationalAttainment":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tEducationalAttainment");
                            break;
                        case "ExaminationAndInterviewStatus":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tExamAndInterviewStatus");
                            break;
                        case "Examinations":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tExaminations");
                            break;
                        case "MRF":
                            ResultData = base.LoadLookup<tEndorseLookUpData>(db, "SELECT ID, MRFNumber AS Name, CAST(1 AS BIT) AS IsActive , ID_Designation AS ID_Designation FROM tMRF");
                            break;
                        case "InterviewStatus":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tInterviewerStatus");
                            break;
                        case "Employee":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID_Employee AS ID, EmployeeName AS Name, CAST(1 AS BIT) AS IsActive FROM vEmployees");
                            break;

                        case "CostCenter":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tCostCenter");
                            break;

                        case "EmployeeStatus":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tEmployeeStatus");
                            break;

                        case "NatureOfRequests":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tNatureOfRequests");
                            break;

                        case "JobClass":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tJobClass");
                            break;

                        case "Department":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tDepartment");
                            break;

                        case "Division":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tDivision");
                            break;
                        case "Company":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tCompany");
                            break;
                        case "Branch":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tBranch");
                            break;
                        case "Section":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tSection");
                            break;
                        case "JobClassGroup":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tJobClassGroup");
                            break;
                        case "Designation":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tDesignation");
                            break;
                        case "FacultyType":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tFacultyType");
                            break;
                        case "FacultyInstitute":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tFacultyInstitute");
                            break;
                        case "Currency":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tCurrency");
                            break;
                        case "PayrollScheme":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tPayrollScheme");
                            break;
                        case "PayrollFrequency":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tPayrollFrequency");
                            break;
                        case "PaymentMode":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tPaymentMode");
                            break;
                        case "Parameter":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tParameter");
                            break;
                        case "LeaveParameter":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tLeaveParameter");
                            break;
                        case "PayrollStatus":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tPayrollStatus");
                            break;
                        case "PayrollClassification":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tPayrollClassification");
                            break;
                        case "AccountNumberType":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, CAST(1 AS BIT) IsActive FROM tAccountNumberType");
                            break;
                        case "CompanyBankAcct":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, BankName AS Name, IsActive FROM tCompanyBankAcct");
                            break;
                        case "TaxExemption":
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tTaxExemption");
                            break;
                        case "OrgBranch":
                            ResultData = base.LoadLookup<tLookUpData>(db, String.Format(@"SELECT o.ID, oml.Name, CAST(1 AS BIT) AS IsActive 
                                                                            FROM tOrg o 
                                                                            INNER JOIN tCompanyOrgType ot ON ot.ID = o.ID_CompanyOrgType 
                                                                            INNER JOIN tOrgMasterList oml ON oml.ID = o.ID_Master 
                                                                            WHERE ot.ID_OrgType = 1 AND o.ID_Company = {0}", Parameter["ID_Parent"]));
                            break;
                        case "OrgDivision":
                            ResultData = base.LoadLookup<tLookUpData>(db, String.Format(@"SELECT o.ID, oml.Name, CAST(1 AS BIT) AS IsActive 
                                                                            FROM tOrg o 
                                                                            INNER JOIN tCompanyOrgType ot ON ot.ID = o.ID_CompanyOrgType 
                                                                            INNER JOIN tOrgMasterList oml ON oml.ID = o.ID_Master 
                                                                            WHERE ot.ID_OrgType = 2 AND o.ID_Parent = {0}", Parameter["ID_Parent"]));
                            break;
                        case "OrgDepartment":
                            ResultData = base.LoadLookup<tLookUpData>(db, String.Format(@"SELECT o.ID, oml.Name, CAST(1 AS BIT) AS IsActive 
                                                                            FROM tOrg o 
                                                                            INNER JOIN tCompanyOrgType ot ON ot.ID = o.ID_CompanyOrgType 
                                                                            INNER JOIN tOrgMasterList oml ON oml.ID = o.ID_Master 
                                                                            WHERE ot.ID_OrgType = 3 AND o.ID_Parent = {0}", Parameter["ID_Parent"]));
                            break;
                        case "OrgSection":
                            ResultData = base.LoadLookup<tLookUpData>(db, String.Format(@"SELECT o.ID, oml.Name, CAST(1 AS BIT) AS IsActive 
                                                                            FROM tOrg o 
                                                                            INNER JOIN tCompanyOrgType ot ON ot.ID = o.ID_CompanyOrgType 
                                                                            INNER JOIN tOrgMasterList oml ON oml.ID = o.ID_Master 
                                                                            WHERE ot.ID_OrgType = 4 AND o.ID_Parent = {0}", Parameter["ID_Parent"]));
                            break;
                        case "OrgJobClassGroup":
                            ResultData = base.LoadLookup<tLookUpData>(db, String.Format(@"SELECT o.ID, oml.Name, CAST(1 AS BIT) AS IsActive 
                                                                            FROM tOrg o 
                                                                            INNER JOIN tCompanyOrgType ot ON ot.ID = o.ID_CompanyOrgType 
                                                                            INNER JOIN tOrgMasterList oml ON oml.ID = o.ID_Master 
                                                                            WHERE ot.ID_OrgType = 5 AND o.ID_Parent = {0}", Parameter["ID_Parent"]));
                            break;
                        case "OrgJobClass":
                            ResultData = base.LoadLookup<tLookUpData>(db, String.Format(@"SELECT o.ID, oml.Name, CAST(1 AS BIT) AS IsActive 
                                                                            FROM tOrg o 
                                                                            INNER JOIN tCompanyOrgType ot ON ot.ID = o.ID_CompanyOrgType 
                                                                            INNER JOIN tOrgMasterList oml ON oml.ID = o.ID_Master 
                                                                            WHERE ot.ID_OrgType = 6 AND o.ID_Parent = {0}", Parameter["ID_Parent"]));
                            break;
                        case "OrgDesignation":
                            ResultData = base.LoadLookup<tLookUpData>(db, String.Format(@"SELECT o.ID, oml.Name, CAST(1 AS BIT) AS IsActive 
                                                                            FROM tOrg o 
                                                                            INNER JOIN tCompanyOrgType ot ON ot.ID = o.ID_CompanyOrgType 
                                                                            INNER JOIN tOrgMasterList oml ON oml.ID = o.ID_Master 
                                                                            WHERE ot.ID_OrgType = 7 AND o.ID_Parent = {0}", Parameter["ID_Parent"]));
                            break;
                    }
                }
                return new ReturnSet() { Data = ResultData.Data, Type = ReturnType.Result };
            }
            catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet ExecuteAction()
        {
            try {
                var MethodName = Parameter["MethodName"]?.ToString().ToLower();
                switch (MethodName)
                {
                    case ("loadtablookup"):
                        return LoadTabLookUp();
                    case ("getinterviewrecord"):
                        return GetInterviewRecord();
                    case ("getexaminationrecord"):
                        return GetExaminationRecord();
                    case ("loadmrf"):
                        return LoadMRF();
                    case ("loadschema"):
                        return LoadSchema();
                    case ("loademployeefilingmodules"):
                        return LoadEmployeeFilingModules();
                    case ("loadapproverschema"):
                        return LoadApproverSchema();
                    case ("loadapprovermodule"):
                        return LoadApproverModule();
                    case ("loadselectedapprovertemplate"):
                        return LoadSelectedApproverTemplate();
                    case ("loadtemplateapprover"):
                        return LoadTemplateApprover();
                    case ("loadjoboffer"):
                        return LoadJobOffer();
                    default: throw new Exception("Method not found");
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
            
        }

        public virtual ReturnSet LoadTabLookUp()
        {
            try
            {
                var to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    //baguhin ung 7 pg nagiba ung usergroup ng HR or alisin itong condition at burahin depende sa sitwasyon
                    var data = db.QueryTable<tLookUpData>("(SELECT ID, Name, IsActive FROM tRecruitmentStatus WHERE Type = 1 and ((7 <> {0} AND ID IN (17, 18)) OR (7 = {0})))a", to, Session.ID_Roles);
                    return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return new ReturnSet() { Message = ex.Message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet GetInterviewRecord()
        {
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session))
                {

                    int id = Parameter["ID_ApplicationForm"].ToObject<int>();

                    var record = db.ExecQuery<vApplicationFormInterviews>(@"SELECT tafi.*, tp.Name AS AssignedEmployee, tu.ID ID_Receiver
                                FROM dbo.tApplicationFormInterviews tafi
                                LEFT OUTER JOIN dbo.tEmployee te on tafi.ID_AssignedEmployee = te.ID
                                LEFT OUTER JOIN dbo.tPersona tp on te.ID_Persona = tp.ID
                                LEFT OUTER JOIN dbo.tUsers tu on tafi.ID_AssignedEmployee = tu.ID_Employee
                                WHERE tafi.ID_ApplicationForm = {0}", id).ToList();

                    return new ReturnSet() { Data = record, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
              return new ReturnSet() { Message = ex.Message, Type = ReturnType.Result };
            }
        }

        public virtual ReturnSet GetExaminationRecord() 
        {
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    int id = Parameter["ID_ApplicationForm"].ToObject<int>();
                    var record = db.ExecQuery<vApplicationFormExamination>("SELECT tafe.*, tp.Name AssignedEmployee, tu.ID ID_Receiver FROM dbo.tApplicationFormExamination tafe" +
                        " left join dbo.tEmployee te on tafe.ID_AssignedEmployee = te.ID" +
                        " left join dbo.tPersona tp on te.ID_Persona = tp.ID" +
                        " left join dbo.tUsers tu on tafe.ID_AssignedEmployee = tu.ID_Employee" +
                        " WHERE tafe.ID_ApplicationForm = {0}", id).ToList();

                    return new ReturnSet() { Data = record, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return new ReturnSet() { Message = ex.Message, Type = ReturnType.Result };
            }
        }
        public virtual ReturnSet LoadMRF()
        {
            try
            {
                var to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    var data = db.QueryTable<MRFLookup>("(SELECT tm.ID_Designation ID, tm.MRFNumber, td.Name, tm.PostingDate, " +
                        " tm.ID ID_MRF FROM dbo.tMRF tm" +
                        " INNER JOIN dbo.tDesignation td ON tm.ID_Designation = td.ID" +
                        " WHERE tm.Posted = 1 and tm.ID_RecruitmentStatus = 7)a", to);
                    return new ReturnSet { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }

            }
            catch (Exception ex)
            {
                return new ReturnSet { Message = ex.Message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadJobOffer()
        {
            try
            {
                int ID_ApplicationForm = Parameter["ID"].ToObject<int>();
                vEmployee_JobOffer defaultData;
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    var data = db.Single<tEmployee_JobOffer>("SELECT * FROM tEmployee_JobOffer WHERE ID_ApplicationForm = {0}", ID_ApplicationForm);
                    if (data == null)
                    {
                        int mrfId = db.ExecScalarInt("SELECT ID_MRF AS Value FROM tApplicationForm WHERE ID = {0}", ID_ApplicationForm);
                        defaultData = db.Single<vEmployee_JobOffer>(Helpers.GetSqlQuery("getDefaultJobOffer"), mrfId);
                        data = new tEmployee_JobOffer()
                        {
                            ID_ApplicationForm = ID_ApplicationForm,

                            ID_Designation = defaultData.ID_Designation,
                            Designation = defaultData.Designation,

                            ID_EmployeeStatus = defaultData.ID_EmployeeStatus,
                            EmployeeStatus = defaultData.EmployeeStatus,

                            ID_NatureOfRequests = defaultData.ID_NatureOfRequests,
                            NatureOfRequests = defaultData.NatureOfRequests,

                        };
                    }
                    return new ReturnSet() { Data = data, Type = ReturnType.Result };
                }

            }
            catch (Exception ex)
            {
                return new ReturnSet() { Message = ex.Message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadSchema()
        {
            try
            {
                List<TableSchema> Schema = new List<TableSchema>();
                Schema = Helpers.GetSchema(Parameter["Name"].ToString());

                var tblName = "tEmployee";


                /////Provde ReferenceNumber on Employee Code
                if (Parameter["Name"].ToString() == tblName)
                {
                    int ID_Company = Session.ID_Company.ToInt32();
                    Schema.ForEach(x => x.DefaultValue = (x.ColumnName == "Code") ? Helpers.getReferenceNumber(tblName, ID_Company) : x.DefaultValue);
                }
                return new ReturnSet() { Data = Schema, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return new ReturnSet() { Message = ex.Message, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadEmployeeFilingModules()
        {
            int rID = Parameter["ID"].IsNull(0).ToInt32();
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    var data = db.ExecQuery<vApprover_Employee_Module>("select tam.*, tm.Name from dbo.tApprover_Employee_Module tam" +
                        " inner join dbo.tFilingModules tfm on tam.ID_FilingModules = tfm.ID" +
                        " inner join dbo.tMenus tm on tfm.ID_Menus = tm.Id" +
                        " where tam.ID_Employee = {0}", rID);
                    return new ReturnSet()
                    {
                        Data = data.ToList(),
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "LoadEmployeeFilingModules", Helpers.CurrentUser(Session), "InSys.ITI.ResumeBankAndSearch");
                return new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadApproverSchema()
        {
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = new vApprover_Employee_Module_Approvers() { },//ClassMapper.BuildData(new vApprover_Employee_Module_Approvers()),
                            Schema = Helpers.GetSchema("tApprover_Employee_Module_Approvers")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "LoadApproverSchema", Helpers.CurrentUser(Session), "InSys.ITI.ResumeBankAndSearch");
                return new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
        }

        public virtual ReturnSet LoadApproverModule()
        {
            var ret = new ReturnSet();
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    var data = db.ExecQuery<vApprover_Employee_Module_Approvers>("select tema.*, tp.Name Employee, tp2.Name Employee2, tp3.Name Employee3, tp.ImageFile Image, tp2.ImageFile Image2, tp3.ImageFile Image3 from dbo.tApprover_Employee_Module_Approvers tema" +
                        " left join dbo.tEmployee te on tema.ID_Employee = te.ID" +
                        " left join dbo.tEmployee te2 on tema.ID_Employee2 = te2.ID" +
                        " left join dbo.tEmployee te3 on tema.ID_Employee3 = te3.ID" +
                        " left join dbo.tPersona tp on te.ID_Persona = tp.ID" +
                        " left join dbo.tPersona tp2 on te2.ID_Persona = tp2.ID" +
                        " left join dbo.tPersona tp3 on te3.ID_Persona = tp3.ID" +
                        " where tema.ID_Approver_Employee_Module = {0}", Parameter["ID"].ToInt32());
                    return new ReturnSet() { Data = data.ToList(), Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "LoadApproverByModule", Helpers.CurrentUser(Session), "InSys.ITI.ResumeBankAndSearch");
                return new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
        }


        public virtual ReturnSet LoadSelectedApproverTemplate()
        {
            var ret = new ReturnSet();
            int rID = Parameter["ID"].IsNull(0).ToInt32();
            int ID_Employee = Parameter["ID_Employee"].ToInt32();
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    var data = db.ExecQuery<vApprover_Employee_Module>("select tam.*, null ID_Employee, tm.Name from dbo.tApprover_Module tam" +
                        " inner join dbo.tFilingModules tfm on tam.ID_FilingModules = tfm.ID" +
                        " inner join dbo.tMenus tm on tfm.ID_Menus = tm.ID" +
                        " where tam.ID_Approver = {0}", rID).Map(x =>
                        {
                            x.Default = LoadApprovers(x.ID);
                            x.ID = 0;
                            x.ID_Employee = ID_Employee;
                            x.IsActive = true;
                            return x;
                        }).ToList();
                    
                    return new ReturnSet()
                    {
                        Data = data,
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "LoadSelectedApproverTemplate", Helpers.CurrentUser(Session), "InSys.ITI.ResumeBankAndSearch");
                return new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
        }

        public List<vApprover_Employee_Module_Approvers> LoadApprovers(int ID)
        {
            var list = new List<vApprover_Employee_Module_Approvers>();
            using (var db = new ResumeBankAndSearchDB(Session))
            {
                list = db.ExecQuery<vApprover_Employee_Module_Approvers>("select tad.*, tp.Name Employee, tp2.Name Employee2, tp3.Name Employee3, null ID_Approver_Employee_Module, tp.ImageFile Image, tp2.ImageFile Image2, tp3.ImageFile Image3 from dbo.tApprover_Default tad" +
                        " LEFT JOIN dbo.tEmployee te on tad.ID_Employee = te.ID" +
                        " LEFT JOIN dbo.tEmployee te2 on tad.ID_Employee2 = te2.ID" +
                        " LEFT JOIN dbo.tEmployee te3 on tad.ID_Employee3 = te3.ID" +
                        " LEFT JOIN dbo.tPersona tp on te.ID_Persona = tp.ID" +
                        " LEFT JOIN dbo.tPersona tp2 on te2.ID_Persona = tp2.ID" +
                        " LEFT JOIN dbo.tPersona tp3 on te3.ID_Persona = tp3.ID" +
                        " WHERE tad.ID_Approver_Module = {0}", ID).Map(x => {
                            x.ID = 0;
                            return x;
                        }).ToList();
            }
            return list;
        }

        public virtual ReturnSet LoadTemplateApprover()
        {
            try
            {
                using (var db = new ResumeBankAndSearchDB(Session))
                {
                    var data = db.tApprover.Where(x => x.ID_Company == Session.ID_Company).ToList();
                    return new ReturnSet() { Data = data, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string msg = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref msg, "LoadTemplateApprover", Helpers.CurrentUser(Session), "InSys.ITI.ResumeBankAndSearch");
                return new ReturnSet() { Message = msg, Type = ReturnType.Error };
            }
        }
        public override ReturnSet LoadPDF()
        {
            try
            {
                string ParentID = Parameter["ID"].ToString();
                if (Config.UseReportServer)
                {

                    //using (var report = new InSysReport(Helpers.GetSqlQuery("vzApplicationForm").BuildParameter(null, ParentID), "ApplicationForm", DbConnection.KIOSK))
                    //{
                    //    return report
                    //        .AddSubReport("Employment", Helpers.GetSqlQuery("vzApplicationForm_Employment").BuildParameter(null, ParentID), DbConnection.KIOSK)
                    //        //.AddReportParameter("ParentID", ParentID)
                    //        .AddSubReport("License", Helpers.GetSqlQuery("vzApplicationForm_License").BuildParameter(null, ParentID), DbConnection.KIOSK)
                    //        //.AddReportParameter("ParentID", ParentID)
                    //        .AddSubReport("Address", Helpers.GetSqlQuery("vzApplicationForm_Address").BuildParameter(null, ParentID), DbConnection.KIOSK)
                    //        //.AddReportParameter("ParentID", ParentID)
                    //        .AddSubReport("Education", Helpers.GetSqlQuery("vzApplicationForm_Education").BuildParameter(null, ParentID), DbConnection.KIOSK)
                    //        //.AddReportParameter("ParentID", ParentID)
                    //        .AddSubReport("Character", Helpers.GetSqlQuery("vzApplicationForm_Character").BuildParameter(null, ParentID), DbConnection.KIOSK)
                    //        //.AddReportParameter("ParentID", ParentID)
                    //        .AddSubReport("SourcingPartner", Helpers.GetSqlQuery("vzApplicationForm_SourcingPartner"), DbConnection.KIOSK)
                    //        .CreateSubReport()
                    //        .CreateReport(Session);
                    //}
                    using (var report = new InSysReport(Helpers.GetSqlQuery("vzApplicationForm").BuildParameter(null, ParentID), "ApplicationForm", DbConnection.HRMS))
                    {
                        return report
                            .AddSubReport("Address", Helpers.GetSqlQuery("vzApplicationForm_Address").BuildParameter(null, ParentID), DbConnection.HRMS)
                            ////.AddReportParameter("ParentID", ParentID)
                            .AddSubReport("Education", Helpers.GetSqlQuery("vzApplicationForm_Education").BuildParameter(null, ParentID), DbConnection.HRMS)
                            ////.AddReportParameter("ParentID", ParentID)
                            .AddSubReport("Licenses_And_Certificates", Helpers.GetSqlQuery("vzApplicationForm_License").BuildParameter(null, ParentID), DbConnection.HRMS)
                            ////////.AddReportParameter("ParentID", ParentID)
                            .AddSubReport("Employment", Helpers.GetSqlQuery("vzApplicationForm_Employment").BuildParameter(null, ParentID), DbConnection.HRMS)
                            //////AddReportParameter("ParentID", ParentID)
                            .AddSubReport("Family_Background", Helpers.GetSqlQuery("vzFamilyBackground").BuildParameter(null, ParentID), DbConnection.HRMS)
                            ////.AddReportParameter("ParentID", ParentID)                    
                            .AddSubReport("Character", Helpers.GetSqlQuery("vzApplicationForm_Character").BuildParameter(null, ParentID), DbConnection.HRMS)
                            ////.AddReportParameter("ParentID", ParentID)
                            .AddSubReport("Character_PreviousEmployment", Helpers.GetSqlQuery("vzCharacter_PreviousEmployment").BuildParameter(null, ParentID), DbConnection.HRMS)
                            //.AddReportParameter("ParentID", ParentID)
                            .AddSubReport("SourcingPartner", Helpers.GetSqlQuery("vzApplicationForm_SourcingPartner"), DbConnection.HRMS)
                            .CreateSubReport()
                            .CreateReport(Session);
                    }
                }
                else
                {
                    return new ReturnSet() { Message = "No JSReport found.", Type = ReturnType.Error };
                    //using (var db = new InSysContext())
                    //{
                    //    var data = db.ExecQuery<ModelName>("your datasource").ToList();
                    //    using (var rpt = new ReportViewer("your jsreport template name without extension"))
                    //    {
                    //        return rpt.SetData(data)
                    //            .LoadReport();
                    //    }
                    //}
                }

            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
    }
}
