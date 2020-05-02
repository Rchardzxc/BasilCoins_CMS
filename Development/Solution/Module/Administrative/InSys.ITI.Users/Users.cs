using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Users
{
    public class Users : BaseModule
    {
        public override BaseModule Initialize(BrowserSession _Session, Pair _Parameter)
        {
            return new Users(_Session, _Parameter);
        }
        public Users(BrowserSession _Session, Pair _Parameter)
        {
            this.Session = _Session;
            this.Parameter = _Parameter;
        }
        public Users() { }
        public override ReturnSet LoadList()
        {
            string message = "";
            var res = new ReturnSet();
            try
            {
                TableOptions to = Parameter["data"].ToObject<TableOptions>();

                using (var db = new UsersDb(Session))
                {
                    var users = db.QueryTable<vUsers>("(select tu.*, tp.Name Employee, tr.Name Roles, null SecretQuestion from dbo.tUsers tu" +
                        " left join dbo.tEmployee te on tu.ID_Employee = te.ID" +
                        " left join dbo.tRoles tr on tu.ID_Roles = tr.ID" +
                        " left join dbo.tPersona tp on te.ID_Persona = tp.ID where tu.ID > 1)a", to);
                    res = new ReturnSet() { Data = new { Total = users.Count, Rows = users.Data.ToList() }, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadUsers", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                res = new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
            return res;
        }
        public override ReturnSet LoadForm()
        {
            var r = new ReturnSet();
            int ID = Parameter["ID"].ToObject<int>().IsNull(0).ToInt32();
            try
            {
                using (var db = new UsersDb(Session))
                {
                    var data = db.Single<vUsers>(@"SELECT tu.*, tp.Name Employee, tr.Name Roles, tsq.Name SecretQuestion
                                                FROM dbo.tUsers tu
                                                LEFT OUTER JOIN  dbo.tEmployee te on tu.ID_Employee = te.ID
                                                LEFT OUTER JOIN  dbo.tRoles tr on tu.ID_Roles = tr.ID
                                                LEFT OUTER JOIN  dbo.tPersona tp on te.ID_Persona = tp.ID
                                                LEFT OUTER JOIN  dbo.tSecretQuestion tsq on tu.ID_SecretQuestion = tsq.ID where tu.ID = {0} and tu.ID > 1", ID);
                    
                    //Once user open data, then set Password into nullm for security purposes
                    if (data != null) data.Password = null;

                    return new ReturnSet()
                    {
                        Data = new
                        {
                            Form = data.IsNull(new vUsers()),
                            Schema = Helpers.GetSchema("tUsers")
                        },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadForm", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public override ReturnSet Save()
        {
            ReturnSet res = new ReturnSet();
            string message = "";
            var UserData = Parameter["Data"].ToObject<tUsers>();
            int UserId = UserData.ID;

            if (UserData.ID == 1) return new ReturnSet() { Data = UserData.ID, Type = ReturnType.Result };

            try
            {
                using (var db = new UsersDb(Session))
                {
                    if (db.Any("(select * from dbo.tUsers where ID = {0})a", UserData.ID))
                    {
                        if (UserData.Password == null || UserData.Password == "")
                        {
                            UserData.Password = db.Single<tUsers>("select * from dbo.tUsers where ID = {0}", UserData.ID)?.Password;
                        }
                        else
                        {
                            UserData.Password = EncryptionHandler.EncryptUserPassword(UserData.Password, EncryptionHandler.DecryptUserKey(UserData.GUID));
                            UserData.LastPasswordChangeDate = DateTime.Now;
                        }
                        UserData.DateTimeModified = DateTime.Now;
                        UserData.ID_ModifiedBy = Session.ID_User;
                        db.Update(UserData);
                        db.SaveChanges(true);
                    }
                    else
                    {
                        UserData.GUID = EncryptionHandler.GenerateEncryptedKey();
                        if (UserData.Password == null || UserData.Password == "")
                        {
                            UserData.Password = EncryptionHandler.EncryptUserPassword(UserData.LogInName.ToLower(), EncryptionHandler.DecryptUserKey(UserData.GUID));
                        }
                        else
                        {
                            UserData.Password = EncryptionHandler.EncryptUserPassword(UserData.Password, EncryptionHandler.DecryptUserKey(UserData.GUID));
                        }
                        UserData.LastPasswordChangeDate = DateTime.Now;
                        UserData.DateTimeCreated = DateTime.Now;
                        UserData.ID_CreatedBy = Session.ID_User;
                        db.Add(UserData);
                        db.SaveChanges(true);
                    }
                    return new ReturnSet() { Data = UserData.ID, Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "SaveForm", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                res = new ReturnSet() { Message = message, Type = ReturnType.Error };
            }

            return res;
        }
        public override ReturnSet DeleteRecord()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new UsersDb(Session))
                {
                    List<int> ids = Parameter["Data"].ToObject<List<int>>();
                    foreach (int id in ids)
                    {
                        var record = db.Single<tUsers>("select * from dbo.tUsers where ID = {0} and ID > 1", id);
                        if (record != null) db.Remove(record);
                    }
                    db.SaveChanges();

                    return new ReturnSet() { Message = "Record deleted.", Type = ReturnType.Result };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "DeleteRecord", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public override ReturnSet LoadLookup()
        {
            string lookupName = Parameter["LookupName"].ToString().ToLower();
            switch (lookupName)
            {
                case "roles":
                    return LoadRoles();
                case "employee":
                    return LoadEmployee();
                default:
                    throw new Exception("Method not found.");
            }
        }

        public virtual ReturnSet LoadRoles()
        {
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new UsersDb(Session))
                {
                    var data = db.QueryTable<tRoles>("(select * from dbo.tRoles where ID > 1)a", to);
                    return new ReturnSet()
                    {
                        Data = data.Data.ToList(),
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadRoles", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadEmployee()
        {
            var r = new ReturnSet();
            try
            {
                TableOptions to = Parameter["Data"].ToObject<TableOptions>();
                using (var db = new UsersDb(Session))
                {
                    var data = db.QueryTable<tLookUpData>("(SELECT e.ID, p.Name, e.IsActive, e.ID_Company FROM tEmployee e" +
                                " INNER JOIN tPersona p ON p.ID = e.ID_Persona)a", to);
                    return new ReturnSet()
                    {
                        Data = new { Total = data.Count, Rows = data.Data.ToList() },
                        Type = ReturnType.Result
                    };
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadEmployee", Helpers.CurrentUser(Session), "InSys.ITI.Administrative");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }

    }
    public class UsersDb : InSysContext {
        public UsersDb(BrowserSession Session) : base(Session)
        {
            this.SessionContext = Session;
        }

        public DbSet<tUsers> tUsers { get; set; }
        public DbSet<tRoles> tRoles { get; set; }
        public DbSet<tSecretQuestion> tSecretQuestions { get; set; }
        public DbQuery<vUsers> vUsers { get; set; }
        public DbQuery<CountData> countDatas { get; set; }
        public DbQuery<EmployeeList> employeeLists { get; set; }
        public DbQuery<tLookUpData> tLookUpData { get; set; }
    }
}
