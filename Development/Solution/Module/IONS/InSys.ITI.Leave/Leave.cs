using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Leave
{
    public class Leave: BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Leave(_Session, _Parameter);
        }
        public Leave() { }


        public Leave(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
        }



        public override ReturnSet LoadList()
        {
            try
            {
                using (var db = new LeaveDb(Session))
                {
                    if (Session.ID_Roles == 1) throw new Exception("System Account is not entitled to access Leave module. Please sign in using Employee account.");
                    TableOptions to = Parameter["data"].ToObject<TableOptions>();
                    var data = db.QueryTable<vLeave>(@"(SELECT * FROM vLeave WHERE ID_Employee = {0})a", to, Session.ID_Employee);
                    return new ReturnSet
                    { 
                        Data = new { Total = data.Count, Rows = data.Data.ToList() }
                        , Type = ReturnType.Result 
                    };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }


        public tPersona GetPersona()
        {

            using (var db = new LeaveDb(Session))
            {
                var Employee = db.Single<tEmployee>("Select * from tEmployee where ID = {0}", Session.ID_Employee);
                return db.Single<tPersona>("Select * from tPersona where ID = {0}", Employee.ID_Persona);

            }
        }

        public override ReturnSet LoadForm()
        {
            try
            {
                using (var db = new LeaveDb(Session))
                {
                    var paramId = Parameter["Id"].IsNull(0).ToInt32();
                    var data = db.Single<vLeave>(@"SELECT * FROM vLeave WHERE ID = {0}", paramId); // where l.id = {0}
                    
                    //payrollitem
                    var PayrollItems = base.LoadLookup<tPayrollItem>(db, $"SELECT * FROM tPayrollItem WHERE IsActive = 1 and (ID_LeaveApplicableGender = {(GetPersona().ID_Gender == 1 ? 2 : 3) } OR ID_LeaveApplicableGender = 1) and isForLeave = 1");

                    //var FilingStatus = base.LoadLookup<tFilingStatus>(db, "SELECT *, 1 IsActive FROM tFilingStatus");

                    var LeaveCredit = base.LoadLookup<vLeaveCredit>(db, $"Select * From [dbo].[fGetEmployeeBalance]({Session.ID_Company}, {Session.ID_Employee}, {DateTime.Now.Year})"); 

                    var EmployeeRestDay = base.LoadLookup<vEmployeeRestDay>(db, $"Select ID,ID_WeekDay,ID_Employee,ID_Company,WeekDay from vEmployeeRestDay where ID_Employee = {Session.ID_Employee}");

                    var LeaveDayType = base.LoadLookup<vLeaveDayType>(db, $"SELECT * FROM tLeaveDayType");


                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new vLeave()
                            {
                                RefNum = "LV-XXXXXX",
                                ID_Employee = Session.ID_Employee.ToInt32(),
                                FiledDate = DateTime.Now,
                                CreatedAt = DateTime.Now,
                                ID_CreatedBy = Session.ID_Employee.ToInt32(),
                                ID_ModifiedBy = Session.ID_Employee.ToInt32(),
                                ModifiedAt = DateTime.Now,
                                ID_FilingStatus = 1,
                                ID_Company = Session.ID_Company.ToInt32(),
                                IsPosted = false,
                                StartDate = DateTime.Now,
                                EndDate = DateTime.Now

                            }),
                            PayrollItems,
                            //FilingStatus,
                            LeaveCredit,
                            EmployeeRestDay,
                            LeaveDayType,
                            Schema = Helpers.GetSchema("tLeave"),
                            SchemaDetail = Helpers.GetSchema("tLeave_Detail"),
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
            try
            {
                using (var db = new LeaveDb(Session))
                {
                    using(var tran = db.Database.BeginTransaction())
                    {

                        try
                        {
                            var LeaveData = Parameter["data"].ToObject<tLeave>();
                            var LeaveData_Detail = Parameter["detail"].ToObject<List<tLeave_Detail>>();

                            //Validation
                            new Validate(LeaveData_Detail,LeaveData,Session,GetPersona());



                            // var LeaveDetailData = Parameter["detailRecord"].ToObject<List<tLeave_Detail>>();
                            // var DeletedIds = Parameter["DeletedIds"].ToObject<List<int>>();


                            if (db.Any("(select * from dbo.tLeave where ID = {0})a", LeaveData.ID))
                            {
                                db.Update(LeaveData);
                                //   PostLeaveDetail(LeaveData.Id, db, LeaveDetailData);
                                //RemoveLeaveDetail(db, );
                            }
                            else
                            {
                                LeaveData.RefNum = Helpers.getReferenceNumber("tLeave", Session.ID_Company.ToInt32());
                                db.Add(LeaveData);
                                //   PostLeaveDetail(LeaveData.Id, db, LeaveDetailData);
                                Helpers.updateReferenceNum("tLeave", Session.ID_Company.ToInt32(), Session);
                            }

                            db.ExecNonQuery("Delete From tLeave_Detail Where ID_Leave = {0}", LeaveData.ID);
                            
                            db.SaveChanges(true);
                            
                            foreach (var d in LeaveData_Detail)
                            {
                                d.Id = 0;
                                d.ID_Leave = LeaveData.ID;
                                db.Add(d);

                            }

                            db.SaveChanges(true);
                            tran.Commit();
                            return new ReturnSet() { Message = "Successfully saved.",Data = new { ID = LeaveData.ID,RefNo = LeaveData.RefNum} , Type = ReturnType.Result };

                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            throw ex;
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
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
                var RecordId = Parameter["ID_Leave"].ToInt32();
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
                switch (Parameter["DetailName"].ToString().ToLower())
                {
                    case "leavedetail":
                        return LoadLeaveDetail();
                    default:
                        throw new Exception("Detail not found.");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual ReturnSet LoadLeaveDetail()
        {
           
            using (var db = new LeaveDb(Session))
            {
                var data = db.ExecQuery<tLeave_Detail>("select * from dbo.tLeave_Detail where ID_Leave = {0}",Parameter["ID_Leave"].ToInt32()).ToList();
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data }, Type = ReturnType.Result };
            }

        }
        
        public override ReturnSet DeleteRecord()
        {
            try
            {
               
                using (var db = new LeaveDb(Session))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var ids = Parameter["Data"].ToObject<List<int>>();
                            
                            //db.RemoveRange(db.tLeave.Where(x => ids.Contains(x.Id)).ToList()); //use this for multiple deleting without iteration

                            foreach (var id in ids)
                            {
                                var DeleteRecord = db.tLeave.Where(x => x.ID == id).FirstOrDefault();
                                if (DeleteRecord != null)
                                {
                                    db.Remove(DeleteRecord);
                                    db.RemoveRange(db.tLeave_Detail.Where(x => x.ID_Leave == DeleteRecord.ID).ToList());
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


        public override ReturnSet LoadLookup()
        {
            using (var db = new LeaveDb(Session))
            {

                var name = Parameter["Name"].ToString();
                switch (name.Trim().ToLower())
                {
                    case "leavedaytype":
                        return base.LoadLookup<vLeaveDayType>(db, $"SELECT * FROM tLeaveDayType");
                    default:
                        var msg = $"{name} is not available on your lookup option check your 'Switch Case' statement.";
                        Logger.LogError(ref msg, "LoadLookup", Session.Name, "InSys.ITI.Leave");
                        throw new Exception("System Error! Please contact your System Administrator");
                }
            }
        }
        
    }



    public class Validate : BaseModule
    {

        public tLeave leave { get; set; }
        public List<tLeave_Detail> leaveDetails { get; set; }
        public tPersona Persona { get; set; }
        public int ID_SL { get; set; } = 27;
        public int ID_BL { get; set; } = 55;

        public int SLConsecutiveDays { get; set; } = 3;

        public int SLMaximumFiling { get; set; } = 3;

        public int SLAttachmentRequiredDays { get; set; } = 2;

        public int VLMinimumFiling { get; set; } = 3;

        public static List<int> RestDays { get; set; }

        public Validate(List<tLeave_Detail> _leaveDetails, tLeave _leave, BrowserSession _Session, tPersona _persona)
        {
            this.Session = _Session;

            leave = _leave;
            leaveDetails = _leaveDetails;
            Persona = _persona;


            GetRestDays();
           

            switch (leave.ID_LeavePayrollItem)
            {
                case 26:   //ID VL
                    VacationLeave();
                    break;
                case 27:   //ID SL
                    SickLeave();
                    break;
                case 55:   //Birthday Leave
                    BirthDayLeave();
                    break;
            }

            General();
        }

        public IEnumerable<DateTime> GenerateDateRange(DateTime startDate, DateTime endDate, int AddStartDate = 0,int AddEndDate = 0)
        {

            var range = endDate.AddDays(AddEndDate).Date.Subtract(startDate.AddDays(AddStartDate).Date).Days + 1 < 0 ? 0 : endDate.AddDays(AddEndDate).Date.Subtract(startDate.AddDays(AddStartDate).Date).Days + 1;
            
            return Enumerable.Range(0, range).Select(d => startDate.AddDays(AddStartDate).Date.AddDays(d));
        }


        public void GetRestDays() {
            
            using (var db = new LeaveDb(Session))
            {

                RestDays = db.vEmployeeRestDay.Where(x => x.ID_Employee == Session.ID_Employee).Select(x => x.ID_WeekDay).ToList();
            }

        }

        public void General()
        {
            try
            {

                //check start date & end date
                if (DateTime.Compare(leave.StartDate, leave.EndDate) == 1)
                {
                    IsValidateError = true;
                    throw new Exception("End Date must be greater than or equal from Start Date.");
                }

                //Check for Duplicate filing && Leave detail
                if (leaveDetails.Count > 0)
                {
                    foreach (var d in leaveDetails)
                    {
                        using (var db = new LeaveDb(Session))
                        {

                            var ifAny = db.tLeave_Detail.Join(db.tLeave, ld => ld.ID_Leave, l => l.ID,
                                  (ld, l) => new
                                  {
                                      ld.Id,
                                      ld.ID_Leave,
                                      ld.ID_LeaveDayType,
                                      l.ID_LeavePayrollItem,
                                      l.ID_Employee,
                                      ld.WorkDate
                                  }
                                  ).Where(x => x.WorkDate == d.WorkDate && x.ID_Employee == Session.ID_Employee && x.ID_LeavePayrollItem == ID_SL && x.ID_Leave != leave.ID).Any();

                            if (ifAny)
                            {
                                IsValidateError = true;
                                throw new Exception("An existing record overlapped with the current filing.");
                            }
                            
                            //if (db.ExecQuery<vLeave_Detail>("Select * from vLeave_Detail where CAST(WorkDate as Date) = {0} and ID_Leave <> {1} and ID_Employee = {2} and ID_LeavePayrollItem = {3}", d.WorkDate.ToString("yyyy-MM-dd"), d.ID_Leave, Session.ID_Employee, ID_SL).Any())
                            //{
                            //    IsValidateError = true;
                            //    throw new Exception("Duplicate Record.");
                            //}
                        }
                    }
                }
                else if (leave.ID_LeavePayrollItem != 62)
                {
                    IsValidateError = true;
                    throw new Exception("Leave detail cannot be empty.");
                }
                
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void SickLeave() {

            try
            {
                int counter = 0;

                if (DateTime.Compare(leave.EndDate, leave.FiledDate) == 1)
                {
                    IsValidateError = true;
                    throw new Exception("SL should not be filed in advance.");
                }

                var dateRange = GenerateDateRange(leave.EndDate, leave.FiledDate, AddStartDate: 1);

                foreach (var r in dateRange)
                {
                    if (!RestDays.Any(x => x == (Convert.ToInt32(r.DayOfWeek) == 0 ? 7 : Convert.ToInt32(r.DayOfWeek))))
                    {
                        counter += 1;
                    }

                }

                if (counter > SLMaximumFiling)
                {
                    if(leave.Attachment == null)
                    {
                        IsValidateError = true;
                        throw new Exception($"SL should be filed maximum of {SLMaximumFiling} days after the end date.");
                    }
                   
                }

                

                //validate 3 consecutive 1 day filing && Attachment for 2 Consecutive 1 day filing
                using (var db = new LeaveDb(Session))
                {
                    if (leaveDetails.Count == 1)
                    {
                        var _detail = leaveDetails[0];


                        var _consecutiveDays = (SLConsecutiveDays - 1);
                        double _leaveWeight = _detail.ID_LeaveDayType == 1 ? 1 : 0.5;

                        var _conRange = GenerateDateRange(_detail.WorkDate.AddDays(-_consecutiveDays), _detail.WorkDate.AddDays(_consecutiveDays));
                        

                        foreach (DateTime d in _conRange)
                        {
                            //check if working day
                            if(!RestDays.Any(x => x == (d.DayOfWeek.ToInt32() == 0 ? 7 : d.DayOfWeek.ToInt32())) && _detail.WorkDate != d)
                            {

                                var vDetail = db.tLeave_Detail.Join(db.tLeave, ld => ld.ID_Leave, l => l.ID,
                                    (ld, l) => new
                                    {
                                        ld.Id,
                                        ld.ID_Leave,
                                        ld.ID_LeaveDayType,
                                        l.ID_LeavePayrollItem,
                                        l.ID_Employee,
                                        ld.WorkDate
                                    }
                                    ).Where(x => x.WorkDate == d && x.ID_Employee == Session.ID_Employee && x.ID_LeavePayrollItem == ID_SL).FirstOrDefault();
                                

                                //var vDetail = db.ExecQuery<vLeave_Detail>("Select * from vLeave_Detail where CAST(WorkDate as Date) = {0} and ID_Employee = {1} and ID_LeavePayrollItem = {2}", d.ToString("yyyy-MM-dd"), Session.ID_Employee, ID_SL).FirstOrDefault();//.ToList();
                                
                                if (!vDetail.IsNull())
                                {
                                    _leaveWeight += vDetail.ID_LeaveDayType == 1 ? 1 : 0.5;
                                }
                                
                            }
                        }

                        if (_leaveWeight >= SLAttachmentRequiredDays)
                        {

                            if(leave.Attachment == null)
                            {
                                IsValidateError = true;
                                throw new Exception($"Attachment is required for more than {SLAttachmentRequiredDays} days SL");
                            }
                            
                        }


                        if (_leaveWeight >= SLConsecutiveDays)
                        {
                            IsValidateError = true;
                            throw new Exception($"{SLConsecutiveDays} Consecutive 1 day filing for SL is not allowed.");
                        }

                        
                    }
                }
                
                //check attachment
                if (leaveDetails.Count >= SLAttachmentRequiredDays)
                {
                    if(leave.Attachment == null || leave.Attachment == "")
                    {
                        IsValidateError = true;
                        throw new Exception($"Attachment is required for more than {SLAttachmentRequiredDays} days SL");
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void VacationLeave() {

            try
            {
                
                int counter = 0;

                var dateRange = GenerateDateRange(leave.FiledDate, leave.StartDate, AddStartDate: 1);

                foreach (var r in dateRange)
                {
                    if (!RestDays.Any(x => x == (Convert.ToInt32(r.DayOfWeek) == 0 ? 7 : Convert.ToInt32(r.DayOfWeek))))
                    {
                        counter += 1;
                    }

                }

                if (counter < VLMinimumFiling)
                {
                    IsValidateError = true;
                    throw new Exception($"VL should be filed minimum of {VLMinimumFiling} days before start date.");
                }



            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public void BirthDayLeave()
        {

            try
            {
                if(leaveDetails.Count == 1)
                {

                    if(leaveDetails[0].WorkDate != Persona.BirthDate)
                    {
                        IsValidateError = true;
                        throw new Exception($"Birth bay leave should be filed only on your birth date");
                    }



                    using (var db = new LeaveDb(Session))
                    {

                        var ifAny = db.tLeave.Where(x => x.StartDate == Persona.BirthDate && x.ID_Employee == Session.ID_Employee && x.ID_LeavePayrollItem == ID_BL && x.ID != leave.ID).Any();


                        if (ifAny)
                        {
                            IsValidateError = true;
                            throw new Exception("Birth day leave has already used");
                        }

                    }
                    
                }
                else
                {
                    IsValidateError = true;
                    throw new Exception($"Birth day leave should be filed once and only on your bBirth date");
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
