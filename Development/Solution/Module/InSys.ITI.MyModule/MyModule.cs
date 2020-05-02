using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.MyModule
{
    public class MyModule : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new MyModule(_Session, _Parameter);
        }
        public MyModule()
        {

        }
        public MyModule(BrowserSession _Session, Pair _Parameter)
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
                using (var db = new MyModuleDb(Session))
                {
                    var data = db.QueryTable<tMyModule>("(select * from dbo.tMyModule)a", to);
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
                using (var db = new MyModuleDb(Session))
                {
                    var data = db.Single<vMyModule>($"{Helpers.GetSqlQuery("vMyModule")} where tmm.ID = {{0}}", Parameter["ID"].IsNull(0).ToInt32());
                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new tMyModule()),
                            Schema = Helpers.GetSchema("tMyModule")
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
            try
            {
                using (var db = new MyModuleDb(Session))
                {
                    using(var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            tMyModule tmm = Parameter["Data"].ToObject<tMyModule>();
                            if (db.Any("(select * from dbo.tMyModule where ID = {0})a", tmm.ID)) db.Update(tmm);
                            else db.Add(tmm);
                            db.SaveChanges(true);

                            var dependents = Parameter["Dependents"].ToObject<List<tMyModule_Dependent>>();
                            foreach (var detail in dependents)
                            {
                                if (db.Any("(select * from dbo.tMyModule_Dependent where ID = {0})a", detail.ID)) db.Update(detail);
                                else db.Add(detail);
                            }
                            db.SaveChanges(true);
                            tran.Commit();

                            return new ReturnSet() { Data = tmm.ID, Type = ReturnType.Result };
                        }
                        catch(Exception ex)
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
        public override ReturnSet DeleteRecord()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new MyModuleDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tMyModule>("select * from dbo.tMyModule where ID = {0}", id);
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
                string lookupName = Parameter["LookupName"].ToString().ToLower();
                switch (lookupName)
                {
                    case "gender":
                        return LoadGender();
                    default:
                        throw new Exception("Lookup not found.");
                }
            }catch(Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }
        public virtual ReturnSet LoadGender()
        {
            var to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new MyModuleDb(Session))
            {
                var data = db.QueryTable<tLookUpData>("(select ID, Name, IsActive from dbo.tGender)a", to);
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
                    case "dependents":
                        return LoadDependents();
                    default:
                        throw new Exception("Detail not found.");
                }
            }catch(Exception ex)
            {
                return ExceptionLogger(ex, Session);
            }
        }

        public virtual ReturnSet LoadDependents()
        {
            TableOptions to = Parameter["data"].ToObject<TableOptions>();
            using (var db = new MyModuleDb(Session))
            {
                var data = db.QueryTable<tMyModule_Dependent>("(select * from dbo.tMyModule_Dependent where ID_MyModule = {0})a", to, Parameter["ID_MyModule"].ToInt32());
                return new ReturnSet() { Data = new { Total = data.Count, Rows = data.Data.ToList() }, Type = ReturnType.Result };
            }
        }
    }
    public class MyModuleDb : InSysContext
    {
        public MyModuleDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }
        public DbQuery<CountData> countDatas { get; set; }
        public DbSet<tMyModule> tMyModules { get; set; }
        public DbQuery<vMyModule> vMyModules { get; set; }
        public DbQuery<tLookUpData> tLookUpDatas { get; set; }
        public DbSet<tMyModule_Dependent> tMyModule_Dependents { get; set; }
    }
}
