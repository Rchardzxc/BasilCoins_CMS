using InSys.Context;
using InSys.Helper;
using InSys.ITI.Controller;
using InSys.ITI.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using z.Data;

namespace InSys.ITI.Login
{
    public class Login
    {
        private BrowserSession Session { get; set; }
        private Pair param { get; set; }
        public Login(Pair _param, BrowserSession _Session)
        {
            this.param = _param;
            this.Session = _Session;
        }

        public virtual ReturnSet Authenticate()
        {
            string Message = null;
            UserLoginModel userLogin = param["Data"].ToObject<UserLoginModel>();
            try
            {
                using (var db = new LoginDb(null))
                {
                    var user = db.Single<tUsers>("select * from dbo.tUsers where LogInName = {0} and IsActive = 1", userLogin.Username);
                    //check if user exist
                    if (user == null)
                    {
                        Message = "Invalid username or password.";
                        return new ReturnSet() { Message = Message, Type = ReturnType.Error };
                    }

                    //check if password match
                    DateTime current = DateTime.Now;
                    if (EncryptionHandler.VerifyHash(userLogin.Password, user.Password, EncryptionHandler.DecryptUserKey(user.GUID)))
                    {
                        //var wow = EncryptionHandler.DecryptUserKey(user.GUID);
                        //check if user is blocked
                        if (user.BlockedDate != null)
                        {
                            TimeSpan ts = (TimeSpan)(user.BlockedDate - current);
                            if (ts.TotalSeconds > 0)
                            {
                                return new ReturnSet() { Data = new { Seconds = ts.TotalSeconds.ToInt32() }, Message = Message, Type = ReturnType.Blocked };
                            }
                        }

                        //check if user first login
                        if (user.IsFirstLog.ToBool())
                        {
                            return new ReturnSet() { Data = new { ID = user.ID }, Type = ReturnType.FirstLogin };
                        }

                        //check if password expired
                        if (user.LastPasswordChangeDate != null)
                        {
                            TimeSpan ts2 = (TimeSpan)(current - user.LastPasswordChangeDate);
                            if (ts2.TotalDays.ToInt32() > SystemSettings.GetSettingValue("PasswordDuration").IsNull(30).ToInt32())
                            {
                                return new ReturnSet() { Data = new { ID = user.ID }, Type = ReturnType.ChangePassword };
                            }
                        }

                        //if user login success
                        return new ReturnSet() { Data = new { ID = user.ID, Authorize = true }, Message = $"Welcome {user.LogInName}!", Type = ReturnType.Result };
                    }
                    else
                    {
                        if (user.BlockedDate != null)
                        {
                            TimeSpan ts = (TimeSpan)(user.BlockedDate - current);
                            if (ts.TotalSeconds > 0)
                            {
                                return new ReturnSet() { Data = new { Seconds = ts.TotalSeconds.ToInt32() }, Message = Message, Type = ReturnType.Blocked };
                            }
                        }
                        var tmpSession = new BrowserSession() { ID_User = user.ID };
                        int invalidLogCount = SystemSettings.GetSettingValue("InvalidLogCount").IsNull(3).ToInt32();
                        if (userLogin.InvalidCount >= invalidLogCount)
                        {
                            int blockDuration = SystemSettings.GetSettingValue("BlockDuration").IsNull(3).ToInt32();
                            user.BlockedDate = DateTime.Now.AddMinutes(blockDuration);
                            using(var db2 = new LoginDb(tmpSession))
                            {
                                db2.Update(user);
                                db2.SaveChanges(true);
                            }
                            
                            TimeSpan ts = (TimeSpan)(user.BlockedDate - current);
                            ts = (TimeSpan)(user.BlockedDate - current);
                            if (ts.TotalSeconds > 0)
                            {
                                return new ReturnSet() { Data = new { Seconds = ts.TotalSeconds.ToInt32() }, Message = Message, Type = ReturnType.Blocked };
                            }
                        }
                        return new ReturnSet() { Data = new { InvalidCount = invalidLogCount }, Message = "Invalid username or password.", Type = ReturnType.WrongPassword };
                    }
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "Login", userLogin.Username, "InSys.ITI.Login");
                return new ReturnSet() { Message = message, Type = ReturnType.Error };
            }
        }
        public virtual ReturnSet SaveFirstLog()
        {
            UserAccountModel userAccount = param["Data"].ToObject<UserAccountModel>();
            string Message = null;
            string name = null;
            var r = new ReturnSet();
            try
            {
                var tmpSession = new BrowserSession() { ID_User = userAccount.ID_User };
                using (var db = new LoginDb(tmpSession))
                {
                    using (var tran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var user = db.Single<tUsers>("select * from dbo.tUsers where ID = {0}", userAccount.ID_User);
                            if (user == null)
                            {
                                Message = "Invalid user.";
                                r = new ReturnSet() { Message = Message, Type = ReturnType.Error };
                                return r;
                            }
                            else
                            {
                                name = user.LogInName;
                                if (EncryptionHandler.VerifyHash(userAccount.OldPassword, user.Password, EncryptionHandler.DecryptUserKey(user.GUID)))
                                {
                                    user.Password = EncryptionHandler.EncryptUserPassword(userAccount.NewPassword, EncryptionHandler.DecryptUserKey(user.GUID));
                                    user.ID_SecretQuestion = userAccount.ID_SecretQuestion;
                                    user.SecretAnswer = EncryptionHandler.EncryptUserPassword(userAccount.SecretAnswer, EncryptionHandler.DecryptUserKey(user.GUID));
                                    user.IsFirstLog = false;
                                    user.LastPasswordChangeDate = DateTime.Now;

                                    var password = new tPasswordHistory()
                                    {
                                        ID_User = user.ID,
                                        Password = user.Password,
                                        DateTimeCreated = DateTime.Now
                                    };
                                    db.Add(password);
                                    db.Update(user);
                                    db.SaveChanges(true);

                                    tran.Commit();

                                    Message = "Account saved!";
                                    r = new ReturnSet() { Message = Message, Type = ReturnType.Result };
                                    return r;
                                }
                                else
                                {
                                    Message = "Invalid old password.";
                                    r = new ReturnSet() { Message = Message, Type = ReturnType.Error };
                                    return r;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            Message = (ex.InnerException ?? ex).Message;
                            Logger.LogError(ref Message, "SaveAccount", name.IsNull("internal").ToString(), "InSys.ITI.Login");
                            r = new ReturnSet() { Message = Message, Type = ReturnType.Error };
                            return r;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref Message, "SaveAccount", name.IsNull("internal").ToString(), "InSys.ITI.Login");
                r = new ReturnSet() { Message = Message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet SaveAccountExpired()
        {
            var r = new ReturnSet();
            UserAccountModel userAccount = param["Data"].ToObject<UserAccountModel>();
            string Message = null;
            string name = null;
            try
            {
                var tmpSession = new BrowserSession() { ID_User = userAccount.ID_User };
                using (var db = new LoginDb(tmpSession))
                {
                    var user = db.Single<tUsers>("select * from dbo.tUsers where ID = {0}", userAccount.ID_User);
                    if (user == null)
                    {
                        Message = "Invalid user.";
                        r = new ReturnSet() { Message = Message, Type = ReturnType.Error };
                        return r;
                    }
                    else
                    {
                        name = user.LogInName;
                        string newPassword = EncryptionHandler.EncryptUserPassword(userAccount.NewPassword, EncryptionHandler.DecryptUserKey(user.GUID));
                        int PasswordUsedCnt = db.ExecQuery<tPasswordHistory>("select * from dbo.tPasswordHistory where ID_User = {0} and Password = {1}", user.ID, newPassword).ToList().Count;
                        if (PasswordUsedCnt > 0)
                        {
                            Message = "Please refrain from using old password.";
                            r = new ReturnSet() { Message = Message, Type = ReturnType.Error };
                            return r;
                        }
                        user.Password = newPassword;
                        user.LastPasswordChangeDate = DateTime.Now;

                        var password = new tPasswordHistory()
                        {
                            ID_User = user.ID,
                            Password = user.Password,
                            DateTimeCreated = DateTime.Now
                        };
                        db.Add(password);
                        db.Update(user);
                        db.SaveChanges(true);
                        Message = "Account saved!";
                        r = new ReturnSet() { Message = Message, Type = ReturnType.Result };
                        return r;
                    }
                }
            }
            catch (Exception ex)
            {
                Message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref Message, "SaveAccount", name.IsNull("internal").ToString(), "InSys.ITI.Login");
                r = new ReturnSet() { Message = Message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadCompany()
        {
            var r = new ReturnSet();
            string name = "";
            try
            {
                using (var db = new LoginDb(null))
                {
                    var user = db.Single<tUsers>("select * from dbo.tUsers where ID = {0}", param["ID"].ToInt32());
                    name = user.LogInName;
                    var companies = new List<tCompany>();
                    if (user.ID_Roles == 1)
                    {
                        companies = db.ExecQuery<tCompany>("select * from dbo.tCompany").ToList();
                    }
                    else
                    {
                        companies = db.ExecQuery<tCompany>("select tc.* from dbo.tCompany tc" +
                            " inner join dbo.tCompanyAccessRights tcar on tc.ID = tcar.ID_Company" +
                            " where tcar.ID_Roles = {0}", user.ID_Roles).ToList();

                    }
                    r = new ReturnSet() { Data = companies, Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadCompany", name.IsNull("internal").ToString(), "InSys.ITI.Login");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
        public virtual ReturnSet LoadQuestion()
        {
            var r = new ReturnSet();
            try
            {
                using (var db = new LoginDb(null))
                {
                    r = new ReturnSet() { Data = db.ExecQuery<tSecretQuestion>("select * from dbo.tSecretQuestion").ToList(), Type = ReturnType.Result };
                    return r;
                }
            }
            catch (Exception ex)
            {
                string message = (ex.InnerException ?? ex).Message;
                Logger.LogError(ref message, "LoadQuestion", "internal", "InSys.ITI.Login");
                r = new ReturnSet() { Message = message, Type = ReturnType.Error };
                return r;
            }
        }
    }

    public class LoginDb:InSysContext{
        public LoginDb(BrowserSession Session) : base(Session)
        {
        }

        public DbSet<tUsers> tUsers { get; set; }
        public DbSet<tCompany> tCompanies { get; set; }
        public DbSet<tPasswordHistory> tPasswordHistories { get; set; }
        public DbSet<tSessionUser> tSessionUsers { get; set; }
        public DbSet<tSecretQuestion> tSecretQuestion { get; set; }
    }
}
