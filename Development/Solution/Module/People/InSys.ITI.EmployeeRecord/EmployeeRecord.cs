using InSys.Context;
using InSys.Helper;
using InSys.HRMS.Models;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;
using System.Text.RegularExpressions;
using InSys.ITI.Models.Dto;

namespace InSys.ITI.EmployeeRecord
{
    public class EmployeeRecord : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new EmployeeRecord(_Session, _Parameter);
        }
        public EmployeeRecord() { }
        public EmployeeRecord(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public override ReturnSet LoadList()
        {
            string message = "";
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new EmployeeRecordDb(Session))
                {
                    var fs = to.Filters.Where(x => x.Name == "ID_Company").FirstOrDefault();
                    if (fs == null)
                        to.Filters.Add(new FilterSchema()
                        {
                            Name = "ID_Company",
                            Type = FilterType.Equals,
                            Value = Session.ID_Company
                        });

                    var data = db.QueryTable<vEmployeeRecordList>("(SELECT * FROM dbo.fEmployeeRecordList({0}))a", to, Session.ID_Company);
                    r = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadList", Helpers.CurrentUser(Session), "InSys.ITI.People");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var personaAddress = new List<vPersonaAddress>();
                    var employee = db.ExecQuery<vEmployee>("SELECT * FROM dbo.fEmployeeView({0})", Parameter["ID"].ToInt32()).FirstOrDefault();
                    var data = new vPersona();
                    var sched = new vWeeklySchedule();

                    if (employee != null)
                    {
                        data = db.ExecQuery<vPersona>("SELECT * FROM dbo.fPersonaView({0})", employee.ID_Persona).FirstOrDefault();
                        sched = db.ExecQuery<vWeeklySchedule>("SELECT * FROM dbo.fWeeklyScheduleView({0})", employee.ID_WeeklySchedule).FirstOrDefault();
                    }
                    
                    
                    if (data != null)
                    {
                        personaAddress = db.ExecQuery<vPersonaAddress>("SELECT * FROM dbo.fPersonaAddressView({0})", data.ID).ToList();
                    }

                    if (data == null && Parameter["ID"].IsNull(0).ToInt32() > 0)
                    {
                        return new ReturnSet()
                        {
                            Message = "Page not found.",
                            Type = ReturnType.PageNotFound
                        };
                    }

                    vPersonaAddress p1 = new vPersonaAddress()
                    {
                        ID_Persona = data.ID,
                        DatetimeCreated = DateTime.Now,
                        IsPresentAddress = true,
                        IsActive = true
                    };
                    vPersonaAddress p2 = new vPersonaAddress()
                    {
                        ID_Persona = data.ID,
                        DatetimeCreated = DateTime.Now,
                        IsPresentAddress = false,
                        IsActive = true
                    };

                    if (personaAddress.Count == 0)
                    {
                        personaAddress.Add(p1);
                        personaAddress.Add(p2);
                    }

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = employee.IsNull(new tEmployee()),
                            Persona = data.IsNull(new tPersona()),
                            PersonaAddress = personaAddress,
                            Schema = Helpers.GetSchema("tPersona"),
                            PersonaAddressSchema = Helpers.GetSchema("tPersonaAddress"),
                            EmploymentSchema = Helpers.GetSchema("tPersonaEmployment"),
                            EducationalSchema = Helpers.GetSchema("tPersonaEducationalBackGround"),
                            LicencesSchema = Helpers.GetSchema("tPersonaLicensesAndCertificates"),
                            ReferencesSchema = Helpers.GetSchema("tPersonaCharacterReference")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadForm", Helpers.CurrentUser(Session), "InSys.ITI.People");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public override ReturnSet Save()
        {
            var r = new ReturnSet();
            string ErrorMessage = "";
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    tPersona tfm = Parameter["Data"].ToObject<tPersona>();
                    
                    List<tPersonaAddress> PersonaAddress = new List<tPersonaAddress>();
                    PersonaAddress = Parameter["PersonaAddress"].ToObject<List<tPersonaAddress>>();
                    
                    List<tPersonaEducationalBackGround> Educational = new List<tPersonaEducationalBackGround>();
                    Educational = Parameter["Educational"].ToObject<List<tPersonaEducationalBackGround>>();
                    
                    List<tPersonaEmployment> Employment = new List<tPersonaEmployment>();
                    Employment = Parameter["Employment"].ToObject<List<tPersonaEmployment>>();

                    List<tPersonaCharacterReference> CharacterReference = new List<tPersonaCharacterReference>(); //bba
                    CharacterReference = Parameter["CharacterReference"].ToObject<List<tPersonaCharacterReference>>();  //bba
                    
                    List<tPersonaLicensesAndCertificates> LicensesCertificates = new List<tPersonaLicensesAndCertificates>(); //bba
                    LicensesCertificates = Parameter["LicensesCertificates"].ToObject<List<tPersonaLicensesAndCertificates>>();  //bba

                    tEmployee er = Parameter["EmployeeRecord"].ToObject<tEmployee>();

                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //Validate Before Save
                            ErrorMessage += ValidateDuplicatePersona(db, tfm);
                            ErrorMessage += ValidateDuplicateTIN(db, tfm);
                            ErrorMessage += ValidateDuplicatePHIC(db, tfm);
                            ErrorMessage += ValidateDuplicateSSS(db, tfm);
                            ErrorMessage += ValidateDuplicateHDMF(db, tfm);

                            if (ErrorMessage != "")
                                throw new Exception(ErrorMessage);

                            if (db.Any("(select ID from dbo.tPersona where ID = {0})a", tfm.ID))
                                db.Update(tfm);
                            else {
                                tfm.DateTimeCreated = DateTime.Now;
                                db.Add(tfm);
                            }
                            db.SaveChanges(true);

                            foreach (var pa in PersonaAddress)
                            {
                                pa.ID_Persona = tfm.ID;
                                pa.ID_UserCreatedBy = Session.ID_User;
                                pa.IsActive = true;
                                pa.DatetimeCreated = DateTime.Now;
                                if (pa.ID_Province != null)
                                    if (db.Any("(select ID from dbo.tpersonaaddress where id = {0})a", pa.ID)) db.Update(pa);
                                    else db.Add(pa);
                            }
                            foreach (var ed in Educational)
                            {
                                ed.ID_Persona = tfm.ID;
                                ed.ID_UserCreatedBy = Session.ID_User;
                                if (db.Any("(select Id from dbo.tPersonaEducationalBackGround where ID = {0})a", ed.ID))
                                    db.Update(ed);
                                else
                                    db.Add(ed);
                            }
                            foreach (var em in Employment)
                            {
                                em.ID_Persona = tfm.ID;
                                em.ID_UserCreatedBy = Session.ID_User;
                                em.DatetimeCreated = DateTime.Now;
                                if (db.Any("(select Id from dbo.tPersonaEmployment where ID = {0})a", em.ID)) db.Update(em);
                                else db.Add(em);
                            }
                            //bba
                            foreach (var cr in CharacterReference)
                            {
                                cr.ID_Persona = tfm.ID;
                                cr.ID_UserCreatedBy = Session.ID_User;
                                cr.DatetimeCreated = DateTime.Now;
                                if (db.Any("(select Id from dbo.tPersonaCharacterReference where ID = {0})a", cr.ID)) db.Update(cr);
                                else db.Add(cr);
                            }
                            foreach (var lc in LicensesCertificates)
                            {
                                lc.ID_Persona = tfm.ID;
                                lc.ID_UserCreatedBy = Session.ID_User;
                                lc.DatetimeCreated = DateTime.Now;
                                if (db.Any("(select Id from dbo.tPersonaLicensesAndCertificates where ID = {0})a", lc.ID)) db.Update(lc);
                                else db.Add(lc);
                            }

                            if (PersonaAddress.Count > 0 || Educational.Count > 0 || Employment.Count > 0 || CharacterReference.Count > 0 || LicensesCertificates.Count > 0) 
                            {
                                if (db.Any("(select Id from dbo.tEmployee where ID_Persona = {0})a", tfm.ID))
                                {
                                    db.Update(er);
                                }
                                else 
                                {
                                    tEmployee temp = new tEmployee()
                                    {
                                        ID_Persona = tfm.ID,
                                        Code = "XXXXXX-" + Session.ID_Company.ToInt32() + tfm.ID,
                                        ID_EmployeeStatus = 2,
                                        ID_AccountNumberType = 2,
                                        IsActive = true,
                                        ID_Company = Session.ID_Company.ToInt32(),
                                        DateTimeCreated = DateTime.Now
                                    };
                                    db.Add(temp);
                                }
                                
                                db.SaveChanges(true);
                            }

                            tran.Commit();
                        }
                        catch(Exception ex)
                        {
                            tran.Rollback();
                            throw ex;
                        }
                    }
                    return new ReturnSet() { Data = er.ID.ToInt32(), Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual String DeleteRow()
        {
            try {
                return "";
            }
            catch (Exception ex) {
                return "";
            }
        }
        public override ReturnSet LoadLookup()
        {
            string lookupName = Parameter["LookupName"].ToString().ToLower();
            switch (lookupName)
            {
                case "educationalrecord":
                    return LoadEducationalRecord();
                case "employmenthistory":
                    return LoadEmploymentHistory();
                case "nationality":
                    return LoadNationality();
                case "citizenship":
                    return LoadCitizenship();
                case "civilstatus":
                    return LoadCivilStatus();
                case "gender":
                    return LoadGender();
                case "religion":
                    return LoadReligion();
                case "bloodtype":
                    return LoadBloodType();
                case "province":
                    return LoadProvince();
                case "city":
                    return LoadCity();
                case "barangay":
                    return LoadBarangay();
                case "educationalattainment":
                    return LoadEducationalAttainment();
                case "loadlicences":
                    return LoadLicences();
                case "loadreferences":
                    return LoadReferences();
                case "weeklyschedule":
                    return LoadWeeklySchedule();
                case "loadshiftschedule":
                    return LoadShiftSchedule();
                default:
                    throw new Exception("Method not found.");
            }
        }
        public virtual String regexNumOnly(string test) { //numbers only
            bool wow = Regex.IsMatch(test, @"^[0-9]*$");
            if (wow == true)
            {
                return "matched";
            }
            else { return "mali"; }
        }
        public override ReturnSet ExecuteAction()
        {
            try
            {
                string MethodName = Parameter["MethodName"]?.ToString();
                switch (MethodName.ToLower())
                {
                    case "loadshiftschedule":
                        return LoadShiftSchedule();
                    default:
                        throw new Exception("Method not found");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        #region lookup
        public virtual ReturnSet LoadEducationalAttainment()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, cast(1 as bit) IsActive from dbo.tEducationalAttainment").ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadBarangay()
        {
            try
            {
                using(var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, cast(1 as bit) IsActive from dbo.tBarangay where ID_City = {0}", Parameter["ID_City"].ToInt32()).ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }catch(Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadCity()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, cast(1 as bit) IsActive from dbo.tCity where ID_Province = {0}", Parameter["ID_Province"].ToInt32()).ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadProvince()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, cast(1 as bit) IsActive from dbo.tProvince").ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadBloodType()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, cast(1 as bit) IsActive from dbo.tBloodType").ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadReligion()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, cast(1 as bit) IsActive from dbo.tReligion").ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadGender()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, IsActive from dbo.tGender").ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadCivilStatus()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, cast(1 as bit) IsActive from dbo.tCivilStatus").ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadCitizenship()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, cast(1 as bit) IsActive from dbo.tCitizenship").ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadNationality()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>("select ID, Name, cast(1 as bit) IsActive from dbo.tNationality").ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadWeeklySchedule()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tLookUpData>(Helpers.GetSqlQuery("vWeeklySchedule")).ToList();
                    return new ReturnSet() { Data = new { Rows = data }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadEducationalRecord()
        {
            string message = "";
            var r = new ReturnSet();
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<vPersonaEducationalBackGround>(@"SELECT tpeb.*, tea.Name EducationAttainmentStatus FROM dbo.tPersonaEducationalBackGround tpeb
                                                                                LEFT JOIN dbo.tEducationalAttainment tea ON tpeb.ID_EducationAttainmentStatus = tea.ID where tpeb.ID_Persona = {0}", Parameter["ID"].ToInt32());
                    r = new ReturnSet() { Data = data.ToList(), Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadEducationalRecord", Helpers.CurrentUser(Session), "InSys.ITI.EmployeeRecord");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadEmploymentHistory()
        {
            string message = "";
            var r = new ReturnSet();
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tPersonaEmployment>("select * from dbo.tPersonaEmployment where ID_Persona = {0}", Parameter["ID"].ToInt32());
                    r = new ReturnSet() { Data = data.ToList(), Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadEmploymentHistory", Helpers.CurrentUser(Session), "InSys.ITI.EmployeeRecord");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadLicences()
        {
            string message = "";
            var r = new ReturnSet();
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tPersonaLicensesAndCertificates>("select * from dbo.tPersonaLicensesAndCertificates where ID_Persona = {0}", Parameter["ID"].ToInt32());
                    r = new ReturnSet() { Data = data.ToList(), Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "tPersonaLicensesAndCertificates", Helpers.CurrentUser(Session), "InSys.ITI.EmployeeRecord");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadReferences()
        {
            string message = "";
            var r = new ReturnSet();
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var data = db.ExecQuery<tPersonaCharacterReference>("select * from dbo.tPersonaCharacterReference where ID_Persona = {0}", Parameter["ID"].ToInt32());
                    r = new ReturnSet() { Data = data.ToList(), Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "tPersonaCharacterReference", Helpers.CurrentUser(Session), "InSys.ITI.EmployeeRecord");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadShiftSchedule()
        {
            try
            {
                using (var db = new EmployeeRecordDb(Session))
                {
                    var ID_WeeklySchedule = Parameter["ID_WeeklySchedule"].IsNull(0).ToInt32();
                    var data = db.Single<vWeeklySchedule>($"{Helpers.GetSqlQuery("vWeeklySchedule")} where week.ID = {{0}}", ID_WeeklySchedule);
                    return new ReturnSet() { Data = data, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public string ValidateDuplicatePersona(EmployeeRecordDb db, tPersona tfm) {
            var EmployeeNumber = db.ExecScalarString(@"
                        SELECT e.Code AS Value
                        FROM dbo.tPersona p
	                        LEFT OUTER JOIN tEmployee e ON e.ID_Persona = p.ID
                        WHERE p.ID <> {0}
	                        AND p.LastName = {1}
                            AND p.FirstName = {2}
                            AND ISNULL(p.MiddleName, '') = ISNULL({3}, '')
                            AND p.ID_Gender = {4}
                            AND p.BirthDate = {5}", tfm.ID, tfm.LastName, tfm.FirstName, tfm.MiddleName, tfm.ID_Gender, tfm.BirthDate);

            if (EmployeeNumber != null)
                return String.Format("Duplicate record with employee # [{0}].", EmployeeNumber);
            else
                return "";
        }

        //Validate Duplicate TIN
        public string ValidateDuplicateTIN(EmployeeRecordDb db, tPersona tfm)
        {
            var ErrorMessage = "";

            if (tfm.TINNo.IsNull("").ToString() != "")
            {
                var EmployeeNumber = db.ExecScalarString(@"SELECT e.Code AS Value
                            FROM tEmployee e
	                            INNER JOIN tPersona p ON p.ID = e.ID_Persona
                            WHERE p.ID <> {0} AND p.TINNo = {1}", tfm.ID, tfm.TINNo);

                if (EmployeeNumber != null)
                    ErrorMessage = String.Format("Duplicate TIN number with employee # [{0}].", EmployeeNumber);
            }

            return ErrorMessage;

        }

        //Valdiate Duplicate PHIC
        public string ValidateDuplicatePHIC(EmployeeRecordDb db, tPersona tfm)
        {
            var ErrorMessage = "";

            if (tfm.PhilHealthNo.IsNull("").ToString() != "")
            {
                var EmployeeNumber = db.ExecScalarString(@"SELECT e.Code AS Value
                        FROM tEmployee e
	                        RIGHT OUTER JOIN tPersona p ON p.ID = e.ID_Persona
                        WHERE p.ID <> {0} AND p.PhilHealthNo = {1}", tfm.ID, tfm.PhilHealthNo);

            if (EmployeeNumber != null)
                return String.Format("Duplicate Philhealth number with employee # [{0}].", EmployeeNumber);
            }
            return ErrorMessage;
        }

        //Validate Duplicate HDMF
        public string ValidateDuplicateHDMF(EmployeeRecordDb db, tPersona tfm)
        {
            var ErrorMessage = "";

            if (tfm.HDMFNo.IsNull("").ToString() != "")
            {
                var EmployeeNumber = db.ExecScalarString(@"SELECT e.Code AS Value
                        FROM tEmployee e
	                        INNER JOIN tPersona p ON p.ID = e.ID_Persona
                        WHERE p.ID <> {0} AND p.HDMFNo = {1}", tfm.ID, tfm.HDMFNo);

            if (EmployeeNumber != null)
                return String.Format("Duplicate HDMF number with employee # [{0}].", EmployeeNumber);
            }
            return ErrorMessage;
        }

        //Validate Duplicate SSS
        public string ValidateDuplicateSSS(EmployeeRecordDb db, tPersona tfm)
        {
            var ErrorMessage = "";

            if (tfm.SSSNo.IsNull("").ToString() != "")
            {
                var EmployeeNumber = db.ExecScalarString(@"SELECT e.Code AS Value
                        FROM tEmployee e
	                        INNER JOIN tPersona p ON p.ID = e.ID_Persona
                        WHERE p.ID <> {0} AND p.SSSNo = {1}", tfm.ID, tfm.SSSNo);

            if (EmployeeNumber != null)
                return String.Format("Duplicate SSS number with employee # [{0}].", EmployeeNumber);
            }
            return ErrorMessage;
        }



        #endregion
    }

   
}
