using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.LeaveType
{
    public class LeaveType : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new LeaveType(_Session, _Parameter);
        }
        public LeaveType(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public LeaveType() {}

        public override ReturnSet LoadList() {
            try
            {
                var options = Parameter["data"].ToObject<TableOptions>();
                using (var db = new LeaveTypeDb(Session))
                {
                    var data = db.QueryTable<vLeaveType>(@"(SELECT lt.ID, lt.Code, lt.Name, lt.IsActive, lt.Remarks,
                                                            lt.ShowInIONS, lt.ID_Company, lag.Name AS ApplicableGender
                                                        FROM tLeaveType lt
                                                            LEFT OUTER JOIN tLeaveApplicableGender lag ON lag.ID = lt.ID_LeaveApplicableGender
                                                        WHERE lt.ID_Company = {0})a", options, Session.ID_Company);
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
                using (var db = new LeaveTypeDb(Session))
                {
                    var record = db.Single<tLeaveType>(@"SELECT * FROM tLeaveType WHERE ID = {0}", id);
                    return new ReturnSet
                    {
                        Data = new
                        {
                            Form = record.IsNull(new tLeaveType() { 
                                ID_Company = Session.ID_Company.ToInt32(),
                                ID_CreatedBy = Session.ID_User,
                                ID_ModifiedBy = Session.ID_User
                            }),
                            TableSchema = Helpers.GetSchema("tLeaveType")
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

        public override ReturnSet Save() {
            var LeaveTypeData = Parameter["Data"].ToObject<tLeaveType>();
            int LeaveTypeId = LeaveTypeData.ID;
            int ReturnId = 0;
            try
            {
                using (var db = new LeaveTypeDb(Session))
                {
                    //New Data
                    if (LeaveTypeId == 0)
                    {
                        db.Add(LeaveTypeData);
                        db.SaveChanges(true);
                        ReturnId = LeaveTypeData.ID;
                    }
                    //Edit
                    else if (LeaveTypeId > 0)
                    {
                        var LeaveTypeToUpdate = db.Single<tLeaveType>("select * from dbo.tLeaveType where ID = {0}", LeaveTypeId);

                        if (LeaveTypeToUpdate != null)
                        {
                            db.Update(LeaveTypeData);
                            db.SaveChanges(true);
                            ReturnId = LeaveTypeData.ID;
                        }
                        else { throw new Exception("Data does not exists"); }
                    }
                }

                return new ReturnSet() { Data = ReturnId, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public override ReturnSet DeleteRecord()
        {
            try {
                var ids = Parameter["data"].ToObject<List<int>>();
                using (var db = new LeaveTypeDb(Session)) {
                    if (ids.Count > 0)
                    {
                        foreach (var id in ids)
                        {
                            var rec = db.tLeaveType.SingleOrDefault(x => x.ID == id);
                            if (rec != null) db.Remove(rec);
                        }
                    }
                    db.SaveChanges();
                    return new ReturnSet() { Message = "Successfully Deleted.", Type = ReturnType.Result };
                }
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }


        public override ReturnSet LoadLookup()
        {
            using (var db = new LeaveTypeDb(Session))
            {
                var name = Parameter["Name"].ToString();
                var rawRights = Helpers.CompanyRights(Session.ID_Roles);
                var companyRights = rawRights.Split(",");
                switch (name.Trim().ToLower())
                {
                    case "leaveapplicablegender":
                        return base.LoadLookup<tLookUpData>(db, $"SELECT ID, Name, CAST(1 AS BIT) AS IsActive FROM tLeaveApplicableGender");
                    default:
                        var msg = $"{name} is not available on your lookup.";
                        Logger.LogError(ref msg, "LoadLookup", Session.Name, "InSys.ITI.LeaveType");
                        throw new Exception("System Error! Please contact your System Administrator");
                }
            }
        }



    }
}
