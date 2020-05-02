using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;
using InSys.ITI.Common;

namespace InSys.ITI.LeaveParameter
{
    public class LeaveParameter : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new LeaveParameter(_Session, _Parameter);
        }
        public LeaveParameter(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public LeaveParameter() { }

        public override ReturnSet LoadList()
        {
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();

                using (var db = new LeaveParameterDb(Session))
                {
                    var LeaveParameterData = db.QueryTable<vLeaveParameterList>(
                            "(SELECT * FROM dbo.vLeaveParameterList)a", to);
                    return new ReturnSet() { Data = new { Total = LeaveParameterData.Count, Rows = LeaveParameterData.Data.ToList() }, Type = ReturnType.Result };
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
                using (var db = new LeaveParameterDb(Session))
                {
                    var data = db.Single<tLeaveParameter>("SELECT * FROM dbo.tLeaveParameter WHERE ID = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tLeaveParameter()
                            {
                                CreatedAt = DateTime.Now,
                                ID_CreatedBy = Session.ID_User
                            }),
                            Schema = Helpers.GetSchema("tLeaveParameter")
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
            var Record = Parameter["Data"].ToObject<tLeaveParameter>();
            var LeaveParameterItemToDelete = Parameter["DetailsToDelete"].ToObject<List<int>>();
            var LeaveParameterItem = Parameter["Details"].ToObject<List<tLeaveParameterItem>>();
            int RecordId = Record.ID;
            string ErrorMessage = "";
            List<string> LeaveParameterErrorMsg = new List<string>();

            try
            {
                using (var db = new LeaveParameterDb(Session))
                {
                    LeaveParameterErrorMsg.Add(ValidateLeaveParameter(Record, db));
                    LeaveParameterErrorMsg.Add(ValidateLeaveParameterItem(LeaveParameterItem, db));

                    if (LeaveParameterErrorMsg.Where(x => x != "").ToList().Count > 0) {
                        foreach (var msg in LeaveParameterErrorMsg)
                            ErrorMessage += msg;
                        
                        throw new Exception(ErrorMessage);
                    }

                    if (RecordId == 0) db.Add(Record); //New
                    else if (RecordId > 0) //Edit
                    {
                        if (db.Any("(SELECT ID FROM dbo.tLeaveParameter WHERE ID = {0})a", RecordId))
                            db.Update(Record);
                        else throw new Exception("Data does not exists");
                    }
                    db.SaveChanges(true);
                    RecordId = Record.ID;
                    //Post Leave Parameter Items
                    PostLeaveParameterItem(LeaveParameterItem, LeaveParameterItemToDelete, RecordId, db);

                }
                return new ReturnSet() { Data = RecordId, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }


        //Validation On Leave Parameter
        private string ValidateLeaveParameter(tLeaveParameter LeaveParameter, LeaveParameterDb db) {
            string returnMsg = "";
            try
            {
                if (db.Any("(SELECT Id FROM tLeaveParameter WHERE Code = {0} AND Id <> {1})a", LeaveParameter.Code, LeaveParameter.ID)) {
                    throw new Exception("Code already exists");
                }
            }
            catch (Exception ex) {
                returnMsg = ex.Message;
            }
            return returnMsg;

        } 

        private string ValidateLeaveParameterItem(List<tLeaveParameterItem> LeaveParameterItem, LeaveParameterDb db) {
            string returnMsg = "";
            string LeaveName = "";
            int counter = 0;
            try
            {
                var _LeaveParameterItem = LeaveParameterItem;
                foreach (var item in LeaveParameterItem) {
                    //validation duplicate
                    //validate negative initial value
                    //Validate Accrual Day should be positive
                    LeaveName = "";
                    if (_LeaveParameterItem.Where(x => x.ID_LeavePayrollItem == item.ID_LeavePayrollItem).ToList().Count > 1) {
                        LeaveName = db.ExecScalarString("SELECT Name AS Value FROM tPayrollItem WHERE ID = {0} AND IsForLeave = 1", item.ID_LeavePayrollItem);
                        returnMsg += StringHelper.ComposedRowMsg("Duplicate Leave Payroll Item [" + LeaveName + "] is not allowed.\n", counter + 1);
                    }

                    if (!(item.InitialValue >= 0)) {
                        returnMsg += StringHelper.ComposedRowMsg("The Initial Balance must be in positive value.\n", counter);
                    }

                    if (!(item.AccrualDay >= 0))
                    {
                        returnMsg += StringHelper.ComposedRowMsg("The Accrual Day must be in positive value.\n", counter);
                    }

                    counter += 1;

                }
            }
            catch (Exception ex)
            {
                returnMsg = ex.Message;
            }
            return returnMsg;

        }

        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new LeaveParameterDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var RecordToDelete = db.Single<tLeaveParameter>("SELECT * FROM dbo.tLeaveParameter WHERE ID = {0}", id);

                        //Do not allow system generated value
                        db.Remove(RecordToDelete);
                    }
                    db.SaveChanges();
                }

                return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual string PostLeaveParameterItem(List<tLeaveParameterItem> LeaveParameterItems, List<int> DeleteLeaveParameterItem, int ParentId, LeaveParameterDb db)
        {
            string message = "";
            try
            {
                //Delete Schedule Details
                foreach (var sd in DeleteLeaveParameterItem)
                {
                    var recordToDelete = db.Single<tLeaveParameterItem>("SELECT * FROM tLeaveParameterItem WHERE ID = {0}", sd);
                    if (recordToDelete != null) { db.Remove(recordToDelete); }
                }
                db.SaveChanges();

                foreach (var detail in LeaveParameterItems)
                {
                    var recordToUpdate = db.Single<tLeaveParameterItem>("SELECT * FROM tLeaveParameterItem WHERE ID = {0}", detail.ID);
                    detail.ID_LeaveParameter = ParentId;
                    if (recordToUpdate == null)
                        db.Add(detail);
                    else db.Update(detail);
                }
                db.SaveChanges();
                return message;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public override ReturnSet LoadLookup()
        {
            try {
                var paramName = Parameter["Name"].ToString().ToLower();
                ReturnSet ResultData = new ReturnSet();
                using ( var db = new LeaveParameterDb(Session)) {
                    switch (paramName) {
                        case ("leaveaccrualtype"):
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT Id, Name, IsActive FROM tLeaveAccrualType");
                            break;
                        case ("leaveparameteritemreferencedate"):
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tLeaveParameterItemReferenceDate");
                            break;
                        case ("accrualoption"):
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tAccrualOption");
                            break;
                        case ("leavepayrollitem"):
                            ResultData = base.LoadLookup<tLookUpData>(db, "SELECT ID, Name, IsActive FROM tPayrollItem WHERE IsForLeave = 1");
                            break;
                    }

                    return new ReturnSet() { Data = ResultData.Data, Type = ReturnType.Result };
                    

                }
            
            } catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }
        }

    }
}
