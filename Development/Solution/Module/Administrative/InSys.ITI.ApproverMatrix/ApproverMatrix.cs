using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Dto;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using z.Data;

namespace InSys.ITI.ApproverMatrix
{
    public class ApproverMatrix:BaseModule
    {
        public ApproverMatrix(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public ApproverMatrix()
        {

        }

        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new ApproverMatrix(_Session,_Parameter);
        }

        public override ReturnSet LoadList()
        {
            ReturnSet res = new ReturnSet();

            TableOptions to = Parameter["Data"].ToObject<TableOptions>();

            try
            {
                using (var db = new ApproverMatrixDB(Session))
                {
                    var data = db.QueryTable<tApprover>("(SELECT * FROM dbo.tApprover)a", to);
                    res = new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex,Session);
            }

            return res;
        }
        public override ReturnSet Save()
        {
            try
            {
                using (var db = new ApproverMatrixDB(Session))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var ta = Parameter["Data"].ToObject<tApprover>();
                            var FilingModuleApprovers = Parameter["FilingModuleApprovers"].ToObject<List<tFilingModuleApprovers>>();
                            var _FilingModuleApprovers = db.tFilingModuleApprovers.Where(x => x.ID_Approver == ta.ID).AsNoTracking().ToList();

                            db.Update(ta);
                            db.SaveChanges(true);

                            //delete data
                            foreach (var data in _FilingModuleApprovers)
                            {
                                //Check if filing module exists on new filing module approvers (came from client respond)
                                var _data = FilingModuleApprovers.Where(x => x.ID == data.ID).FirstOrDefault();
                                if (_data == null) db.Remove(data);
                            }

                            //Add Or Update
                            foreach (var data in FilingModuleApprovers) {
                                data.ID = data.ID == 0? null : data.ID; // Use to track changes                               
                                var _data = _FilingModuleApprovers.Where(x => x.ID == data.ID).FirstOrDefault();
                                data.ID_Approver = ta.ID;

                                if (_data != null)
                                {
                                    db.Update(data);
                                }
                                else db.Add(data);
                            }


                            //Post Filing Employees
                            PostApproverMatrixTemplate(ta.ID, db);

                            db.SaveChanges(true);
                            tran.Commit();

                            return new ReturnSet { Data = ta.ID, Type = ReturnType.Result };
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            return ExceptionLogger(ex, Session);
                        }
                    }
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

                using (var db = new ApproverMatrixDB(Session))
                {
                    var ID = Parameter["ID"].IsNull(0).ToInt32();

                    List<vFilingModules> _FilingModules = new List<vFilingModules>();
                    List<vFilingModuleApprovers> _ApproverFilingModules = new List<vFilingModuleApprovers>();

                    tApprover _Approver = new tApprover();                    
                    _Approver = db.tApprover.Where(x => x.ID == ID).FirstOrDefault();
                    _FilingModules = db.ExecQuery<vFilingModules>(@"SELECT fm.ID, fm.ID_Menus, m.Name, fm.IsActive
                                                                    FROM tFilingModules fm
                                                                        INNER JOIN tMenus m ON m.Id = fm.ID_Menus ORDER BY OrderNumber").ToList();
                    _ApproverFilingModules = db.ExecQuery<vFilingModuleApprovers>("SELECT * FROM dbo.fFilingModuleApprovers({0})", ID).ToList();
                    var _FilingModuleApproverSchema = Helpers.GetSchema("fFilingModuleApprovers");

                       //db.ExecQuery<tFilingModules>(Helpers.GetSqlQuery("vApprover_Module").BuildParameter(null, record.ID)).ToList();
                       //if (record != null) {
                       //    filingModules = db.ExecQuery<tFilingModules>(Helpers.GetSqlQuery("vApprover_Module").BuildParameter(null, record.ID)).ToList();
                       //}
                       //var approverEmployees = db.ExecQuery<vApprover_Employees>(@"SELECT  ae.ID,
                       //ae.ID_Approver,
                       //p.Name,
                       //cast(1 as bit) IsChecked,
                       //ae.ID_Employee
                       //FROM dbo.tApprover_Employees ae
                       //INNER JOIN dbo.tEmployee e ON e.ID = ae.ID_Employee
                       //LEFT JOIN dbo.tPersona p ON p.ID = e.ID_Persona
                       //WHERE ae.ID_Approver = {0}",ID).ToList();

                    return new ReturnSet {
                        Data = new 
                        { 
                            Form = _Approver.IsNull(new tApprover()),
                            Schema = Helpers.GetSchema("tApprover"),
                            FilingModules = _FilingModules,
                            AllEmployees = _ApproverFilingModules,
                            FilingModuleApproverSchema = _FilingModuleApproverSchema
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex,Session);
            }
        }
        public override ReturnSet DeleteRecord()
        {
            return base.DeleteRecord();
        }
       
        //Load Approver List
        public virtual ReturnSet LoadApproverList()
        {
            var ret = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new ApproverMatrixDB(Session))
                {
                    var employees = db.QueryTable<vEmployeeRecordList>(@"(SELECT * FROM dbo.fEmployeeRecordList({0}))a", to, Session.ID_Company);
                    ret = new ReturnSet() 
                    { 
                        Data = new {
                            Total = employees.Count,
                            Rows = employees.Data.ToList()
                        }, 
                        Type = ReturnType.Result 
                    };
                    return ret;
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadApproverSchema()
        {
            var ret = new ReturnSet();
            try
            {
                using (var db = new ApproverMatrixDB(Session))
                {
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = new vApprover_Default(),
                            Schema = Helpers.GetSchema("tApprover_Default")
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
        public virtual ReturnSet LoadEmployees() {
            try
            {
               
                var to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new ApproverMatrixDB(Session)) 
                {
                    var EmployeeLists = db.QueryTable<vEmployeeRecordList>("(SELECT * FROM dbo.fEmployeeRecordList({0}))a", to, Session.ID_Company);
                    return new ReturnSet
                    {
                        Data = EmployeeLists,
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex,Session);
            }
        }
        public virtual ReturnSet PostApproverMatrixTemplate(int _ID_Approver, ApproverMatrixDB db) 
        {
            try
            {
                var FilingModuleEmployees = Parameter["FilingModuleEmployees"].ToObject<List<vEmployeeRecordList>>();
                var RemoveFilingModuleEmployees = Parameter["RemoveFilingModuleEmployees"].ToObject<List<int>>();
                var _FilingModuleEmployees = FilingModuleEmployees.Select(x => new tFIlingModuleEmployees { ID_Approver = _ID_Approver, ID_Employee = x.ID }).ToList();
                var DbFilingModuleEmployees = db.tFIlingModuleEmployees.Where(x => x.ID_Approver == _ID_Approver).ToList();

                //delete record
                foreach (var id in RemoveFilingModuleEmployees) {
                    var deletedRecord = DbFilingModuleEmployees.Where(x => x.ID_Employee == id && x.ID_Approver == _ID_Approver).FirstOrDefault();
                    if (deletedRecord != null) db.Remove(deletedRecord);
                }

                //Add new record
                foreach (var data in _FilingModuleEmployees)
                {
                    var _data = DbFilingModuleEmployees.Where(x => x.ID_Approver == data.ID_Approver && x.ID_Employee == data.ID_Employee).FirstOrDefault();
                    if (_data == null)
                    {
                        db.Add(data);
                    }
                }
                db.SaveChanges();
                return new ReturnSet { Data = _ID_Approver, Type = ReturnType.Result };
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
                
        }
        
        public override ReturnSet ExecuteAction()
        {
            try
            {
                string MethodName = Parameter["MethodName"]?.ToString();
                switch (MethodName.ToLower())
                {
                    case "loadapproverlist":
                        return LoadApproverList();
                    case "loadfilingmoduleemployeelist":
                        return LoadFilingModuleEmployeeList();
                    case "postapprovermatrixtemplate":
                        ReturnSet ReturnValue = new ReturnSet();
                        int _ID_Approver = Parameter["ID_Approver"].IsNull(0).ToInt32();

                        using (var db = new ApproverMatrixDB(Session)) {
                            ReturnValue = PostApproverMatrixTemplate(_ID_Approver, db);
                        }
                        return ReturnValue;
                    default:
                        throw new Exception("Method not found");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual ReturnSet LoadFilingModuleEmployeeList() {
            try
            {
                var ID_Approver = Parameter["ID_Approver"].ToObject<int>();
                var to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new ApproverMatrixDB(Session)) {
                    var data = db.ExecQuery<vEmployeeRecordList>(@"SELECT fr.* FROM dbo.fEmployeeRecordList({0}) fr
                                                            INNER JOIN tFilingModuleEmployees fme ON fme.ID_Employee = fr.ID
                                                            WHERE fme.ID_Approver = {1}", Session.ID_Company, ID_Approver).ToList();

                    return new ReturnSet() {
                        Data = data,
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex) {
                return ExceptionLogger(ex, Session);
            }


        }
    }
}
