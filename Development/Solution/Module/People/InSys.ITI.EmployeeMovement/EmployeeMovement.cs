using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;
using InSys.ITI.Common;

namespace InSys.ITI.EmployeeMovement
{
    public class EmployeeMovement : BaseModule
    {

        private string _TableName = "tEmployeeMovement";
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new EmployeeMovement(_Session, _Parameter);
        }
        public EmployeeMovement(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public EmployeeMovement() { }


        public override ReturnSet LoadList()
        {
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new EmployeeMovementDb(Session))
                {
                    var data = db.QueryTable<vEmployeeMovementList>("(SELECT * FROM dbo.vEmployeeMovementList)a", to);
                    r = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                    return r;
                }
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
                using (var db = new EmployeeMovementDb(Session))
                {
                    var data = db.Single<vEmployeeMovementForm>("SELECT * FROM dbo.fEmployeeMovementForm({0})", Parameter["ID"].IsNull(0).ToInt32());
                    if (data == null && Parameter["ID"].IsNull(0).ToInt32() != 0)
                        return new ReturnSet() { Message = "Page not found.", Type = ReturnType.PageNotFound };
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tEmployeeMovement()
                                {
                                    RefNumber = "EMV-XXXXXX",
                                    EffectivityDate = DateTime.Now,
                                    CreatedAt = DateTime.Now,
                                    ID_CreatedBy = Session.ID_User,
                                    ModifiedAt = DateTime.Now,
                                    ID_Company = Session.ID_Company
                                }),
                            Schema = Helpers.GetSchema(_TableName)
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

        public override ReturnSet LoadDetail()
        {
            var r = new ReturnSet();
            var EmpMove = Parameter["Schema"].ToObject<tEmployeeMovement>();
            string OldValueScript = "";
            try
            {
                using (var db = new EmployeeMovementDb(Session))
                {
                    var EmployeeMovementForDetail = db.ExecQuery<vEmployeeMovementFormDetail>("SELECT * FROM dbo.fEmployeeMovementFormDetail({0}, {1})", EmpMove.ID, EmpMove.ID_MovementType).ToList();
                    var RecordDetails = new List<vEmployeeMovementFormDetail>();


                    foreach (var data in EmployeeMovementForDetail)
                    {
                        if (!(EmpMove.ID_FilingStatus == 2 && EmpMove.Posted == true))
                        {
                            if (data.LookUpTable != null && data.LookUpTable != "")
                            {
                                if (data.OldValue != null)
                                {
                                    string DisplayOldValueCmd = "SELECT Name AS Value FROM " + data.LookUpTable + " WHERE Id = " + data.OldValue;
                                    data.DisplayOldValue = db.ExecScalarString(DisplayOldValueCmd);
                                }
                                else
                                {

                                    if (data.TargetTable.ToLower() == "temployee")
                                    {
                                        OldValueScript = String.Format(@"SELECT CAST({0} AS VARCHAR(1000)) AS OldValue, tmp.Name AS DisplayOldValue
                                                                    FROM tEmployee e
                                                                        INNER JOIN {1} tmp ON tmp.Id = e.{0}
                                                                    WHERE e.ID = {2}", data.FieldName, data.LookUpTable, EmpMove.ID_Employee);
                                    }
                                    else if (data.TargetTable.ToLower() == "tpersona")
                                    {
                                        OldValueScript = String.Format(@"SELECT CAST(p.{0} AS VARCHAR(1000)) AS OldValue, tmp.Name AS DisplayValue
                                                                    FROM tEmployee e
	                                                                    INNER JOIN tPersona p ON p.ID = e.ID_Persona
	                                                                    INNER JOIN {1} tmp ON tmp.ID = p.{0}
                                                                    WHERE e.ID = {2}", data.FieldName, data.LookUpTable, EmpMove.ID_Employee);
                                    }
                                } //For Null OldValue


                                //For Edit
                                if (data.NewValue != null)
                                {
                                    string DisplayNewValueCmd = "SELECT Name AS Value FROM " + data.LookUpTable + " WHERE Id = " + data.NewValue;
                                    data.DisplayNewValue = db.ExecScalarString(DisplayNewValueCmd);
                                }

                            }
                            else {
                                if (data.TargetTable.ToLower() == "temployee")
                                {
                                    OldValueScript = String.Format(@"SELECT CAST({0} AS VARCHAR(1000)) AS OldValue, CAST({0} AS VARCHAR(1000)) AS DisplayOldValue
                                                                        FROM tEmployee e
                                                                        WHERE e.ID = {1}", data.FieldName, EmpMove.ID_Employee);
                                }
                                else if (data.TargetTable.ToLower() == "tpersona")
                                {
                                    OldValueScript = String.Format(@"SELECT CAST(p.{0} AS VARCHAR(1000)) AS OldValue, CAST(p.{0} AS VARCHAR(1000)) AS DisplayValue
                                                                        FROM tEmployee e
	                                                                        INNER JOIN tPersona p ON p.ID = e.ID_Persona
                                                                        WHERE e.ID = {1}", data.FieldName, EmpMove.ID_Employee);
                                }
                            }

                            if (OldValueScript != "") {
                                var OldValueObj = db.Single<vEmployeeMovementOldValues>(OldValueScript);

                                if (OldValueObj != null)
                                {
                                    data.OldValue = OldValueObj.OldValue.ToString() ?? "";
                                    data.DisplayOldValue = OldValueObj.DisplayOldValue.ToString() ?? "";
                                }
                                else {
                                    data.OldValue = null;
                                    data.DisplayOldValue = null;
                                }

                            }
                        }
                        RecordDetails.Add(data);
                    }

                    

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = RecordDetails.IsNull(new tEmployeeMovementDetails()),
                            Schema = Helpers.GetSchema("tEmployeeMovementDetails")
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
            var r = new ReturnSet();
            string ErrorMessage = "";
            try
            {
                using (var db = new EmployeeMovementDb(Session))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        var Data = Parameter["Data"].ToObject<tEmployeeMovement>();
                        List<tEmployeeMovementDetails> Details = Parameter["Detail"].ToObject<List<tEmployeeMovementDetails>>();
                        try
                        {

                            //Validation Before Save
                            ErrorMessage = ValidateBeforeSave(db, Data);
                            if (ErrorMessage != "") throw new Exception(ErrorMessage);


                            var _data = db.tEmployeeMovements
                                    .Where(x => x.ID == Data.ID)
                                    .Select(item => new { Id = item.ID })
                                    .FirstOrDefault();

                            if (_data != null) 
                            {
                                Data.ID_ModifiedBy = Session.ID_User;
                                Data.ModifiedAt = DateTime.Now;
                                db.Update(Data);
                            }
                            //Create for New record.
                            else
                            {
                                Data.ID_CreatedBy = Session.ID_User;
                                Data.CreatedAt = DateTime.Now;
                                Data.RefNumber = Helpers.getReferenceNumber(_TableName, Session.ID_Company.ToInt32());
                                Data.ID_Company = Session.ID_Company.ToInt32();
                                db.Add(Data);
                                db.SaveChanges(true);
                                Helpers.updateReferenceNum(_TableName, Session.ID_Company.ToInt32(), Session);
                            }
                            
                            foreach (var detail in Details) {
                                var Id_EmployeeMovement = db.tEmployeeMovementDetails
                                    .Where(x => x.ID == detail.ID)
                                    .Select(item => new { Id = item.ID })
                                    .FirstOrDefault();

                                detail.ID_EmployeeMovement = Data.ID;
                                if (Id_EmployeeMovement != null) {
                                    db.Update(detail);
                                } else db.Add(detail);
                            }
                            db.SaveChanges();
                            tran.Commit();
                        }
                         catch (Exception ex)
                        {
                            tran.Rollback();
                             throw ex;
                        }
                        return new ReturnSet() { Data = Data.ID, Type = ReturnType.Result };
                    }
                    
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet DeleteRecord()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new EmployeeMovementDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var Record = db.tEmployeeMovements.AsNoTracking().Single(x => x.ID == id);
                        var RecordDetails = db.tEmployeeMovementDetails.AsNoTracking().Where(x => x.ID_EmployeeMovement == Record.ID).ToList();
                        if (Record != null) {
                            db.Remove(Record); //Remove Record
                            if (RecordDetails != null) db.Remove(RecordDetails); //Remove Details
                        }
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

        public override ReturnSet Post()
        {
            try
            {
                var Id = Parameter["ID"].ToInt32();
                using (var db = new EmployeeMovementDb(Session))
                {
                    db.ExecNonQuery("Update tEmployeeMovement SET Posted = 1 WHERE (Id = {0} AND Posted <> 1)", Id);
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
                using (var db = new EmployeeMovementDb(Session))
                {
                    db.ExecNonQuery("Update tEmployeeMovement SET Posted = 0 WHERE (Id = {0} AND Posted <> 0)", Id);
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
                var Id = Parameter["ID"].ToInt32();
                string Msg = "";
                using (var db = new EmployeeMovementDb(Session))
                {
                    db.ExecNonQuery(@"UPDATE tEmployeeMovement SET ID_FilingStatus = 2 WHERE (Id = {0} AND ID_FilingStatus <> 2)", Id);
                    Msg = ExecuteEmployeeMovement(2); //Execute Employee Movement
                    return new ReturnSet { Data = Id, Message = Msg, Type = ReturnType.Result };
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
                var Id = Parameter["ID"].ToInt32();
                using (var db = new EmployeeMovementDb(Session))
                {
                    db.ExecNonQuery("Update tEmployeeMovement SET ID_FilingStatus = 3 WHERE (Id = {0} AND ID_FilingStatus <> 2)", Id);
                    return new ReturnSet { Message = "Successfully Unposted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public string ExecuteEmployeeMovement(int Status) {
            string Msg = "";
            try
            {
                var Id = Parameter["ID"].ToInt32();

                //string UpdatePersona = "";
                //string UpdateEmployee = "";
                
                using (var db = new EmployeeMovementDb(Session)) {
                    var Data = db.tEmployeeMovements.Where(x => x.ID == Id).FirstOrDefault();
                    var Details = db.tEmployeeMovementDetails.Where(x => x.ID_EmployeeMovement == Id).ToList();
                    //var EmployeeMovementFields = db.ExecQuery<tEmployeeMovementFields>("SELECT * FROM tEmployeeMovementFields").ToList();

                    string DateToday = DateTime.Now.ToString("MM/dd/yyyy");
                    string DateEffective = Data.EffectivityDate.ToString("MM/dd/yyyy");

                    if (DateToday == DateEffective)
                    {
                        db.ExecNonQuery("EXEC pApplyEmployeeMovement {0}", Session.ID_User);
                        Msg = "Successfully Updated.";
                    }
                }
            }
            catch (Exception ex) {
                //return ExceptionLogger(ex, Session);
                Msg = ex.Message;
            }
            return Msg;

        }
        
        public override ReturnSet LoadLookup()
        {
            try {
                string lookupName = Parameter["LookupName"].ToString().ToLower();
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                return CommonHelper.GetLookUp(lookupName, to, Session);
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        /// <summary>
        /// Check Duplcate RefNumber
        /// Check Duplicate Employee, Effectivity Date & Movement Type
        /// </summary>
        /// <param name="db"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ValidateBeforeSave(InSysContext db, tEmployeeMovement data) {
            string ErrorMessage = "";
            try {
                //Check duplicate values.
                if (db.Any("(SELECT ID FROM tEmployeeMovement WHERE RefNumber = {0} AND Id <> {1})a", data.RefNumber, data.ID))
                    throw new Exception("RefNumber already exists. Please report to your administrator.");

                if (db.Any(@"(SELECT ID
                            FROM tEmployeeMovement
                            WHERE ID_Employee = {0}
                                AND ID_MovementType = {1}
                                AND FORMAT(EffectivityDate, 'MM/dd/yyyy') = {2}
                                AND ID <> {3})a", data.ID_Employee, data.ID_MovementType, data.EffectivityDate.ToString("MM/dd/yyyy"), data.ID)
                    ) throw new Exception("Employee, Movement Type and Effectivity Date already exists.");
            }
            catch (Exception ex) {
                ErrorMessage = ex.Message;
            }
            return ErrorMessage;

        }


    }
    
}
