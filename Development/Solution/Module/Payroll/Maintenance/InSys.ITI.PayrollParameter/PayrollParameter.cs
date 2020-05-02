using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.PayrollParameter
{
    public class PayrollParameter : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new PayrollParameter(_Session, _Parameter);
        }
        public PayrollParameter()
        {
        }
        public PayrollParameter(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public override ReturnSet LoadList()
        {
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();
                using (var db = new PayrollParameterDb(Session))
                {
                    var data = db.QueryTable<vParameter>(@"(select p.*, c.Name Company, tc.Name AS TaxComputation from dbo.tParameter p 
                                                            LEFT JOIN dbo.tTaxComputation tc ON p.ID_TaxComputation = tc.ID 
                                                            LEFT JOIN dbo.tCompany c ON p.ID_Company = c.ID)a", to);
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
            try
            {
                var dataID = Parameter["ID"].IsNull(0).ToInt32();
                using (var db = new PayrollParameterDb(Session))
                {
                    //var Parameter = db.tParameters.Where(x => x.ID == dataID).SingleOrDefault();
                    var Parameter = db.Single<vParameter>($"{Helpers.GetSqlQuery("vParameter")} where p.ID = {{0}}", dataID);
                    //db.Single<vParameter>($"{Helpers.GetSqlQuery("vParameter")} where p.id = {0}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = Parameter.IsNull(new vParameter() { DateTimeCreated = DateTime.Now, ID_Company = Session.ID_Company, Company = Session.Company}),
                            Schema = Helpers.GetSchema("tParameter")
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
                using (var db = new PayrollParameterDb(Session))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            tParameter tp = Parameter["Data"].ToObject<tParameter>();
                            int ID = tp.ID; //check if new
                            if (db.Any("(select * from dbo.tParameter where ID = {0})a", tp.ID)) db.Update(tp);
                            else db.Add(tp);
                            db.SaveChanges(true);

                            var payrollItemRate = Parameter["PayrollItemRate"].ToObject<List<tPayrollItemRate>>();
                            foreach ( var detail in payrollItemRate)
                            {
                                if (db.Any("(select * from dbo.tPayrollItemRate where ID = {0})a", detail.ID)) db.Update(detail);
                                else db.Add(detail);
                            }
                            db.SaveChanges(true);

                            if (ID < 1) //if new                               {string.Join(",", payrollItemRate.Select(x => x.ID_PayrollItem).ToList())}
                            {
                                var epi =String.IsNullOrWhiteSpace( string.Join(", ", payrollItemRate.Select(x => x.ID_PayrollItem).ToList())) ? "0" : string.Join(", ", payrollItemRate.Select(x => x.ID_PayrollItem).ToList());
                                var defaultPayrollItem = db.ExecQuery<tPayrollItem>($"select * from dbo.tPayrollItem where ID BETWEEN 1 AND 44 AND ID not in ({epi})").ToList(); //payroll id from 1 to 44 default
                                foreach (var pi in defaultPayrollItem)
                                {
                                    tPayrollItemRate tpr = new tPayrollItemRate()
                                    {
                                        ID = 0,
                                        ID_Parameter = tp.ID,
                                        ID_PayrollItem = pi.ID,
                                        IsActive = pi.IsActive
                                    };
                                    db.Add(tpr);
                                }
                                if (defaultPayrollItem.Count > 0) db.SaveChanges();
                            }
                                tran.Commit();

                            return new ReturnSet() { Data = tp.ID, Type = ReturnType.Result };
                        }
                        catch (Exception ex) 
                        {
                            tran.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet DeleteRecord()
        {
            try
            {
                using (var db = new PayrollParameterDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tParameter>("select * from dbo.tParameter where id = {0}", id);
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
        public override ReturnSet LoadLookup()
        {
            try
            {
                string lookupName = Parameter["lookupName"].ToString().ToLower();
                 switch (lookupName)
                {
                    case "taxcomputation":
                        return LoadTaxComputation();
                    case "company":
                        return LoadCompany();
                    case "payrollitem":
                        return LoadPayrollItem();
                    default:
                        throw new Exception("Lookup Not found.");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadTaxComputation()
        {
            var to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new PayrollParameterDb(Session))
            {
                var data = db.QueryTable<tLookUpData>("(select ID, Name, isActive from dbo.tTaxComputation where isActive = 1)a", to);
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }
        public virtual ReturnSet LoadCompany()
        {
            var to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new PayrollParameterDb(Session))
            {
                var data = db.QueryTable<tLookUpData>("(select ID, Name, isActive from dbo.tCompany where isActive = 1)a", to);
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }
        public virtual ReturnSet LoadPayrollItem() 
        {
            var to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new PayrollParameterDb(Session)) 
            {
                var data = db.QueryTable<tLookUpData>("(select ID, Name, IsActive from dbo.tPayrollItem where isActive = 1)a", to);
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }
        public override ReturnSet LoadDetail()
        {
            string DetailName = Parameter["DetailName"].ToString().ToLower();
            try
            {
                switch (DetailName) 
                {
                    case "payrollitemrate":
                        return LoadPayrollItemRate();
                    default:
                        throw new Exception("Detail not found.");
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public override ReturnSet DeleteDetail()
        {
            try
            {
                using (var db = new PayrollParameterDb(Session)) 
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var recordToDelete = db.Single<tPayrollItemRate>("select * from dbo.tPayrollItemRate where id = {0}", id);
                        if (recordToDelete != null) db.Remove(recordToDelete);
                    }
                    db.SaveChanges();

                    return new ReturnSet() { Message = "Record/Records deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadPayrollItemRate() 
        {
            TableOptions to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new PayrollParameterDb(Session))
            {
                var data = db.QueryTable<vPayrollItemRate>(@"(select pir.*, pi.name PayrollItem from dbo.tPayrollItemRate pir
                                                            LEFT JOIN dbo.tPayrollItem pi ON pir.ID_PayrollItem = pi.ID 
                                                            where ID_Parameter ={0})a", to, Parameter["ID_Parameter"].ToInt32());
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }
    }
    
    public class PayrollParameterDb : InSysContext
    {
        public PayrollParameterDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbQuery<CountData> countDatas { get; set; }
        public DbSet<tParameter> tParameters { get; set; }
        public DbQuery<vParameter> vParameter { get; set; }
        public DbQuery<tLookUpData> tLookUpDatas { get; set; }
        public DbSet<tPayrollItemRate> tPayrollItemRates { get; set; }
        public DbQuery<vPayrollItemRate> vPayrollItemRates { get; set; }
        public DbSet<tPayrollItem> tPayrollItems { get; set; }
    }
}
