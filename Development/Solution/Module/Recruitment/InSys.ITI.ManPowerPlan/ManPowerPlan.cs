using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using z.Data;

namespace InSys.ITI.ManPowerPlan
{
    public class ManPowerPlan : BaseModule
    {
        private int? _ID_Company { get; set; }
        private int? _ID_Role { get; set; }
        private int? _ID_User { get; set; }
        private string tblName { get; set; }
        private int _System_ID_Company { get; set; }


        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new ManPowerPlan(_Session, _Parameter);
        }

        public ManPowerPlan(BrowserSession _Session, Pair _Parameter)
        {
            this.Parameter = _Parameter;
            this.Session = _Session;
            //Need to review the System_ID_Company, especially the OrganizationDB
            this._System_ID_Company = _ID_Company.ToInt32(); // _ID_Company == 0 ? OrganizationDb.CompanyRights(_ID_Role) : _ID_Company;
            this._ID_Company = Session.ID_Company;
            this._ID_Role = Session.ID_Roles;
            this._ID_User = Session.ID_User;
            this.tblName = "tManPowerPlans";
        }

        public ManPowerPlan() {


        }

        public override ReturnSet LoadList()
        {
            try
            {
                //APPLY BASIC FILTERING
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                List<vManPowerPlans> ManPowerPlanData = new List<vManPowerPlans>();
                using (var db = new ManPowerPlanDb(Session))
                {
                    var fs = to.Filters.Where(x => x.Name == "ID_Company").FirstOrDefault();
                    if (fs == null)
                        to.Filters.Add(new FilterSchema()
                        {
                            Name = "ID_Company",
                            Type = FilterType.IN,
                            Value = _System_ID_Company
                        });
                    else if (fs.Value == null)
                    {
                        fs.Type = FilterType.IN;
                        fs.Value = _System_ID_Company;
                    }

                    ManPowerPlanData = db.QueryTable<vManPowerPlans>("(SELECT * FROM tManPowerPlans)a", to).Data.ToList();

                    return new ReturnSet() { Data = new { Total = ManPowerPlanData.Count, Rows = ManPowerPlanData }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadManPowerPlan", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet Save()
        {
            var mppData = Parameter["data"].ToObject<tManPowerPlans>();
            List<tManPowerPlanDetails> mppDataDetails = Parameter["DataDetails"].ToObject<List<tManPowerPlanDetails>>()
                                                        .OrderBy(x => x.ID_Designation)
                                                        .ToList();
            List<int> mppDetailToDelete = Parameter["DeletedDetails"].ToObject<List<int>>();

            int? mppDataId = mppData.ID;
            mppDataId = mppDataId ?? 0;
            int RecordId = 0;
            string ErrorMessage = "";
            try
            {
                using (var db = new ManPowerPlanDb(Session))
                {
                    using (var dbtrans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            //New
                            if (mppDataId == 0)
                            {
                                //Create validation for saving
                                mppData.RefNum = Helpers.getReferenceNumber(tblName, mppData.ID_Company.ToInt32());
                                mppData.DateTimeCreated = DateTime.Now;
                                mppData.FromDate = mppData.FromDate.Date;
                                mppData.ToDate = mppData.ToDate.Date;
                                mppData.ID_CreatedBy = _ID_User;
                                db.Add(mppData);
                                db.SaveChanges(true);
                                Helpers.updateReferenceNum(tblName, mppData.ID_Company.ToInt32(), Session);
                                RecordId = mppData.ID;

                                foreach (var mppDataDetail in mppDataDetails)
                                {
                                    mppDataDetail.ID_ManPowerPlans = RecordId;
                                    ErrorMessage += CheckDuplicateData(mppData.FromDate, mppData.ToDate, mppDataDetail.ID_Designation, mppDataDetail.ID, db);
                                    db.Add(mppDataDetail);
                                }
                                if (ErrorMessage != "")
                                {
                                    throw new Exception(ErrorMessage);
                                }

                                db.SaveChanges(true);
                                db.Database.ExecuteSqlCommand("pPostManPowerDetails @mppId", new SqlParameter("@mppId", mppData.ID));
                                dbtrans.Commit();
                            }
                            //Edit
                            else if (mppDataId > 0)
                            {
                                var mppToUpdate = db.Single<tManPowerPlans>("SELECT * FROM tManPowerPlans WHERE ID = {0}", mppDataId);
                                //tManPowerPlans.Where(x => x.ID == mppDataId).FirstOrDefault();
                                if (mppToUpdate != null)
                                {
                                    mppData.DateTimeModified = DateTime.Now;
                                    mppData.ID_ModifiedBy = _ID_User;
                                    db.Update(mppData);
                                    db.SaveChanges(true);
                                    RecordId = mppData.ID;

                                    //Delete
                                    foreach (int id in mppDetailToDelete)
                                    {
                                        var record = db.Single<tManPowerPlanDetails>("SELECT * from tManPowerPlanDetails WHERE ID = {0}", id);
                                        if (record != null) db.Remove(record);
                                    }
                                    db.SaveChanges();

                                    foreach (var detailData in mppDataDetails)
                                    {
                                        var detailToUpdate = db.Single<tManPowerPlanDetails>("SELECT * from tManPowerPlanDetails WHERE ID = {0}", detailData.ID);
                                        var ErrorMsg = CheckDuplicateData(mppData.FromDate, mppData.ToDate, detailData.ID_Designation, detailData.ID, db);

                                        if (ErrorMsg == "")
                                        {
                                            if (detailToUpdate != null)
                                                db.Update(detailData);
                                            else
                                            {
                                                ErrorMessage += CheckDuplicateData(mppData.FromDate, mppData.ToDate, detailData.ID_Designation, detailData.ID, db);
                                                db.Add(detailData);
                                            }
                                        }
                                        else { ErrorMessage += ErrorMsg; }
                                    }

                                    if (ErrorMessage != "")
                                    {
                                        throw new Exception(ErrorMessage);
                                    }

                                    db.SaveChanges(true);

                                    db.Database.ExecuteSqlCommand("pPostManPowerDetails @mppId", new SqlParameter("@mppId", mppData.ID));
                                    dbtrans.Commit();
                                }
                                else
                                {
                                    throw new Exception("Data does not exists.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            dbtrans.Rollback();
                            string message = (ex.InnerException ?? ex).Message;
                            Logger.LogError(ref message, "SaveManPowerPlan", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                            return new ReturnSet() { Message = message, Type = ReturnType.Error };
                        }
                    }
                }
                return new ReturnSet() { Data = RecordId, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "SaveManPowerPlan", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }
        //There's no need to override since this will no be called outside.
        public virtual string CheckDuplicateData(DateTime FromDate, DateTime ToDate, int ID_Designation, int mppdID, ManPowerPlanDb db)
        {
            string message = "";
            try
            {
                List<vManPowerPlanDetailsValidateResult> results = new List<vManPowerPlanDetailsValidateResult>();

                results = db.ExecQuery<vManPowerPlanDetailsValidateResult>("SELECT * FROM dbo.fValidateMPPDRecord({0}, {1}, {2}, {3})"
                                                                    , FromDate, ToDate, ID_Designation, mppdID).ToList();
                message = "";

                if (results != null)
                {
                    foreach (var msg in results)
                    {
                        message += "Position " + msg.Position + " is existing on Ref#: " + msg.RefNum.ToString() + "\n";
                    }
                }
                return message;
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "ValidateIfAlreadyExist", "Internal", "InSys.ITI.Recruitment");
                return message;
            }
        }

        public override ReturnSet DeleteRecord()
        {
            ReturnSet res = new ReturnSet();
            string message = "";

            try
            {
                using (var db = new ManPowerPlanDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tManPowerPlans>("SELECT * FROM tManPowerPlans WHERE ID = {0}", id);
                        var recordDetails = db.ExecQuery<tManPowerPlanDetails>("SELECT * FROM tManPowerPlanDetails WHERE ID_ManPowerPlans = {0}", id).ToList();
                        if (record != null)
                        {
                            db.Remove(record);
                            if (recordDetails != null)
                            {
                                foreach (var recordDetail in recordDetails)
                                {
                                    db.Remove(recordDetail);
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                }
                return new ReturnSet() { Message = "Successfully Deleted", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteManPowerPlan", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        public override ReturnSet LoadRecordDetails()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new ManPowerPlanDb(Session))
                {
                    var data = db.Single<tManPowerPlans>("SELECT * FROM tManPowerPlans WHERE ID = {0}", Parameter["ID"].IsNull(0).ToInt32());

                    var dataDetails = GetManPowerPlanDetails(data, db);
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            SchemaDetails = dataDetails
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadRecordDetails", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }

        
        //No need to override since this is not be called outside
        public virtual List<vManPowerPlanDetails> GetManPowerPlanDetails(tManPowerPlans mpp, ManPowerPlanDb db)
        {
            List<vManPowerPlanDetails> dataDetails = new List<vManPowerPlanDetails>();
            dataDetails = db.ExecQuery<vManPowerPlanDetails>("SELECT * FROM dbo.fManPowerPlanDetailsView({0}, {1}, {2})"
                , mpp.ID, mpp.FromDate, mpp.ToDate).ToList();

            return dataDetails;

        }

        public override ReturnSet LoadForm()
        {
            DateTime DefaultToDate = DateTime.Now;
            var r = new ReturnSet();
            try
            {
                using (var db = new ManPowerPlanDb(Session))
                {
                    var data = db.Single<tManPowerPlans>($"SELECT * FROM tManPowerPlans WHERE ID = {{0}} AND ID_Company IN({_System_ID_Company})", Parameter["ID"].IsNull(0).ToInt32());
                    if (data == null && Parameter["ID"].IsNull(0).ToInt32() > 0)
                    {
                        return new ReturnSet()
                        {
                            Message = "Record not found.",
                            Type = ReturnType.PageNotFound
                        };
                    }
                    var dataDetails = new List<vManPowerPlanDetails>();
                    if (data != null) dataDetails = GetManPowerPlanDetails(data, db);
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tManPowerPlans()
                            {
                                RefNum = Helpers.getReferenceNumber(tblName, _ID_Company.ToInt32()),
                                FromDate = DateTime.Now,
                                ToDate = DefaultToDate.AddDays(60),
                                ID_Company = _ID_Company,
                                IsActive = true
                            }),
                            Schema = Helpers.GetSchema(tblName),
                            SchemaDetails = dataDetails,
                            DetailSchema = Helpers.GetSchema("tManPowerPlanDetails")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadForm", Helpers.CurrentUser(Session), "InSys.ITI.Recruitment");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
    }
}
